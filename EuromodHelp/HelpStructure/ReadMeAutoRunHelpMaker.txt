===========================================================================================================================
 Information about AutoRunHelpMaker.xlsm
===========================================================================================================================

 The file is used by the FunctionConfigAdministrator for automatic help generation.

 It can in fact not be opened, as it calls Application.Quit in the Workbook_Open function (i.e. autorun function).
 Thus it closes once it is opened (and after it has done its job).

 To still be able to open it, one needs to open EM_HelpSystem_Structure.xls first.
 The function usually opens this file and closes it (and itself) after it has done its job, by quitting Excel.
 If the file however was open, it shouldn't be closed and therefore Application.Quit is not called.

===========================================================================================================================
