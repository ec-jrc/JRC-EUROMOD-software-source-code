adopath + $EUROMOD_PATH
program StataPlugin, plugin

end
capture program drop run_euromod
program define run_euromod
	local wd = c(pwd)
	cd $EUROMOD_PATH
	syntax [anything(everything)], dataset(string) [REPository(string)] * 
	di "`repository'"
	if ("`repository'" != "") {
		import delimited "`repository'\\`dataset'.txt",clear
	}
    version 15.1
	syntax [if] [in], SYStem(string) DATAset(string) MODel(string) [ REPository(string) OUTput_data_path(string) TU_output(string) VARs_output(string) il_output(string) REPLace_const(string) PREFix(string)] 
	//do not touch name of varlist, plugin is hardcoded on the name #####
    marksample touse
	qui ds, has(type numeric)
	local varlist = "`r(varlist)'"
	di "`replace_const'"
	//#####
    capture plugin call StataPlugin `varlist' if `touse' `in', "simulate" "`system'" "`model'" "`dataset'" "`repository'" "`output_data_path'" "" "`tu_output'" "`vars_output'" "`il_output'" "`replace_const'"
	if (_rc != 0) {
		di "Something went wrong with the EM_stataPlugin. Please check the parameters that you passed to the model."
		cd wd
		exit
	}
	
	local output_vars_stata ""
	foreach var of global vars_EM {
		capture gen `prefix'`var' = .
		local output_vars_stata "`output_vars_stata' `prefix'`var'"
	}
	plugin call StataPlugin `output_vars_stata' if `touse' `in' , "store_data"
	cd wd
end