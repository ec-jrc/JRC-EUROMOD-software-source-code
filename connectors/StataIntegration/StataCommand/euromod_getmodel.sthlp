{smcl}
{* *! version 1.1.1  1 December, 2025 @ 12:00:00}{...}
{viewerjumpto "Syntax" "euromod getmodel##syntax"} {viewerjumpto "Description" "euromod getmodel####description"}{viewerjumpto "Options" "euromod getmodel####options"}{viewerjumpto "Examples" "euromod getmodel####examples"}
{viewerjumpto "Authors" "euromod getmodel####authors"}{cmd:help euromod getmodel} {it:(EUROMOD Connector v1.1.1)}

{hline}

{phang}
{bf:euromod getmodel} {hline 2} Loads the latest publically released model from the euromod website and unzips it in the folder. {p_end}


{marker syntax}{title:Syntax}

{p 4 4 2}
{cmd:euromod getmodel}, 
{bf: folder}{it:(path/where/you/want/to/store/the/model)}  

{title:Description}{marker description}

{p 4 4 2} {cmd:euromod getmodel} 
This command loads the latest public model of euromod and stores it either in the working directory or in the path provided by the option {it:folder}  {p_end}  

{title:Options}{marker options}

{pstd}{opt folder(string)} 
An optional argument that sets the path to where the folder should be downloaded and unzipped.{p_end}


{marker examples}
{title:Examples}

{p 4 8 2}{cmd:. euromod getmodel, folder(path/to/project)} {p_end}


{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}