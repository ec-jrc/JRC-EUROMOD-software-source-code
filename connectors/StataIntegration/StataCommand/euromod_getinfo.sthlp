{smcl}
{* *! version 1.1.1  1 December, 2025 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod getinfo##syntax"} {viewerjumpto "Description" "euromod getinfo####description"}{viewerjumpto "Examples" "euromod getinfo####examples"}
{viewerjumpto "Authors" "euromod getinfo####authors"}{cmd:help euromod getinfo} {it:(EUROMOD Connector v1.1.1)}

{hline}


{marker syntax}{title:Syntax}

{p 4 4 2}
{cmd:euromod getinfo}, 
{bf:model}{it:(EUROMOD system name)}  
{bf:country}{it:(EU country)} 

{title:Description}{marker description}

{pstd}{cmd:euromod_getinfo} allows the user to get information of the EUROMOD model. Depending on the options used the information included in the return list will differ. 
By default the information stored in return list will be the most specialized information. For example, when the system option is specified, information corresponding to the country or model will not be included in the return list. 
This default behaviour can be overwritten by using the option {bf:all}.{p_end}  

{pstd}{opt model(string)} This parameter is the root folder of the EUROMOD project, where the model is stored. This parameter is mandatory. When one only passes this parameter to the euromod_getinfo command, the command stores a list of countries available in the model in the return list.{p_end}

{p 8 4 2} Using this option can add the following information to the {bf:r() return list}.  {p_end}

{txt}{center:{hline 130}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 100: {bf:Description}}{txt}}
{txt}{center:{hline 130}}
{center:{txt}{lalign 30:mod_countries}{txt}{lalign 100: A space separated list representing the available country models. }}
{txt}{center:{hline 130}}

{title:Options}{marker options}
{dlgtab:Main}

{pstd}{opt country(string)} The official country code of the country of interest. For EU member states see: {p_end}

{p 8 4 2} https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Tutorial:Country_codes_and_protocol_order#Codes.2C_names_and_protocol_order_of_European_Union_.28EU.29_Member_States. {p_end}

{p 8 4 2} Using this option can add the following information to the {bf:r() return list}.  {p_end}

{txt}{center:{hline 130}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 100: {bf:Description}}{txt}}
{txt}{center:{hline 130}}
{center:{txt}{lalign 30:ctry_systems}{txt}{lalign 100: A space separated list representing the systems. }}
{center:{txt}{lalign 30:ctry_datasets}{txt}{lalign 100: A space separated list representing the datasets. }}
{center:{txt}{lalign 30:ctry_publicdatasets}{txt}{lalign 100: A space separated list representing the public datasets. }}
{center:{txt}{lalign 30:ctry_isBestMatch}{txt}{lalign 100: A space separated list which items, corresponds to ctry_allSystems and ctry_allDatasets. }}
{center:{txt}{lalign 30:}{txt}{lalign 100: Every item indicates whether system data combination is a best match. }}
{center:{txt}{lalign 30:ctry_allSystems}{txt}{lalign 100: A space separated list corresponding to ctry_isBestMatch and ctry_allDatasets. }}
{center:{txt}{lalign 30:}{txt}{lalign 100: Every item indicates the name of the system in a system dataset combination. }}
{center:{txt}{lalign 30:ctry_allDatasets}{txt}{lalign 100: A space separated list corresponding to ctry_isBestMatch and ctry_allSystems. }}
{center:{txt}{lalign 30:}{txt}{lalign 100: Every item indicates the name of the dataset in a system dataset combination. }}
{center:{txt}{lalign 30:ctry_bestMatchSystems}{txt}{lalign 100: A space separated list corresponding to ctry_bestMatchDatasets. }}
{center:{txt}{lalign 30:}{txt}{lalign 100: Every item indicates the name of the system in a best match system dataset combination. }}
{center:{txt}{lalign 30:ctry_bestMatchDatasets}{txt}{lalign 100: A space separated list corresponding to ctry_bestMatchSystems. }}
{center:{txt}{lalign 30:}{txt}{lalign 100: Every item indicates the name of the system in a best match system dataset combination. }}

{txt}{center:{hline 130}}

{pstd}{opt system(string)} Name of the system of interest {p_end}

{p 8 4 2} Using this option adds the following information to the {bf: r() return list}.  {p_end}

{txt}{center:{hline 130}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 100: {bf:Description}}{txt}}
{txt}{center:{hline 130}}
{center:{txt}{lalign 30:sys_year}{txt}{lalign 100: Year of the system. }}
{center:{txt}{lalign 30:sys_private}{txt}{lalign 100: Indicating if the system is private yes/no. }}
{center:{txt}{lalign 30:sys_name}{txt}{lalign 100: Name of the system }}
{center:{txt}{lalign 30:sys_iD}{txt}{lalign 100: ID of the system. }}
{center:{txt}{lalign 30:sys_headDefInc}{txt}{lalign 100: Income variable used to determine who is Head of a HH. }}
{center:{txt}{lalign 30:sys_currencyParam}{txt}{lalign 100: Currency of the parameters in the system. }}
{center:{txt}{lalign 30:sys_currencyOutput}{txt}{lalign 100: Currency of the output of the simulation. }}
{center:{txt}{lalign 30:sys_bestMatchDatasets}{txt}{lalign 100: Best match datasets to the system. }}
{txt}{center:{hline 130}}

{txt}{center:{hline 130}}

{pstd}{opt dataset(string)} Name of the dataset of interest {p_end}

{p 8 4 2} Using this option adds the following information to the {bf: r() return list}.  {p_end}

{txt}{center:{hline 130}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 100: {bf:Description}}{txt}}
{txt}{center:{hline 130}}
{center:{txt}{lalign 30:data_yearInc}{txt}{lalign 100: Income reference year of the dataset. }}
{center:{txt}{lalign 30:data_yearCollection}{txt}{lalign 100: Year of data collection of the dataset. }}
{center:{txt}{lalign 30:data_useCommonDefault}{txt}{lalign 100: Does EUROMOD apply a common default when a variable is not present in the dataset? yes/no  }}
{center:{txt}{lalign 30:data_iD}{txt}{lalign 100: ID of the dataset. }}
{center:{txt}{lalign 30:data_readXVariables}{txt}{lalign 100: Do expenditures variables get read? yes/no. }}
{center:{txt}{lalign 30:data_private}{txt}{lalign 100: Is the dataset private? yes/no. }}
{center:{txt}{lalign 30:data_name}{txt}{lalign 100: Name of the dataset. }}
{center:{txt}{lalign 30:data_decimalSign}{txt}{lalign 100: Decimal sign of the dataset. }}
{center:{txt}{lalign 30:data_currency}{txt}{lalign 100: Currency of the dataset. }}
{txt}{center:{hline 130}}

{pstd}{opt switchvalue} When using this option, the command requires fives additional options specified: {opt system(string)}, {opt dataset(string)}, {opt extension(string)}, {opt model(string)} and {opt country(string)}.
When provided with the correct options, the command will look up the value of the switch set for the system,dataset and extension combination and store it in the return list under the local 'switchvalue'. {p_end}

{p 8 4 2}{cmd:. euromod getinfo, model({it:Path to model}) system(BE_2020) dataset(BE_2020_c2) country(BE) extension(BTA)} {p_end}
{p 8 4 2}
This example, is for retrieving the value of the extension switch set for the system BE_2020, dataset BE_2020_c2 and BTA extension. {p_end}

{marker globals}
{title:Globals}
{pstd}Additional to the information being added to the return list. Some extra globals are being created by the Stata Connector{p_end}


{p 8 4 2} - All system dataset combinations: $EM_all_systems, $EM_all_datasets, $EM_isBestMatch. {p_end}

{p 8 4 2} - All Best Match system dataset combinations: $EM_bestmatch_systems, $EM_bestmatch_datasets {p_end}

{p 4 4 2}These macros are lists separated by spaces. The first item of the $EM_all_systems list corresponds to the first item of $EM_all_datasets and $EM_isBestMatch as portrayed in the next table: {p_end}

	{txt}{center:{hline 88}}
	{center:{txt}{lalign 13: {bf:Macro}}{txt}{center 16: {bf:Item 1}}{txt}{center 16:{bf:Item 2}}{txt}{center 16:{bf:Item 3}}{txt}{center 18:{bf:...}}}
	{txt}{center:{hline 88}}
	{center:{txt}{lalign 1:$EM_all_datasets}{txt}{center 16:BG_2021_c1}{txt}{center 16:BG_2020_c1}{txt}{center 16:BG_2019_c2}{txt}{center 18:...}}
	{center:{txt}{lalign 1:$EM_all_systems}{txt}{center 16:BG_2021}{txt}{center 16:BG_2021}{txt}{center 16:BG_2020}{txt}{center 18:...}}
	{center:{txt}{lalign 1:$EM_isBestMatch}{txt}{center 16:yes}{txt}{center 16:no}{txt}{center 16:no}{txt}{center 18:... }}
	{txt}{center:{hline 88}}



{marker examples}
{title:Examples}

{p 4 4 2}
For example, the following code snippet would loop over all possible dataset combinations for a given country, load the data into memory and run a simulation. If an error occurs in this example, it would give a warning.{p_end}

{p 8 4 2}{cmd:.euromod getinfo, model("PATH/TO/MODEL") country("BG") }{p_end}

{p 8 4 2}{cmd:.local r : word count $EM_all_datasets }{p_end}
{p 8 4 2}{cmd:.forvalues i = 1/`r'} { {p_end}
{p 10 4 2}{cmd:		local dataset : word `i' of $EM_all_datasets }{p_end}
{p 10 4 2}{cmd:		local system : word `i' of $EM_all_systems }{p_end}
{p 12 4 2}{cmd:		qui import delimited "$input_data_path`dataset'.txt",clear }{p_end}
{p 12 4 2}{cmd:		di "Generating output for `dataset' with system `system'" }{p_end}
{p 12 4 2}{cmd:		qui euromod_run, dataset(`dataset') model($model_path_new) system(`system') replace} {p_end}
{p 12 4 2}{cmd:		if $EM_n_errors > 0 } { {p_end}
{p 14 4 2}{cmd:		di in r "Warning or error has been returned for running system `system' with dataset `dataset'" }{p_end}
{p 12 4 2}	{cmd:	} }{p_end}
{p 8 4 2}} {p_end}}

{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}
