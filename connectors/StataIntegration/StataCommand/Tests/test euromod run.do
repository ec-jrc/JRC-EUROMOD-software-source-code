	clear
	discard
	global EUROMOD_PATH = "C:\Users\serruha\source\EUROMOD\connectors\StataIntegration\StataPlugin\x64\Release"
	global system  "BG_2023"
	global model_path "R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\16 - Connectors\StataConnector\EUROMOD_MASTER_VERSION_I5_180"
	global dataset "BG_2021_c1"
	global input_data_path "R:\\B2\\04 - EUROMOD JRC\\01 - Repository\\03 - Datasets\\All data\\All countries\\"
	global input_data_path_itt "R:\B2\01 - Households\08 - Indirect taxation\00 - Databases\1. Imputed databases for EUROMOD-ITT\All countries All data\"
	import delimited "$input_data_path$dataset.txt",clear
	quietly {
		noi di "test"
		euromod getinfo, model("$model_path") country("BG")
		noi di "test"
	}
	euromod_getinfo, model("$model_path") country("BG")
	di "test"
	
	euromod_run, model($model_path) system(ES_2023) dataset(ES_2021_b1) country(ES)
	
	
	euromod_run, dataset($dataset) model($model_path) system($system) country(BG)
	// example or runnning euromod with Extensions	
	euromod_run, dataset($dataset) model($model_path) system($system)  prefix("EM1_") extensions(UAA=on) country("BG") 
	
	euromod_run, dataset($dataset) model($model_path) system(lol)  prefix("EM1_") extensions(UAA=on) country("BG")
	
	su *ils_dispy

	
	// EXAMPLE with CUSTOM OUTPUT VARIABLES 
	global output_vars "yem lhw idperson"
	global il_output "ils_tax ils_dispy"
	euromod_run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM2_") vars_output($output_vars) il_output($il_output) extrainfo_output("'tu_bmaycct_bg':'IsDependentChild'")
	
	
	//RUN ITT and overwrite constant $tco_base_t_std
	global datasetItt "BG_2015_x2"
	import delimited "$input_data_path_itt/$datasetItt.txt",clear
	euromod_run, dataset($datasetItt) model($model_path) system($system) country(BG)  addons(ITT_XBASE|ITT_XBase)  extensions(XBase=on) prefix(ITT1_) outputdataset(bg_2023_xbase_std.txt)
	//euromod_getdata, outputdataset(bg_2023_xbase_std.txt) prefix(ITT1_)
	euromod_run, dataset($datasetItt) model($model_path) system($system) country(BG)  addons(ITT_XBASE|ITT_XBase)  extensions(XBase=on) prefix(ITT2_) constants("tco_base_t_std(2023) = '0.5'") 
	// Note that the next command loads the data into memory in case of multiple outputs
	euromod_getdata, outputdataset(bg_2023_xbase_std.txt) prefix(ITT2_)
	su ITT1_il_extdispy ITT2_il_extdispy
	
	
	//EXAMPLE THAT OVERRIDES MULTIPLE CONSTANTS
	global ConstToChange "flat_rate = '0.2' tintart='0.5'"
	euromod_run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM3_")  vars_output($output_vars) il_output($il_output) constants($ConstToChange)
	
	
	//example of overwriting EUROMOD's uprating factors
	euromod_run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM4_")  constants("f_hourly_wage_lindi_6(2022) = '100' f_cpi(2022)='100'")
	// You can compare different outputs
	su EM*_ils_dispy
	
	
	// EXAMPLE OF USING EUROMOD ITERATORS
	euromod_getinfo, model($model_path) country("BG")
	local r : word count $EM_bestmatch_datasets
	forvalues i = 1/`r' {
		local dataset : word `i' of $EM_bestmatch_datasets
		local system : word `i' of $EM_bestmatch_systems
			qui import delimited "$input_data_path`dataset'.txt",clear
			di "Generating output for `dataset' with system `system'"
			qui euromod_run, dataset(`dataset') model($model_path_new) country(BG) system(`system') replace
			if $EM_n_errors > 0 {
				di in r "Warning or error has been returned for running system `system' with dataset `dataset'"
			}
	}


