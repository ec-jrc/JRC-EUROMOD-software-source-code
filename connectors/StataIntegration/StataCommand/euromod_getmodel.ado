global EUROMOD_COMMAND_VERSION = "1.1.0"
/*#####################################################
#  Version 1.1.0
#  Author: Hannes Serruys
#  Last updated: 11/30/2025
#####################################################*/
capture program drop euromod_getmodel 
program define euromod_getmodel 
	syntax , [folder(string)]
	**# Download latest model
	di "Downloading the file from https://euromod-web.jrc.ec.europa.eu"
	mata : st_numscalar("folder_exists", direxists("`folder'"))
	if folder_exists == 0 {
		noi display as error "The folder `folder' does not exists."
		error 601
	}
	if "`folder'" == "" {
		local folder = "`c(pwd)'"
	}
	capture copy "https://euromod-web.jrc.ec.europa.eu/sites/default/files/latest_public_model_release.zip" "`folder'/latest_public_model_release.zip", replace
	if _rc != 0 {
		display as error "Latest model cannot be downloaded. Please try again later."
		error 601
	}
	local pwd = "`c(pwd)'"
	qui cd "`folder'"
	qui unzipfile "`folder'/latest_public_model_release.zip", replace
	qui cd "`pwd'"
	di "unzipped model in `folder'"
	
end