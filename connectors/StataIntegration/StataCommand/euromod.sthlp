{smcl}
{* *! version 1.1.1  1 December, 2025 @ 12:00:00}{...}
{viewerjumpto "Requirements" "euromod####Requirements"}
{viewerjumpto "Description" "euromod####description"}{viewerjumpto "Examples" "euromod####examples"}
{viewerjumpto "Authors" "euromod####authors"}{cmd:euromod} {it:(EUROMOD Connector v1.1.1)}

{hline}
{marker description}{title:Requirements}
In order to use the Stata Connector to EUROMOD, please install the latest software version of EUROMOD.
This can be downloaded from {browse "https://euromod-web.jrc.ec.europa.eu/download-euromod":here}.

{marker description}{title:Description} The euromod command allows the user to interact with the EUROMOD microsimulation software from Stata. 
The user is able to 
{p 10} - Run the model, see {help euromod run} {p_end}
{p 10} - Download the latest public model, see {help euromod getmodel} {p_end}
{p 10} - Retrieve information about the model, see {help euromod getinfo} {p_end}
{p 10} - Retrieve information about the model change the model, see {help euromod setinfo} {p_end}
{p 10} - Load the data into memory, see {help euromod getdata}{p_end}
Note that euromod getdata command is only needed when multiple datasets are returned by the model.
 
{marker examples}
{title:Examples}

{p 4 8 2}{cmd:. euromod getdata, outputdataset(sk_2022_xbase_std.txt) prefix(sim1_)} {p_end}
{p 4 8 2}{cmd:. euromod run, model({it:Path to model}) system(BE_2020) dataset(BE_2020_c2) country(BE)} {p_end}
{p 4 8 2}{cmd:. euromod getinfo, model({it:Path to model}) system(BE_2020) dataset(BE_2020_c2) country(BE)} {p_end}
{p 4 8 2}{cmd:. euromod setinfo, model("{it:Path to model}") country("BG")	system("BG_2023") parId("5DF9FF12-F7D3-452F-BF2C-0320937B87C9") newParValue("12345")} {p_end}
{marker authors}
{title:Authors}

{p 4 4 2}
Hannes Serruys, B2-JRC Seville (Hannes.SERRUYS@ec.europa.eu)

{hline}

{break}