/*#####################################################
#  Version 1.1.1
#  Author: Hannes Serruys
#  Last updated: 11/30/2025
#####################################################*/
global EUROMOD_COMMAND_VERSION = "1.1.0"
if "$EUROMOD_PATH" == "" {
	global EUROMOD_PATH = "C:/Program Files/EUROMOD/Executable"
}
quietly adopath + "$EUROMOD_PATH"
local wd = c(pwd)
quietly cd "$EUROMOD_PATH"
capture program drop EM_StataPlugin
program EM_StataPlugin, plugin using ("$EUROMOD_PATH/StataPlugin.plugin")
quietly cd "`wd'"

//once plugin is loaded, get software version of EUROMOD
if "$EUROMOD_SOFTWARE_VERSION" == "" {
		capture plugin call EM_StataPlugin , "setVersion"
		if (_rc != 0) {
			global EUROMOD_SOFTWARE_VERSION = "3.7.10"
		}
	}
capture program drop euromod_run
program define euromod_run, rclass
	// change working directory to path for plugin for plugin to function correctly. Problem with linking of CLR library with stata plugin, because lib file needs to be in the working directory.
	global vars_EM ""
	syntax [anything(everything)], dataset(string) [REPository(string)] * 
	if ("`repository'" != "") {
		import delimited "`repository'\\`dataset'.txt",clear
	}
	capture confirm variable idhh idperson
	if _rc != 0 | _N == 0 {
		display as error "There is either no data present or the data does not contain idperson or idhh as a variable."
		error -1
		exit
	}
    version 15.1
	syntax [if] [in], SYStem(string) DATAset(string) MODel(string) country(string) [ REPository(string)  TU_output(string) VARs_output(string) il_output(string) constants(string) PREFix(string) SETtings(string) EXTRAinfo_output(string) SUPPress_output)  uselogger outputdataset(string) EXTensions(string) ADDons(string) replace euro sequentialrun PUBLICcomponentsonly outputpath(string) keep breakfun_id(string)]
	
//DO NOT touch name of varlist_input_EM, plugin is hardcoded on the name #####
	if ("`euro'" == "euro") {
	    local settings = "`settings''FORCE_OUTPUT_EURO': 'yes',"
	}
	if ("`sequentialrun'" == "sequentialrun") {
	    local settings = "`settings''FORCE_SEQUENTIAL_RUN':'yes',"
	}
	if ("`publiccomponentsonly'" == "publiccomponentsonly") {
	    local settings = "`settings''IGNORE_PRIVATE':'yes',"
	}
	if ("`uselogger'" != "uselogger"  ) {
		local disableLogger "disableLogger"
	}
	quietly ds, has(type numeric)
	local numeric_list = "`r(varlist)'"
	quietly ds, has(char inputvar)
	local previous_input = "`r(varlist)'"
	
	quietly ds, not(char simulated) 
	local not_simulated_list = "`r(varlist)'"
	local varlist_input_EM9870 = ""
	marksample touse
	// Next foreach loop makes sure that variables which were previous output are not entering as input again 
	//(unless they were assigned to be an input variable before and overwritten by the output of a simulation)
	
	if ("`vars_output'" != "") | ("`il_output'" != "") {
		local outputdataset = "custom_output.txt"
	}
	
	foreach var in `numeric_list' {
		if  `: list var in not_simulated_list' {
			local varlist_input_EM9870 = "`varlist_input_EM9870' `var'"
			char `var'[inputvar] "yes"
		}
		else if `: list var in previous_input' {
		    
		    local varlist_input_EM9870 = "`varlist_input_EM9870' `var'"
		}	
	}
    capture noisily plugin call EM_StataPlugin `varlist_input_EM9870' if `touse', "simulate" "`system'" "`model'" "`dataset'" "`repository'" "`outputdataset'" "`country'" "`vars_output'" "`il_output'" "`extrainfo_output'" "`suppress_output'" "`constants'" "`settings'" "`disableLogger'" "`extensions'" "`addons'" "`outputpath'" "`keep'" // "`breakfun_id'"
	if (_rc != 0) {
		quietly plugin call EM_StataPlugin, "setReturnList"
		foreach local_connector in `return_list_locals_connector' {
			return local `local_connector' = "`rlm`local_connector''"
		}
		error 1
		exit
	}
	capture gen __simulated__ = 0
	replace __simulated__ = `touse'
	if ($EM_n_outputs == 1) {
		quietly euromod_getdata, outputdataset($EM_outputs) prefix(`prefix') `replace'
		return add
	}
	else if "`outputdataset'" != "" {
		euromod_getdata, outputdataset("`outputdataset'") prefix(`prefix') `replace'
		return add
	}
	else if (strpos("$EM_outputs","custom_output") > 0) {
		quietly euromod_getdata, outputdataset("custom_output.txt") prefix(`prefix') `replace'
		display "Loaded custom_output.txt by default."
		return add
	}
	else {
		quietly plugin call EM_StataPlugin, "setReturnList"
		foreach local_connector in `return_list_locals_connector' {
			return local `local_connector' = "`rlm`local_connector''"
		}
	}
	exit
end