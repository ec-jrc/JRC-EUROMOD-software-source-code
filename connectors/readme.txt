For the Stata Plugin there might be issues if you pull this repo from github with loading the dll.
Please check if: -
- the libraries are set correctly for the linker under the properties of the StataPlugin project
- the libraries are set correctly for #using additional directories under c/c++ under properties of the StataCLR_Library project.



