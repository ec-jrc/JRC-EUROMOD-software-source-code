{smcl}
{* *! version 1.1.1  January 22, 2024 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod getdata##syntax"} {viewerjumpto "Description" "euromod getdata####description"}{viewerjumpto "Options" "euromod getdata####options"}{viewerjumpto "Examples" "euromod getdata####examples"}
{viewerjumpto "Authors" "euromod getdata####authors"}{cmd:help euromod getdata} {it:(EUROMOD Connector v1.1.1)}

{hline}

{phang}
{bf:euromod getdata} {hline 2} Loads output data of the last simulation for Euromod (Stata Connector){p_end}
{pstd} Note: euromod_getdata is a complementary command of {help euromod run}{p_end}

{marker syntax}{title:Syntax}

{p 4 4 2}
{cmd:euromod getdata}, 
{bf: outputdataset}{it:(dataset)}  
[{bf:replace}
{bf:prefix}{it:(prefix name)}]

{title:Description}{marker description}

{p 4 4 2} {cmd:euromod getdata} 
This command must be used after using the euromod_run command. The command will load the output data of the last simulation that is kept into memory by the Stata plugin into the Stata DataFrame. {p_end}  

{pstd}{opt outputdataset(string)} Filename of the EUROMOD output dataset as configured in the EUROMOD spine (or custom_output when using il_output, vars_output or info_queries).{p_end}

{title:Options}{marker options}

{pstd}{opt replace} 
This option will replace the data held before the simulation by the data resulting from the simulation. It will delete the data previously held in memory in the Stata DataFrame{p_end}

{pstd}{opt prefix(string)} 
This prefix will be added to the variable names loaded into memory. For example, the following syntax will add all the output variables of the simulation to the Stata DataFrame with sim1_ as a prefix:{p_end}

{pstd}For example, the following syntax:{p_end}

{p 8 4 1}{it:euromod getdata, … prefix(“sim1_”)}{p_end}

{title:Information stored in the return list}
{txt}{center:{hline 130}}
	{center:{txt}{lalign 30: {bf:local}}{txt}{lalign 100: {bf:Description}}{txt}}
{txt}{center:{hline 130}}
{center:{txt}{lalign 30:outputVars}{txt}{lalign 100: A space separated list containg the output variables of the simulation. }}
{txt}{center:{hline 130}}


{marker examples}
{title:Examples}

{p 4 8 2}{cmd:. euromod getdata, outputdataset(sk_2022_xbase_std.txt) prefix(sim1_)} {p_end}


{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}