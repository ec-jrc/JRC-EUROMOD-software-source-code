"""Build one documentation site per released minor version of a package.

Used for both packages documented on this Pages site — the euromod connector
at the site root and euromod-linking under /python-linking/ — so the layout
below is relative to wherever --out points:

    <out>/              the latest released minor, copied, so the URLs that
                        were published before versioning keep working
    <out>/0.3/          each released minor, built at the highest patch tag of
    <out>/0.2/          that minor from its own source tree
    <out>/dev/          the tip of the branch being built
    <out>/versions.json
    <out>/_versions/    the switcher, hosted once per package and injected into
                        every page of every version at build time

Releases are the tags ``<tag-prefix><major>.<minor>.<patch>`` on this
repository — ``python_linking_v0.3.0``, ``python_connector_v0.3.2``. They are
created on the sync commit that carried a release across from the private
source, which is the public handle for "this is what 0.3.0 was". A page per
*minor*: patch releases document the same API, so the newest patch of each
minor is what gets built and older patches are not kept.

Each version is built from a ``git worktree`` at its tag, with that tag's own
conf.py, its own committed notebook outputs and the package installed from
that tree — so old documentation renders as it did when it was current, and
is never the old source pushed through new tooling. The toolchain itself
(sphinx, furo, myst-nb) is the caller's venv, one for every version of a
package: the docs requirements have not diverged between releases, and pinning
a toolchain per tag would mean reinstalling it per tag.

The version switcher is deliberately *not* part of either package's
documentation source. Injecting it here, into the built HTML, is what gives a
version released before the switcher existed the same switcher as the newest
one — and keeps the mechanism in one place, in the publishing pipeline, rather
than in every package's docs. The switcher finds its own ``versions.json``
from where it is served, so the two packages' switchers never see each other.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

ASSETS = Path(__file__).parent / "versions"
INJECT_MARK = "<!-- versioned-docs switcher -->"


def run(cmd: list, **kw) -> subprocess.CompletedProcess:
    print("+", " ".join(str(c) for c in cmd), flush=True)
    return subprocess.run([str(c) for c in cmd], check=True, **kw)


def released_minors(repo: Path, tag_prefix: str) -> list[tuple[str, str]]:
    """``[(minor, tag)]`` newest first — the newest tag of each minor.

    Newest by patch, then by PEP 440 post-release number: a tag such as
    ``python_connector_v0.3.2.post1`` is how a documentation-only correction
    to a released version reaches that version's pages. The site root is
    built from the latest *tag*, deliberately, so a docs fix merged to master
    would otherwise appear only under dev/ until the next software release.
    A post-release names the same software with corrected docs — which is
    precisely what PEP 440 defines it for — and needs no PyPI upload."""
    tag_re = re.compile(
        rf"^{re.escape(tag_prefix)}(\d+)\.(\d+)\.(\d+)(?:\.post(\d+))?$")
    out = subprocess.run(["git", "tag", "--list", f"{tag_prefix}*"],
                         cwd=repo, check=True, capture_output=True, text=True).stdout
    best: dict[tuple[int, int], tuple[tuple[int, int], str]] = {}
    for tag in out.split():
        m = tag_re.match(tag)
        if not m:
            print(f"  ignoring tag {tag!r}: not "
                  f"{tag_prefix}<major>.<minor>.<patch>[.post<n>]")
            continue
        major, minor, patch = (int(x) for x in m.groups()[:3])
        rank = (patch, int(m.group(4) or 0))
        if (major, minor) not in best or rank > best[(major, minor)][0]:
            best[(major, minor)] = (rank, tag)
    return [(f"{maj}.{mi}", tag) for (maj, mi), (_rank, tag) in
            sorted(best.items(), reverse=True)]


class Builder:
    def __init__(self, python: Path, package: Path, docs: Path,
                 no_deps: bool, nitpicky: bool):
        self.python, self.package, self.docs = python, package, docs
        self.no_deps, self.nitpicky = no_deps, nitpicky

    def build(self, source_tree: Path, out: Path, doctrees: Path) -> None:
        """One version's docs from one source tree into ``out``.

        The package is installed editable from the same tree first, because
        every version's conf.py reads the release string from installed
        metadata: that is how the sidebar title says 0.2.0 on the 0.2 pages
        and not whatever was installed last."""
        install = [self.python, "-m", "pip", "install", "--quiet"]
        if self.no_deps:
            install.append("--no-deps")
        run(install + ["-e", source_tree / self.package])
        cmd = [self.python, "-m", "sphinx", "-b", "html"]
        if self.nitpicky:
            cmd.append("-n")
        run(cmd + ["-d", doctrees, "-D", "nb_execution_mode=off",
                   source_tree / self.docs, out])


def build_all(repo: Path, out: Path, builder: Builder, tag_prefix: str,
              dev_name: str) -> list[dict]:
    minors = released_minors(repo, tag_prefix)
    if minors:
        print("releases:", ", ".join(f"{m} <- {t}" for m, t in minors))
    else:
        # No release tags in this checkout: a repository that has never
        # tagged a release, or a checkout made before its tags were pushed.
        # Publishing nothing would take the site down over a missing tag, so
        # the branch tip is built alone and stands in at the root — and the
        # log says so, since it is not what a released package should show.
        print(f"\n::warning::no {tag_prefix}* tags in this checkout: building "
              f"'{dev_name}' only and serving it at the root\n", flush=True)

    versions: list[dict] = []
    skipped: list[str] = []
    with tempfile.TemporaryDirectory(prefix="versioned-docs-") as tmp:
        tmp = Path(tmp)
        for i, (minor, tag) in enumerate(minors):
            wt = tmp / f"wt-{minor}"
            run(["git", "worktree", "add", "--detach", wt, tag], cwd=repo)
            try:
                builder.build(wt, out / minor, tmp / f"doctrees-{minor}")
            except subprocess.CalledProcessError as e:
                # An old release whose docs no longer build under today's
                # toolchain must not stop today's docs from publishing. It is
                # dropped from the site and said loudly; the latest release is
                # what this run exists to publish, so that one still fails.
                if i == 0:
                    raise
                print(f"\n::warning::{tag}: docs did not build ({e}); "
                      f"{minor} is left out of this deployment\n", flush=True)
                shutil.rmtree(out / minor, ignore_errors=True)
                skipped.append(tag)
                continue
            finally:
                run(["git", "worktree", "remove", "--force", wt], cwd=repo)
            versions.append({"version": minor, "tag": tag, "path": f"{minor}/",
                             "name": f"{minor} (latest)" if i == 0 else minor,
                             "latest": i == 0})
        if skipped:
            print("NOT BUILT (old releases whose docs failed):", ", ".join(skipped))

        # The branch tip: what is merged but not yet released. With no release
        # to stand at the root it is the latest there is, and is marked so.
        builder.build(repo, out / dev_name, tmp / "doctrees-dev")
        versions.append({"version": dev_name, "tag": None, "path": f"{dev_name}/",
                         "name": f"{dev_name} (unreleased)", "latest": not minors})

    # The latest release also lives at the root, so existing links and the
    # intersphinx inventory the other package points at both keep resolving.
    latest = out / (minors[0][0] if minors else dev_name)
    for item in latest.iterdir():
        dest = out / item.name
        if item.is_dir():
            shutil.copytree(item, dest, dirs_exist_ok=True)
        else:
            shutil.copy2(item, dest)
    return versions


def install_switcher(out: Path, versions: list[dict]) -> int:
    """Host the switcher once, write versions.json, and reference both from
    every page. Returns the number of pages injected.

    A subtree with its own versions.json is another package's site nested in
    this one (the connector's root holds /python-linking/), and is skipped:
    its pages get its switcher, not ours."""
    (out / "versions.json").write_text(json.dumps(versions, indent=2) + "\n",
                                       encoding="utf-8")
    assets = out / "_versions"
    assets.mkdir(exist_ok=True)
    for name in ("switcher.js", "switcher.css"):
        shutil.copy2(ASSETS / name, assets / name)

    injected = 0
    for dirpath, dirnames, filenames in os.walk(out):
        here = Path(dirpath)
        if here != out and "versions.json" in filenames:
            dirnames[:] = []
            continue
        rel = os.path.relpath(out, here).replace(os.sep, "/")
        snippet = (f'{INJECT_MARK}'
                   f'<link rel="stylesheet" href="{rel}/_versions/switcher.css">'
                   f'<script defer src="{rel}/_versions/switcher.js"></script>')
        for name in filenames:
            if not name.endswith(".html"):
                continue
            page = here / name
            html = page.read_text(encoding="utf-8")
            if INJECT_MARK in html or "</head>" not in html:
                continue
            page.write_text(html.replace("</head>", snippet + "</head>", 1),
                            encoding="utf-8")
            injected += 1
    return injected


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    ap.add_argument("--repo", type=Path, default=Path("."),
                    help="checkout of the mirror, with tags fetched")
    ap.add_argument("--out", type=Path, required=True,
                    help="directory to build into — the package's root on the site")
    ap.add_argument("--python", type=Path, default=Path(sys.executable),
                    help="interpreter of the venv holding the docs toolchain")
    ap.add_argument("--tag-prefix", required=True,
                    help="release tags are <prefix><major>.<minor>.<patch>")
    ap.add_argument("--package", type=Path, required=True,
                    help="package directory within the repo, e.g. connectors/PythonLinking")
    ap.add_argument("--docs", type=Path, default=None,
                    help="docs directory within the repo (default: <package>/docs)")
    ap.add_argument("--no-deps", action="store_true",
                    help="install the package editable without its dependencies")
    ap.add_argument("--nitpicky", action="store_true", help="pass -n to sphinx")
    ap.add_argument("--dev-name", default="dev",
                    help="directory name for the branch tip build")
    args = ap.parse_args()

    repo = args.repo.resolve()
    out = args.out.resolve()
    if out.exists():
        shutil.rmtree(out)
    out.mkdir(parents=True)

    # absolute(), not resolve(): on Linux a venv's bin/python is a symlink to
    # the base interpreter, and resolving it silently builds with a Python
    # that has no Sphinx. (Run #6, 2026-09-09: "No module named sphinx".)
    python = args.python.absolute()
    try:
        subprocess.run([str(python), "-c", "import sphinx"], check=True,
                       capture_output=True)
    except (subprocess.CalledProcessError, OSError) as e:
        sys.exit(f"{python}: not a Python with sphinx installed ({e}); pass the "
                 "docs venv's interpreter as --python")
    builder = Builder(python, args.package,
                      args.docs or args.package / "docs", args.no_deps, args.nitpicky)
    versions = build_all(repo, out, builder, args.tag_prefix, args.dev_name)
    n = install_switcher(out, versions)

    print()
    print(f"built {len(versions)} versions of {args.package} into {out}:")
    for v in versions:
        print(f"  {v['name']:<20} {v['path']:<8} {v['tag'] or '(branch tip)'}")
    print(f"switcher injected into {n} pages")


if __name__ == "__main__":
    main()
