"""Re-execute the documentation notebooks and commit their outputs.

``nb_execution_mode`` is ``"off"``: the notebooks ship with outputs already in
them, because a CI runner has no EUROMOD model. Run this locally whenever the
API changes, then commit the refreshed notebooks.

    set EUROMOD_MODEL_PATH=C:\\EUROMOD_RELEASES
    python docs/execute_notebooks.py

The notebooks read ``EUROMOD_MODEL_PATH`` themselves and fall back to a generic
placeholder, so no local path is ever committed and the source needs no
rewriting between runs.

Notebooks are written back in the house JSON style (CRLF, indent=1,
ensure_ascii=False, trailing newline), which nbclient does not preserve. Keeping
that style is what stops a re-run from producing a whole-file diff.

Requires: nbformat, nbclient, and the package's own runtime dependencies.
"""

import json
import os
import sys
from pathlib import Path

import nbformat
from nbclient import NotebookClient

HERE = Path(__file__).parent
NOTEBOOKS = HERE / "notebooks"


def execute(path: Path) -> None:
    nb = nbformat.read(path, as_version=4)

    NotebookClient(nb, timeout=1800, kernel_name="python3",
                   resources={"metadata": {"path": str(path.parent)}}).execute()

    # Wall-clock timestamps would churn the diff on every run.
    for cell in nb.cells:
        cell.get("metadata", {}).pop("execution", None)

    with open(path, "w", encoding="utf-8", newline="\r\n") as f:
        f.write(json.dumps(nb, indent=1, ensure_ascii=False, default=str) + "\n")

    code_cells = [c for c in nb.cells if c.cell_type == "code"]
    outputs = sum(len(c.get("outputs", [])) for c in code_cells)
    print(f"{path.name}: {len(code_cells)} code cells, {outputs} outputs")


def main() -> int:
    if not os.environ.get("EUROMOD_MODEL_PATH", "").strip():
        print("EUROMOD_MODEL_PATH is not set; the notebooks will use their "
              "placeholder path and almost certainly fail.", file=sys.stderr)

    names = sys.argv[1:] or sorted(p.name for p in NOTEBOOKS.glob("*.ipynb"))
    for name in names:
        execute(NOTEBOOKS / name)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
