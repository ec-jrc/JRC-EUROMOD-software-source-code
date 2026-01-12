# -*- coding: utf-8 -*-
"""
Created on Thu Jul 25 10:34:41 2024

@author: hannes
"""

import sys
from sphinx.cmd.build import main as sphinx_main

def build_sphinx_docs(source_dir, build_dir, builder='html'):
    """
    Build Sphinx documentation.

    :param source_dir: The path to the directory containing the conf.py file.
    :param build_dir: The path to the directory to place the built documentation.
    :param builder: The output format (e.g., 'html', 'latex').
    """
    # Clear the previous build directory
    # You might want to add extra logic to handle the build directory.

    # Sphinx arguments
    sphinx_args = [
        '-b', builder,  # Builder name.
        '-d', build_dir + '/doctrees',  # Path to the doctree directory.
        source_dir,  # Path to the documentation source files.
        build_dir + '/' + builder,  # Path to the output directory.
    ]

    # Build the documentation
    result = sphinx_main(sphinx_args)

    if result != 0:
        raise RuntimeError("Sphinx documentation build failed.")

# Example usage
if __name__ == '__main__':
    source_dir = r'C:\Users\serruha\source\EUROMOD\connectors\PythonIntegration/docs/'  # Replace with the path to your source files
    build_dir = r'C:\Users\serruha\source\EUROMOD\connectors\PythonIntegration\PythonIntegration/docs/build'    # Replace with the desired build directory
    try:
        build_sphinx_docs(source_dir, build_dir)
        print("Documentation was built successfully.")
    except RuntimeError as e:
        print(str(e))