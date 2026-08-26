# Configuration file for the Sphinx documentation builder.
#
# For the full list of built-in configuration values, see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

import os

# -- Project information -----------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#project-information

project = 'Euromod Connector'
copyright = '2026 European Commission. EUROMOD is licensed under the EUPL, Version 1.2'
author = 'Belousova Irina, Serruys Hannes'

# Keep the documented release in sync with the installed package version when
# it is available; fall back to a literal for local checkouts.
try:
    from importlib.metadata import version as _pkg_version
    release = _pkg_version("euromod")
except Exception:
    release = "0.3.2"

version = release

# -- General configuration ---------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#general-configuration


extensions = [
    "myst_nb",
    # "myst_parser",
    "autoapi.extension",
    # "sphinx_copybutton",
    "sphinx.ext.napoleon",
    "sphinx.ext.viewcode",
    "sphinx.ext.intersphinx",
	#"sphinxcontrib.jquery"
]

# -- Intersphinx options
intersphinx_mapping = {
     "python": ("https://docs.python.org/3/", None),
     "numpy": ("https://numpy.org/doc/stable/", None),
     "pandas": ("https://pandas.pydata.org/docs/", None),
 }

# -- MyST-NB notebook execution ----------------------------------------------
# The example notebooks run real EUROMOD simulations, which need the model, the
# .NET engine and input microdata — none of which exist on the CI runner. We
# therefore ship the notebooks with their outputs already committed and do NOT
# re-execute them at build time. Re-run them locally (see docs/README) when the
# API changes, then commit the refreshed outputs.
nb_execution_mode = "off"

# -- Plausible support
ENABLE_PLAUSIBLE = os.environ.get("READTHEDOCS_VERSION_TYPE", "") in ["branch", "tag"]
html_context = {"enable_plausible": ENABLE_PLAUSIBLE}

# -- autoapi configuration ---------------------------------------------------

autoapi_dirs = ["../src/euromod"]  # location to parse for API reference
autoapi_type = "python"
autoapi_template_dir = "_templates/autoapi"
autoapi_options = [
    "members", # Display children of an object
    "undoc-members", # Display objects without docstrings. ??If this is removed API reference is not generated??
    # "show-inheritance", # Display a list of base classes below the class signature.
    "show-module-summary", # summary at the top
    # "imported-members" removed: it pulled re-exported names (and their
    # third-party origins) into the reference, producing noise and duplicate
    # object descriptions.
]

# Do not document internal implementation modules in the public API reference.
autoapi_ignore = ["*/calibrate/*", "*/calibrate.py", "*/test/*", "*/euromod_cli.py"]
autoapi_keep_files = True
# autoapi_generate_api_docs = False

#autoapi_python_class_content = "both" # Use the concatenation of the class docstring and the __init__ docstring.
autoapi_member_order = "groupwise"

autodoc_typehints = "signature"
autosummary_generate = True

templates_path = ['_templates']
exclude_patterns = ['changelog.md','build','_build', '../src/euromod/libs', '../src/euromod/utils','_templates', 'Thumbs.db', '.DS_Store']

# If true, '()' will be appended to :func: etc. cross-reference text.
add_function_parentheses = True

# If true, the current module name will be prepended to all description
# unit titles (such as .. function::).
add_module_names = True

# The name of the Pygments (syntax highlighting) style to use.
pygments_style = 'sphinx'

# If true, keep warnings as "system message" paragraphs in the built documents.
# keep_warnings = False

# If true, `todo` and `todoList` produce output, else they produce nothing.
todo_include_todos = False

# -- Options for HTML output -------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#options-for-html-output

html_theme = 'furo'

# Ship a small stylesheet that keeps wide notebook outputs (pandas DataFrames)
# scrolling inside their cell instead of overflowing the content area.
html_static_path = ['_static']
html_css_files = ['custom.css']

# extensions.append("sphinxjp.themes.basicstrap")
# html_theme = 'basicstrap'

# import sphinx_bootstrap_theme
# html_theme = 'bootstrap'
# html_theme_path = sphinx_bootstrap_theme.get_html_theme_path()

# html_theme="sizzle"

# html_theme = "sphinx_rtd_theme"
# html_static_path = ['_static']
# # html_css_files = [
# #     "css/custom.css",
# # ]
# html_style = 'css/theme.css' 
# The release goes in the title because that is the only place furo shows it:
# its sidebar brand template renders the title and nothing else, so a bare
# project name leaves the reader with no way to tell which version of the
# package the page describes.
html_title = f'Euromod Connector {release}'
html_short_title = 'Euromod'
html_last_updated_fmt = ''
html_use_index = True

# If false, no module index is generated.
html_domain_indices = True

# If true, links to the reST sources are added to the pages.
html_show_sourcelink = True

# The name of an image file (relative to this directory) to place at the top
# of the sidebar.
## html_logo = None


def contains(seq, item):
    """Jinja2 custom test to check existence in a container.

    Example of use:
    {% set class_methods = methods|selectattr("properties", "contains", "classmethod") %}

    Related doc: https://jinja.palletsprojects.com/en/3.1.x/api/#custom-tests
    """
    return item in seq

def prepare_jinja_env(jinja_env) -> None:
    """Add `contains` custom test to Jinja environment."""
    jinja_env.tests["contains"] = contains

autoapi_prepare_jinja_env = prepare_jinja_env


def skip_member(app, what, name, obj, skip, options):
    # Hide the internal Container.add() helpers. Match the *method* name exactly
    # ("...add") rather than any name containing the substring "add", which would
    # also hide legitimate members such as a future add_dataset().
    if what == "method" and name.rsplit(".", 1)[-1] == "add":
       skip = True
    if what == "package" and "utils" in name:
       skip = True
    if what == "package" and "libs" in name:
       skip = True
    if what == "package" and "test" in name:
       skip = True
    if what == "attribute" and "containerDict" in name:
       skip = True
    if what == "attribute" and "containerList" in name:
       skip = True
    if what == "attribute" and "idDict" in name:
       skip = True
    if what == "module" and "base" in name:
       skip = True
    if what == "module" and "info" in name:
       skip = True
    if what == "module" and "euromod_cli" in name:
       skip = True
    if what == "class" and "PolicyContainer" in name:
       skip = True
    if what == "class" and "OutputContainer" in name:
       skip = True
    if what == "class" and "FunctionContainer" in name:
       skip = True
    if what == "class" and "CountryContainer" in name:
       skip = True
    if what == "class" and "AddonContainer" in name:
       skip = True
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

