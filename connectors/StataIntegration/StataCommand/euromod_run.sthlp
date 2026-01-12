{smcl}
{* *! version 1.1.1  1 December, 2025 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod run####syntax"} 
{viewerjumpto "Description" "euromod run####description"} 
{viewerjumpto "Options" "euromod run####options"} 
{viewerjumpto "Examples" "euromod run####examples"} 
{viewerjumpto "Requirements" "euromod run####requirements"}
{viewerjumpto "Authors" "euromod run####authors"}
{cmd:help euromod run} {it:(EUROMOD Connector v1.1.1)}

{hline}

{phang}
{bf:euromod run} {hline 2} Stata Connector to EUROMOD

{marker syntax} {title:Syntax}

{p 4 4 2}
{cmd:euromod run} 
[{help if}]{bf:,}
{bf: model }{it:(Path to model)}  
{bf:system }{it:(EUROMOD system name)} 
{bf:dataset }{it:(EUROMOD dataset)}
{bf:country }{it:(EU country)}
[{opt addons(addon name)}
{opt extensions(extension condition)}
{opt constants (constant condition)}
{opt replace}
{opt euro}
{opt prefix (prefix)}
{opt useLogger}
{opt publiccomponentsonly}
{opt repository}
{opt sequentialoutput}
{opt outputdataset (output name)}
{opt outputpath}
{opt il_output (income lists)}
{opt vars_output (variables)}
{opt extrainfo_output (Additional Information)}
{opt suppress_output}
{opt keep} ] {p_end}

{title:Description} {marker description}

{p 4 4 2} {cmd:euromod run} 
is the Stata connector, built to facilitate and simplify the usage of the EUROMOD microsimulation model for research purposes. The connector passes the data stored in Stata directly in memory to the EUROMOD software. It is therefore a prerequisite that the EUROMOD software is installed.  
The Stata connector to EUROMOD exists out of a main command euromod run and two additional commands: {p_end}  
{p 8 2 1} - {help euromod getdata}{p_end}
{p 8 2 1} - {help euromod getinfo}{p_end}

{pstd} This command will execute a run of EUROMOD using the data stored in the memory of Stata as input data. Therefore, in order for the model to be able to run, data needs to be loaded into Stata’s memory with the necessary variables included in order for the model to be able to run. 
When the run is successful, the EUROMOD output data will be loaded into the Stata DataFrame. {p_end}

{pstd}The syntax of the command is as follows: euromod run {help if}, options [extra options]. The use of “if” allows the user to pass a subset of the full DataFrame to EUROMOD, but only a specific subset. Note that one should be careful not to split households accidentally by using the if clause. {p_end}

{p 4 4 2}{opt model(string)} This parameter is the root folder of the EUROMOD project, where the model is stored.{p_end}

{p 4 4 2}{opt system(string)} This parameter represents the name of a system in EUROMOD (e.g. DK_2023) that you want to run using the connector. {p_end}

{p 4 4 2}{opt dataset(string)} This parameter is the dataset name as configured in the EUROMOD model. The parameter will determine the uprating factors and extensions to be applied to the input data. {p_end}

{p 4 4 2}{opt country(string)} The official country code of the country of interest (for EU member states see: 
https://ec.europa.eu/eurostat/statistics-explained/index.php?title=Tutorial:Country_codes_and_protocol_order#Codes.2C_names_and_protocol_order_of_European_Union_.28EU.29_Member_States) {p_end}

{pstd} Note: In case the installation path of EUROMOD is not the standard one (i.e. it is not c:\Program Files), one needs to first set the global EUROMOD_PATH before using the Stata connector. This path is where the StataPlugin.plugin is stored and should therefore always end with “Executable/”.{p_end}

{title:Options}{marker options}

{pstd}{opt repository(string)} This parameter specifies a path to a folder where there is a dataset stored with the name of the data set with the “.txt” extension. 
When using this option the data held in memory is overwritten by the data contained in the input dataset. {p_end}

{pstd}{opt addons(string)} 
This parameter takes a string which lists the names of the add-ons and their specific country implementation. {Addon}|{Addon}_{Country}.  Multiple add-ons can be used by passing them in a comma separated fashion. 
For example, integrating the of MTR and TCA add-ons for BG in the run of EUROMOD can be done with the following syntax: {p_end}

{p 8 2 1}{it:euromod run, … addon("MTR|MTR_BG, TCA|TCA_BG")}{p_end}

{pstd}{opt extensions(string)} This parameter allows to activate and deactivate global and country-specific extensions that deviate from the default configuration of the specific system-dataset combination. 
This parameter takes a comma-separated list of {ShortNameExtension}={on|off}. 
For example, activating the TCA and deactivating the UAA extensions can be done with the following syntax:{p_end}

{p 8 2 1}{it:euromod run, … extensions(UAA=on, TCA=off)}{p_end}

{pstd}{opt constants(string)} This parameter allows to overwrite the values of constants used in the model. The following types of constants can be modified: {p_end}

{p 8 2 1} - Uprating factors  {p_end}
{p 8 2 1} - Constants that are defined by DefConst {p_end}
{p 8 2 1} - Constants specific to EUROMOD’s Indirect Tax Tool (ITT) {p_end}

{p 8 4 8} The mandatory dollar sign in EUROMOD constants has to be omitted when referring to it in this option, to avoid issues with Stata global macros, which also use the dollar sign. The syntax works as follows:{p_end}

{p 8 2 1}{it:name_constant(Group Number or Year) =  'formula'}{p_end}

{p 8 4 8} Multiple constants can be overwritten by applying the syntax multiple times and separate the constants by spaces. The group number or year parameter is optional in the syntax, but is necessary for the ITT and uprating constants to identify the constants to overwrite.
For example, overwriting the $flat_rate constant in the BG system and changing the uprating factor $f_cpi would works as follows: {p_end}

{p 8 4 8} {it:euromod run, system("BG_2023") country("BG") dataset( "BG_2021_c1") constants("flat_rate = '0.2' f_cpi(2023) = '500'")} {p_end}

{pstd}{opt replace} This option will replace the data held before the simulation by the data resulting from the simulation.

{pstd}{opt euro} This will convert the output of the EUROMOD simulation to euro if the output was in national currency. {p_end}

{pstd}{opt prefix(string)} 
Enables the specification of a prefix that will be added to the variable names of the EUROMOD output loaded into memory. For example, the following syntax will add the prefix sim1 to all the output variable names:{p_end}

{p 8 2 1}{it:euromod run, … prefix(“sim1_”)}{p_end}

{pstd}{opt publiccomponentsonly} This option will run the model with only the public components and ignoring the private components. {p_end}

{pstd}{opt sequentialoutput} This option forces EUROMOD to let the spine run sequentially instead of in a parallelised way over all Households. {p_end}

{pstd}{opt keep} This option keeps the output of the simulation in memory and passes it to other simulation. One can keep only one simulation at a time in memory. {p_end}


{pstd}{opt outputdataset(string)} In case there are multiple output dataset returned by the EUROMOD simulation, one can pass the filename in advance to the command such that the respective dataset will be loaded into memory afterwards. 
Otherwise the data can be loaded in a second step by the {help euromod getdata} command. {p_end}

{pstd}{opt outputpath(string)} In case this path is provided, the output will not only be passed through memory, but also generated as a text file stored on the specified path. {p_end}

{pstd}{opt il_output(string)}: The usage of this option results into the creation of an additional DefOutput added to the spine which will be named “custom_output”. One can specify the income lists one likes to include by spacing them by commas. 
For example, adding an additional DefOutput for the income lists ils_dispy and ils_tax would work as follows: {p_end}

{p 8 4 2}{it:euromod run, system("BG_2023") country("BG") dataset("BG_2021_c1") il_output("ils_dispy, ils_tax")}{p_end}

{pstd}Note that this parameter can be used together with vars_output {p_end}

{pstd}{opt vars_output(string)}: vars_output The usage of this option results into the creation of an additional DefOutput added to the spine which will be named “custom_output”. 
One can specify the variables one likes to include by spacing them by commas. For example, adding an additional DefOutput for the variables  bsa_s and yem would work as follows:{p_end}

{p 8 4 2}{it: euromod run, system("BG_2023") country("BG") dataset("BG_2021_c1") vars_output("bsa_s, yem")}{p_end}

{pstd}Note that this parameter can be used together with il_output {p_end}

{pstd}{opt extrainfo_output(string)}: This option allows for the addition of specific TaxUnit information to the custom_output. It has a specifc syntax that has the following structure

{p 8 4 2}{it:'taxUnitName1': 'Query1,Query2,…', 'taxUnitName2': 'Query3,Query4,…',… }{p_end}

{pstd} taxUnitName# refers to the name of the Tax Unit in the model, Query# refers to a query such as IsDependentChild, IsPartner or a TaxUnit specific variable such as HeadID. 
Note that the TaxUnit names and the Queries need to be enclosed by single quote marks and different Queries or TaxUnit specific variables that belong to the same Tax Unit should be separated by commas. 
One can also include multiple TaxUnit Query combinations by separating them by commas. 
Below one finds a working example that would run EUROMOD and retrieve the IsDependentChild condition for every individual for the tax units tu_bmaycct_bg and tu_bcheydc_bg,
 and the HeadID for every individual according to the tax unit definition of tu_bcheydc_bg. {p_end}

{p 8 4 2}{it: euromod run, system("BG_2023") country("BG") dataset("BG_2021_c1") extrainfo_output("'tu_bmaycct_bg':'IsDependentChild', 'tu_bcheydc_bg':'IsDependentChild,HeadID'")} {p_end}

{pstd}{opt sequentialoutput} This option should only be used when a custom output is demanded through the il_output option or the vars_output option. 
It will suppress the original DefOutput and only return the custom output. 
This can lead to considerable speed gains. {p_end}


{title:Global variables}

{pstd}After a successful EUROMOD simulation, a series of global variables are stored in memory, what facilitates the user to validate and process the output of the simulation. 
The following globals are available to the user after running {help euromod run} command:{p_end}

{pstd}{opt $n_euromod_errors} The number of errors/warnings returned during the simulation. 
Note the output of the simulation is not loaded into memory when an error occurs.{p_end}

{pstd}{opt $EM_n_outputs}: The number of output datasets stored after simulation.{p_end}

{pstd}{opt $EM_outputs}: A string containing the names of the outputs separated by spaces.{p_end}

{pstd}{opt $EM_vars}: Is a variable spaced list of the variables contained in the EUROMOD output data loaded into memory. Note that this is the name of the variable in EUROMOD and that this might deviate because of two reasons: {p_end}

{p 8 8 4} - Variable names in Stata can be maximum 32 characters and therefore can be truncated when the variable name in EUROMOD exceeds 32 characters.{p_end}
{p 8 8 4} - The usage of the prefix option. {p_end}
{title:Information stored in the return list}
{txt}{center:{hline 150}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 120: {bf:Description}}{txt}}
{txt}{center:{hline 150}}
{center:{txt}{lalign 30:outputVars}{txt}{lalign 120: A space separated list containg the output variables of the simulation. }}
{center:{txt}{lalign 30:outputFiles}{txt}{lalign 120: A space separated list containg the output files of the simulation. }}
{center:{txt}{lalign 30:nOutputFiles}{txt}{lalign 120: Number of output files produced by the simulation. Note that the files are not written to the hard disk. }}
{center:{txt}{lalign 30:nErrors}{txt}{lalign 120: Number of errors/warnings produced by the simulation of the model. }}
{center:{txt}{lalign 30:errorMessages}{txt}{lalign 120: Macro containing the errorMessages produced by the simulation. }}
{txt}{center:{hline 150}}

{marker examples}
{title:Examples}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(BE_2020) dataset(BE_2020_c2) country(BE)} {p_end}
{p 4 4 2}
This example, is for running EUROMOD for Belgium for system BE_2020 in combination with dataset BE_2020_c2. {p_end}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(SE_2023) dataset(SE_2021_b1) country(SE) constants("tinna_rate1='-0.01'")  prefix(sim2_) euro} {p_end}
{p 4 4 2}
Running EUROMOD, changing a constant, creating a prefix and converting to Euro currency. {p_end}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(SK_2022) dataset(SK_2020_b1) country(SK) extensions(BTA=on) prefix(sim2_)} {p_end}
{p 4 4 2}
Running EUROMOD, controlling the extensions and generating a new prefix. {p_end}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(SK_2022) dataset(SK_2010_x1) country(SK) addons(ITT_XBASE|ITT_XBase) prefix(sim1_)} {p_end}
{p 4 4 2}
Running EUROMOD, multiple addons selection and generating a new prefix. {p_end}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(SK_2022) dataset(SK_2010_x1) country(SK) addons(ITT_XBASE|ITT_XBase) prefix(sim2_) constants("tco_base_t_std(2022)='0.3'") outputdataset(sk_2022_xbase_std.txt)} {p_end}
{p 4 4 2}
Running EUROMOD, Addons selection, generating a new prefix, applying a constant and controlling data output. {p_end}

{p 4 8 2}{cmd:. euromod run if dgn == 0, model({it:Path to model}) system(SE_2022) dataset(SE_2021_b1) country(SE) prefix(sim_)} {p_end}
{p 4 4 2}
Running EUROMOD and Conditional if usage. {p_end}

{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(SE_2022) dataset(SE_2022_x1) country(SE) constants("tinna_rate1='-0.01' tinna_rate2='0.4'") prefix(sim2_) euro } {p_end}
{p 4 4 2}
Running EUROMOD, applying multiple constants and converting to Euro currency. {p_end}

{marker requirements}
{title:Requirements}
In order to use the Stata Connector to EUROMOD, please install the latest software version of EUROMOD.
This can be downloaded from {browse "https://euromod-web.jrc.ec.europa.eu/download-euromod":here}.

{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}