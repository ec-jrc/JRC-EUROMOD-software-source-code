/*#####################################################
#  Version 1.1.1
#  Author: Hannes Serruys
#  Last updated: 11/30/2025
#####################################################*/
global EUROMOD_COMMAND_VERSION = "1.1.0"
global EUROMOD_CONNECTOR_VERSION = "3.7.10"


if "$EUROMOD_PATH" == "" {
	global EUROMOD_PATH = "C:/Program Files/EUROMOD/Executable"
}
capture confirm file "${EUROMOD_PATH}/stataplugin.plugin"

// Check the return code to determine if the file exists
if _rc != 0 {
	display as error "Expected to find a stataplugin on the path: ${EUROMOD_PATH}. Make sure to have downloaded the latest version of EUROMOD. (minimum version $EUROMOD_CONNECTOR_VERSION )"
	exit _rc
}
program define euromod
	
	
    gettoken first rest : 0, parse(", ")
       if "`first'" == "run" {
			local subcommand = "euromod_run"
	   } 
	   else if "`first'" == "getdata" {
			local subcommand = "euromod_getdata"
	   }
	   else if "`first'" == "getinfo" {
			local subcommand = "euromod_getinfo"
	   }
	   else if "`first'" == "setinfo" {
			local subcommand = "euromod_setinfo"
	   }
	   else if "`first'" == "getmodel" {
			local subcommand = "euromod_getmodel"
	   }
	   else {
			di as err "No valid subcommand specified."
			error -1
	   }
       `subcommand'`rest'
	   exit 0
end