# Installation 

## Install
The Euromod Connector can be installed from [PyPi](https://test.pypi.org/project/euromod/)  using _pip_:
```bash
$ pip install euromod
```

## Requirements
The Euromod Connector requires two [EUROMOD](https://euromod-web.jrc.ec.europa.eu "https://euromod-web.jrc.ec.europa.eu") components: 1) the model (coded policy rules) , and 2) the input microdata with the variables that respect the [EUROMOD](https://euromod-web.jrc.ec.europa.eu "https://euromod-web.jrc.ec.europa.eu") naming conventions.
For more information, please, read the sections "Model" and "Input microdata" on the [Download Euromod](https://euromod-web.jrc.ec.europa.eu/download-euromod "https://euromod-web.jrc.ec.europa.eu/download-euromod") web page.

### Python version support
Minimum Python version 3.8 required

### Windows version support
Windows 64-bit

## Dependencies

The Euromod Connector requires the following dependencies:

| Package | Minimum supported version |
| ------ | ------ |
| pandas | 2.0.3|
| pythonnet | 3.0.2 |


## Managing Errors

### <u>**1) ModuleNotFoundError**</u> or <u>**AttributeError**</u>: 
If the `import` of the Euromod Connector libreries fails displaying one of the messages below:

```python
ModuleNotFoundError: No module named 'System'
```
```python
AttributeError: module 'clr' has no attribute 'AddReference'
```
uninstall the Python _clr_ package and re-install the _pythonnet_ package:
```bash
$ pip uninstall clr
$ pip install pythonnet
```
This error is caused by a conflict between the Python _clr_ package and the _clr_ library of the _pythonnet_ package.

 ### <u>**2) RuntimeError**</u>:
If you encounter a `RuntimeError` as below, either 1) restart the kernel, or 2) open a new console window, or 3) deselect the option **User Module Reloader (UMR)** in the `Tools`-> `Preferences` -> `Python Interpreter` (or `Tools` -> `Console` -> `Advanced setting`, depending on the Python editor version) then press `Apply` and `Ok` and restart the consol windows.

**Note:** Re-enabling the UMR option has no effect on the console windows that are already open.

This error is produced when Python reloads the libraries of the _pythonnet_ package.

```python
RuntimeError: Failed to initialize Python.Runtime.dll

Failed to initialize pythonnet: System.InvalidOperationException: This property must be set before runtime is initialized
   at Python.Runtime.Runtime.set_PythonDLL(String value)
   at Python.Runtime.Loader.Initialize(IntPtr data, Int32 size)
   at Python.Runtime.Runtime.set_PythonDLL(String value)
   at Python.Runtime.Loader.Initialize(IntPtr data, Int32 size)
```

