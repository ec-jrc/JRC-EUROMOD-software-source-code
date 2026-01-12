	clear
	discard
	global EUROMOD_PATH = "C:\Users\serruha\source\EUROMOD\connectors\StataIntegration\StataPlugin\x64\Release"
	adopath ++ "C:\Users\serruha\source\EUROMOD\connectors\StataIntegration\StataCommand" // path were stata commands will be stored
	
	//global EUROMOD_PATH = "C:\Program Files\EUROMOD\Executable"
	global system  "BG_2023"
	global model_path "R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\16 - Connectors\StataConnector\EUROMOD_MASTER_VERSION_I5_180"
	global dataset "BG_2021_c1"
	global input_data_path "R:\\B2\\04 - EUROMOD JRC\\01 - Repository\\03 - Datasets\\All data\\All countries\\"
	global input_data_path_itt "R:\B2\01 - Households\08 - Indirect taxation\00 - Databases\1. Imputed databases for EUROMOD-ITT\All countries All data\"
	global EUROMOD_CONNECTOR_VERSION = "3.0.0"
	
	
	capture euromod_getinfo, model(a) country("BG")	system("BG_2023")
	assert _rc != 0
	capture euromod_getinfo, model("$model_path") country(b)	system("BG_2023")
	assert _rc != 0
	capture euromod_getinfo, model("$model_path") country("BG")	system(c)
	assert _rc != 0
	euromod_getinfo, model("$model_path") country("BG")	system("BG_2023")
	assert "`r(sys_name)'" == "BG_2023"
	capture euromod_getinfo, model("$model_path") country("BG")	system(FAULTY_NAME)
	assert _rc != 0
	euromod_getinfo, model("$model_path") country("BG")
	assert "`r(ctry_datasets)'" != ""
	assert "`r(ctry_systems)'" != ""
	assert "`r(ctry_publicdatasets)'" != ""
	euromod_getinfo, model("$model_path") country("BG")	dataset("BG_2021_c1")
	assert "`r(data_private)'" != ""
	euromod_getinfo, model("$model_path") country("BG")	dataset("BG_2021_c1")
	assert "`r(data_private)'" != ""
	euromod_getinfo, model("$model_path") country("BG") dataset("BG_2021_c")
	capture euromod_getinfo, model("$model_path"abc) country("") dataset("BG_2021_c1")
	assert _rc != 0
	di "SUCCESFULLY FINISHED TEST FOR euromod_getinfo! :)"
	import delimited "$input_data_path$dataset.txt",clear
	global ConstToChange "tscersi='0.3'"
	euromod_run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM0_")   
	su EM0_ils_sicer
	local mean0 = `r(mean)'
	euromod run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM1_")  constants($ConstToChange)
	su EM1_ils_sicer
	local mean1 = `r(mean)'
	di "`mean0' must be different then `mean1'"
	assert `mean0' != `mean1'
	
	capture euromod run, dataset($dataset) model($model_path) system($system) country(BG) prefix("EM1_")   extensions(MWA=on Test=off) //test for false syntax
	assert _rc != 0
	
	
	
	
	