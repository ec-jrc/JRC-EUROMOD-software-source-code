clear
discard
adopath++ "C:\Users\serruha\source\EUROMOD\connectors\StataIntegration\StataCommand"
global EUROMOD_PATH = "C:\Users\serruha\source\EUROMOD\connectors\StataIntegration\StataPlugin\x64\Release"
global system  "BG_2023"
global model_path "R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\16 - Connectors\StataConnector\EUROMOD_MASTER_VERSION_I5_180"
global dataset "BG_2023_hhot"
global input_data_path "R:\\B2\\04 - EUROMOD JRC\\01 - Repository\\03 - Datasets\\All data\\All countries\\"
global input_data_path_itt "R:\B2\01 - Households\08 - Indirect taxation\00 - Databases\1. Imputed databases for EUROMOD-ITT\All countries All data\"
import delimited "$input_data_path$dataset.txt",clear

euromod_run, dataset($dataset) model($model_path) system($system) country(BG) breakfun_id(A0290A87-229D-4373-80EA-707BBEB169DC)