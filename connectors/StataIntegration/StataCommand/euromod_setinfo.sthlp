{smcl}
{* *! version 1.1.1  1 December, 2025 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod setinfo##syntax"} {viewerjumpto "Description" "euromod setinfo####description"}{viewerjumpto "Examples" "euromod setinfo####examples"}
{viewerjumpto "Authors" "euromod setinfo####authors"}{cmd:help euromod setinfo} {it:(EUROMOD Connector v1.1.1)}

{hline}


{phang}
{bf:euromod setinfo} {hline 2} Easy loops on data for Euromod Stata Connector {p_end}
{pstd} Note: euromod_getdata is a complementary command of {help euromod_run}{p_end}

{marker syntax}{title:Syntax}

{p 4 4 2}
{cmd:euromod setinfo}, 
{bf:model}{it:(EUROMOD system name)}  
{bf:country}{it:(EU country)} 

{title:Description}{marker description}

{pstd}{cmd:euromod setinfo} allows the user to change parameters in the EUROMOD model. .{p_end}  

{pstd}{opt model(string)} This parameter is the root folder of the EUROMOD project, where the model is stored. This parameter is mandatory. When one only passes this parameter to the euromod_getinfo command, the command stores a list of countries available in the model in the return list.{p_end}

{pstd}{opt system(string)} Indicates the system you want to target.{p_end}

{pstd}{opt parId(string)} Id of the parameter that you want to target. You need to copy the identifier of the system with the earliest policy year. {p_end}

{pstd}{opt newParValue(string)} Indicates the new value of the variable that you want to set.{p_end}



{marker examples}
{title:Examples}

{p 4 4 2}

{p 8 4 2}euromod setinfo, model("$model_path") country("BG")	system("BG_2023") parId("5DF9FF12-F7D3-452F-BF2C-0320937B87C9") newParValue("12345") {p_end}


{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}