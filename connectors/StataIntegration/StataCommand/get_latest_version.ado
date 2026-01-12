capture mata: mata drop get_latest_version()
mata: 
	void get_latest_version(string dirPath)
	{
		real col, row
		string matrix dirList

		// Get the list of directories and files
		dirList = sort(dir(dirPath,"dirs","v*.*.*"),1)


		if (rows(dirList) > 0) {
			st_local("latest_version",dirList[rows(dirList)])
		}
	}

end
mata: get_latest_version("$EUROMOD_PATH")
di "`latest_version'"
if "$EUROMOD_PATH" == "" {
	global EUROMOD_PATH = "C:/Program Files/EUROMOD/Executable"
}
if "`latest_version'" != "" {
	global EUROMOD_PATH = "$EUROMOD_PATH/`latest_version'"
}