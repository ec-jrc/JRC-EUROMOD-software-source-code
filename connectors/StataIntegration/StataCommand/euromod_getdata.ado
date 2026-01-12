/*#####################################################
#  Version 1.1.1
#  Author: Hannes Serruys
#  Last updated: 11/30/2025
#####################################################*/
global EUROMOD_COMMAND_VERSION = "1.1.0"


if "$EUROMOD_PATH" == "" {
	global EUROMOD_PATH = "C:/Program Files/EUROMOD/Executable"
}
adopath + "$EUROMOD_PATH"
local wd = c(pwd)
quietly cd "$EUROMOD_PATH"
capture program drop EM_StataPlugin
capture program EM_StataPlugin, plugin using ("$EUROMOD_PATH/StataPlugin.plugin")
quietly cd "`wd'"
capture program drop euromod_getdata
program define euromod_getdata, rclass
	set type double
	syntax , OUTputdataset(string) [replace PREfix(string)]
	capture confirm variable idperson
	if (_rc == 0) {
		capture noisily plugin call EM_StataPlugin idperson if __simulated__, "checkConcatenation" "`outputdataset'"
	} 
	//Check if data needs to be replaced before throwing an error
	if (_rc != 0) {
	    if ("`replace'" != "replace") {
	    quietly cd "`wd'"
		display in r "Output dataset cannot be concatenated."
		display in r "Consider replacing existing dataset with current one using replace option."
		quietly plugin call EM_StataPlugin, "setReturnList"
		error -1
		exit
		}
	}
	if ("`replace'" == "replace") {
		quietly ds, has(char inputvar)
		local previous_input = "`r(varlist)'"
		capture noi plugin call EM_StataPlugin, "setNobs" "`outputdataset'"
		display in r "`errorMessageEM'"
		if (_rc != 0) {
			quietly cd "`wd'"
			error -1
			exit
		}
		clear
		insobs `nObs'
		gen __simulated__ = 1
	}
	
	capture noi plugin call EM_StataPlugin if __simulated__ , "setOutputVarsMacro" "`outputdataset'"
	display in r "`errorMessageEM'"
		if (_rc != 0) {
			quietly cd "`wd'"
			error -1
			exit
		}

	local output_vars_stata ""
	foreach var of global vars_EM {
		//ensure unique varname of maximum length of 32
		local candidate = strtoname("`prefix'`var'")
		capture confirm variable `candidate'
		if _rc == 0 {
			// if exists already check first length, if necessary use substring
			local len_candidate = ustrlen("`candidate'")
			if `len_candidate' >= 30 {
				local candidate = substr("`candidate'",1,29)
			}
			//add a counter which you increase until variable name is for sure unique
			local counter = 0
			while _rc == 0 {
				local counter = `counter' + 1
				capture confirm variable `candidate'`counter'
			}
			local varname = "`candidate'`counter'"
		}
		else
		{
			local varname = "`candidate'"
		}
		capture gen `varname' = .
		quietly format `varname' %12.0g
		char `varname'[simulated] "yes"
		//set variable name of euromod as label. Add optional prefix between parenthesises
		if "`prefix'" == "" {
			label variable `varname' "`var'"
		}
		else {
			label variable `varname' "`var' (`prefix')"
		}
		
		
		local output_vars_stata "`output_vars_stata' `varname'"
	}
	// This loads the output 
	capture noi plugin call EM_StataPlugin `output_vars_stata' if __simulated__, "storeData" "`outputdataset'"
	display in r "`errorMessageEM'"
	if (_rc != 0) {
		quietly cd "`wd'"
		error -1
		exit
	}
	// Setting return List Macros
	capture noi plugin call EM_StataPlugin, "setReturnList"
	if (_rc != 0) {
		quietly cd "`wd'"
		error -1
		exit
	}
	foreach local_connector in `return_list_locals_connector' {
		return local `local_connector' = "`rlm`local_connector''"
	}
	
end