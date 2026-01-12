# Configuration file for the Sphinx documentation builder.
#
# For the full list of built-in configuration values, see the documentation:
# https://www.sphinx-doc.org/en/master/usage/configuration.html

import os

# -- Project information -----------------------------------------------------
# https://www.sphinx-doc.org/en/master/usage/configuration.html#project-information

project = 'Euromod Connector'
copyright = '2024 European Commission. EUROMOD is licensed under the EUPL, Version 1.2'
author = 'Belousova Irina, Serruys Hannes'
release = "0.2.4"

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
    "imported-members", # display objects imported from the same top level package or module
]
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
html_title = 'Euromod Connector'
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
    if what == "method" and "add" in name:
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
    return skip

def setup(sphinx):
   sphinx.connect("autoapi-skip-member", skip_member)

