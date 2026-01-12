{smcl}
{* *! version 1.1.0  January 22, 2024 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod_reload##syntax"} {viewerjumpto "Description" "euromod_reload####description"}{viewerjumpto "Examples" "euromod_reload####examples"}
{viewerjumpto "Authors" "euromod_getdata####authors"}{cmd:help euromod_getdata} {it:(EUROMOD Connector v1.0.4)}

{hline}

{title:Title}

{phang}
{bf:euromod_reload} {hline 2} Reloads a model kept in memory by the Connector.{p_end}


{marker syntax}{title:Syntax}

{p 4 4 2}
{cmd:euromod_reload}, 
{bf: model}{it:(path/to/the/model)}  


{title:Description}{marker description}

{p 4 4 2} {cmd:euromod_reload} 
Reloads a model that has been previously loaded by using euromod_run, euromod_getinfo and euromod_setinfo. 
This command is useful when make changes to the model in the user-interface and one wants to apply those changes to the model used in Stata.{p_end}  



{marker examples}
{title:Examples}

{p 4 8 2}{cmd:. euromod_reload, model(path/to/the/model)} {p_end}


{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}