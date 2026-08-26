# Configuration file for the Sphinx documentation builder.
#
# Kept deliberately close to the euromod connector's docs/conf.py so the two
# sites read as one family. The differences are noted where they occur.

import sys
from pathlib import Path

# The method-reference directive lives beside this file.
sys.path.insert(0, str(Path(__file__).parent / "_ext"))

# -- Project information -----------------------------------------------------

project = 'EUROMOD Linking'
copyright = '2026 European Commission. EUROMOD is licensed under the EUPL, Version 1.2'
author = 'Serruys Hannes'

try:
    from importlib.metadata import version as _pkg_version
    release = _pkg_version("euromod-linking")
except Exception:
    release = "0.1.0"

version = release

# -- General configuration ---------------------------------------------------

extensions = [
    "myst_nb",              # both MyST markdown and .ipynb rendering
    "autoapi.extension",
    "sphinx.ext.napoleon",
    "sphinx.ext.viewcode",
    "sphinx.ext.intersphinx",
    "sphinx_copybutton",
    "methods",             # local: docs/_ext/methods.py
]

myst_enable_extensions = ["colon_fence", "deflist"]

# -- Intersphinx options
# The euromod entry is the one that matters: this package's signatures are full
# of Model/Country/System, and without it those are dead text. Sphinx warns and
# continues when the inventory cannot be fetched, so an offline build still works.
#
# It points at the connector docs published beside this site rather than at
# readthedocs, because the two are built from the same source tree in the same
# CI run — so the inventory always describes the euromod version this package is
# documented against, which a separately-scheduled build cannot guarantee.
intersphinx_mapping = {
    "python": ("https://docs.python.org/3/", None),
    "numpy": ("https://numpy.org/doc/stable/", None),
    "pandas": ("https://pandas.pydata.org/docs/", None),
    "euromod": ("https://ec-jrc.github.io/JRC-EUROMOD-software-source-code/", None),
}
intersphinx_timeout = 10

# -- MyST-NB notebook execution ----------------------------------------------
# The notebooks load a real EUROMOD model, which does not exist on a CI runner.
# They ship with their outputs committed and are not re-executed at build time.
# Re-run them locally when the API changes, then commit the refreshed outputs:
#   set EUROMOD_MODEL_PATH=...  &&  python docs/execute_notebooks.py
nb_execution_mode = "off"

# -- autoapi configuration ---------------------------------------------------

autoapi_dirs = ["../src/euromod_linking"]
autoapi_type = "python"
autoapi_template_dir = "_templates/autoapi"
autoapi_options = [
    "members",
    "undoc-members",         # without this the API reference is not generated at all
    "show-module-summary",
    # "imported-members" is deliberately absent: it pulls re-exported names and
    # their third-party origins into the reference, producing duplicates.
]

# build/lib holds a stale copy of the package; tests are not public API.
autoapi_ignore = ["*/build/*", "*/tests/*", "*/.eggs/*"]
autoapi_keep_files = True
autoapi_member_order = "groupwise"

autodoc_typehints = "signature"
autosummary_generate = True

# `tuple[str, ...]` annotations make autoapi emit a cross-reference to Ellipsis,
# which has no documented target anywhere. Nothing to fix in the source.
nitpick_ignore = [("py:class", "Ellipsis")]

templates_path = ['_templates']
exclude_patterns = ['build', '_build', 'jupyter_execute', '_templates', '_ext',
                    'Thumbs.db', '.DS_Store', '**.ipynb_checkpoints']

add_function_parentheses = True
add_module_names = True
pygments_style = 'sphinx'
todo_include_todos = False

# -- Options for HTML output -------------------------------------------------

html_theme = 'furo'
html_static_path = ['_static']
html_css_files = ['custom.css']
# The release goes in the title because that is the only place furo shows it:
# its sidebar brand template renders the title and nothing else, so a bare
# project name leaves the reader with no way to tell which version of the
# package the page describes.
html_title = f'euromod-linking {release}'
html_short_title = 'euromod-linking'
html_last_updated_fmt = ''
html_use_index = True
html_domain_indices = True
html_show_sourcelink = True


def contains(seq, item):
    """Jinja2 custom test to check existence in a container.

    Example of use:
    {% set class_methods = methods|selectattr("properties", "contains", "classmethod") %}

    Related doc: https://jinja.palletsprojects.com/en/3.1.x/api/#custom-tests
    """
    return item in seq


def prepare_jinja_env(jinja_env) -> None:
    """Add `contains` custom test to Jinja environment.

    Required by _templates/autoapi/python/class.rst, which uses
    selectattr("properties", "contains", "property") to split properties out of
    the methods summary table."""
    jinja_env.tests["contains"] = contains


autoapi_prepare_jinja_env = prepare_jinja_env


def skip_member(app, what, name, obj, skip, options):
    """Prune the API reference.

    Nothing is pruned. Every module here is something a caller may legitimately
    reach for — `session.get_model` and `env.configure` are part of how you set
    the package up, and the `methods` subpackages carry the economics, which is
    the most useful part of the reference. autoapi already hides underscore-
    prefixed names, which is the only distinction this package draws.

    The hook stays wired so there is an obvious place to add a rule when one is
    genuinely needed."""
    return skip


def _summarylabel_role(name, rawtext, text, lineno, inliner, options=None, content=None):
    """Inline role used by the autoapi summary templates (``:summarylabel:``).

    Renders the label text in an inline node carrying the ``summarylabel`` CSS
    class. Registering it here prevents "Unknown interpreted text role" errors
    when the summary tables are rendered.
    """
    from docutils import nodes
    return [nodes.inline(rawtext, text, classes=["summarylabel"])], []


def setup(sphinx):
    from docutils.parsers.rst import roles
    roles.register_local_role("summarylabel", _summarylabel_role)
    sphinx.connect("autoapi-skip-member", skip_member)
