#pragma once
#include "../../ProcessXMLInfo/ModelInfoHandler.h"
#include "../../NativeCountryInfoHandler.h"
#include "stplugin.h"
ST_retcode get_system_info_command(int argc, char* argv[]);
ST_retcode get_dataset_info_command(int argc, char* argv[]);
ST_retcode get_country_info_command(int argc, char* argv[]);
ST_retcode setIterators(int argc, char* argv[]);
ST_retcode get_model_info_command(int argc, char* argv[]);
ST_retcode get_parameter_info_command(int argc, char* argv[]);
ST_retcode set_syspar_value_command(int argc, char* argv[]);
ST_retcode reload_model(int argc, char* argv[]);
ST_retcode get_ext_switch(int argc, char* argv[]);
bool get_model_handler(const std::string& path_model, NativeModelInfoHandler*& refModelInfoHandler);