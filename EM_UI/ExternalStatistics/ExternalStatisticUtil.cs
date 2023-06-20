using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_XmlHandler;
using EM_Statistics.ExternalStatistics;
using System;
using System.Collections.Generic;
using EM_XmlHandler;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EM_UI.ExternalStatistics.ExternalStatisticsComponent;

namespace EM_UI.ExternalStatistics
{
    class ExternalStatisticUtil
    {
        /// <summary>
        /// Returns the External Statistics ready to be included in the tables
        /// </summary>
        /// <param name="countryShortName"></param>
        public static ExternalStatistic LoadExternatStatisticsComponentsAndValues(string countryShortName)
        {
            //We are going to obtain information from two different sources
            //1) The income list components (HU.xml, EM3 version)
            List<String> years;
            CountryConfigFacade _countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(countryShortName, true);
            years = GetYearsFromStoredExternalStatistics(_countryConfigFacade);
            Dictionary<string, ExternalStatisticAggregate> xmlIncomeListComponents = GetCountryXMLIncomeListComponents(countryShortName, years);

            //2) The information stored in the XML by the user (HU.xml, EM2 version) is combined with the information in the income list
            ExternalStatistic storedExternalStatistic = GetStoredExternalStatistics(_countryConfigFacade, xmlIncomeListComponents, years);

            return storedExternalStatistic;
        }

        // this quickly tries to return the years in the External Statistics by checking the first row only
        // it assumes that all rows have the same set of years (something that should be always true in the table)
        public static List<string> GetYearsFromStoredExternalStatistics(CountryConfigFacade _countryConfigFacade)
        {
            List<string> years = new List<string>();
            CountryConfig.ExternalStatisticRow row = _countryConfigFacade.GetExternalStatistics().FirstOrDefault();
            if (row != null)
            {
                string yearValues = row.YearValues;
                if (!String.IsNullOrEmpty(yearValues))
                {
                    string[] yearValuesArray = yearValues.Split(InDepthDefinitions.SEPARATOR);
                    if (yearValuesArray != null && yearValuesArray.Length > 0)
                    {
                        foreach (string yearValue in yearValuesArray)
                        {
                            string[] eachFieldSeparated = yearValue.Split(InDepthDefinitions.SEPARATOR_INNER);
                            years.Add(eachFieldSeparated[0]);
                        }
                    }
                }
            }
            return years;
        }

        public static ExternalStatistic GetStoredExternalStatistics(CountryConfigFacade _countryConfigFacade, Dictionary<string, ExternalStatisticAggregate> xmlIncomeListComponents, List<String> years)
        {
            // This is static, so it can be read every time without impacting the speed
            Dictionary<string, ExternalStatisticDistributional> distComponents = GetDistributionalComponents();

            ExternalStatistic externalStatistics = new ExternalStatistic();
  
            foreach (CountryConfig.ExternalStatisticRow stat in _countryConfigFacade.GetExternalStatistics()) // add one row for each index
            {
                if (stat.TableName.Equals(ExternalStatisticsForm.AGGREGATES)) { 
                    //New ExternalStatisticAggregate
                    ExternalStatisticAggregate aggregate = null;
                    if (xmlIncomeListComponents.ContainsKey(stat.Category + "_" + stat.Reference))
                        aggregate = xmlIncomeListComponents[stat.Category + "_" + stat.Reference];
                    else
                        continue;

                    if(aggregate == null)
                    {
                        aggregate = new ExternalStatisticAggregate();
                        aggregate.Name = stat.Reference;
                        aggregate.IncomeList = stat.Category;
                        aggregate.Description = stat.Description;
                    }
                    
                    aggregate.Source = stat.Source;
                    aggregate.Comment = stat.Comment;
                    aggregate.Destination = stat.Destination;

                    string yearValues = stat.YearValues;
                    Dictionary<string, ExternalStatisticAggregateValues> valuePerYear = new Dictionary<string, ExternalStatisticAggregateValues>();

                    if (!String.IsNullOrEmpty(yearValues))
                    {
                        string[] yearValuesArray = yearValues.Split(InDepthDefinitions.SEPARATOR);
                        if (yearValuesArray != null && yearValuesArray.Length > 0)
                        {
                            foreach (string yearValue in yearValuesArray)
                            {
                                string[] eachFieldSeparated = yearValue.Split(InDepthDefinitions.SEPARATOR_INNER);
                                if (eachFieldSeparated != null && eachFieldSeparated.Length > 0 && eachFieldSeparated.Length == 4)
                                {
                                    string year = eachFieldSeparated[0];
                                    if (years.Contains(year))
                                    {
                                        ExternalStatisticAggregateValues extValues = new ExternalStatisticAggregateValues();
                                        extValues.Amount = eachFieldSeparated[1];
                                        extValues.Beneficiares = eachFieldSeparated[2];
                                        extValues.Level = eachFieldSeparated[3];
                                        valuePerYear.Add(year, extValues);
                                    }
                                }
                            }
                        }
                    }

                    aggregate.YearValues = valuePerYear;
                    xmlIncomeListComponents[stat.Category + "_" + stat.Reference] = aggregate;
                }
                else
                {
                    if (!distComponents.ContainsKey(stat.Reference)) continue;
                    //New ExternalStatisticDistributional
                    string yearValues = stat.YearValues;
                    Dictionary<string, string> yearValuesDictionary = new Dictionary<string, string>();

                    if(!String.IsNullOrEmpty(yearValues))
                    {
                        string[] yearValuesArray = yearValues.Split(InDepthDefinitions.SEPARATOR);
                        if (yearValuesArray != null && yearValuesArray.Length > 0)
                        {
                            foreach (string yearValue in yearValuesArray)
                            {
                                string[] eachFieldSeparated = yearValue.Split(InDepthDefinitions.SEPARATOR_INNER);
                                if (years.Contains(eachFieldSeparated[0]))
                                {
                                    if (eachFieldSeparated != null && eachFieldSeparated.Length > 0 && eachFieldSeparated.Length == 2)
                                    {
                                        yearValuesDictionary.Add(eachFieldSeparated[0], eachFieldSeparated[1]);
                                    }
                                }
                            }
                        }
                    }
                    distComponents[stat.Reference].Source = stat.Source;
                    distComponents[stat.Reference].Comment = stat.Comment;
                    distComponents[stat.Reference].YearValues = yearValuesDictionary;
                }
            }

            externalStatistics.Aggregate = xmlIncomeListComponents;
            externalStatistics.Distributional = distComponents;
            externalStatistics.Years = years;

            return externalStatistics;
        }

        public static Dictionary<string, ExternalStatisticAggregate> GetCountryXMLIncomeListComponents(string countryShortName, List<String> years)
        {
            Dictionary<string, ExternalStatisticsComponent> componentsIncomeList = ReadIncomeListComponentsFromXMLFile(countryShortName, years, false, true);
            Dictionary<string, ExternalStatisticAggregate> externalStatisticsAgregate = new Dictionary<string, ExternalStatisticAggregate>();
            Dictionary<string, Dictionary<string, ExternalStatisticAggregate>> tempStatisticsAgregate = new Dictionary<string, Dictionary<string, ExternalStatisticAggregate>>();
            foreach (KeyValuePair<string, ExternalStatisticsComponent> yearIncomeLists in componentsIncomeList)
            {
                string year = yearIncomeLists.Key;

                //They key is the incomelist, it contains all variables inside
                Dictionary<string, Dictionary<string, IlVarInfo>> fiscalIls = yearIncomeLists.Value.fiscalIls;

                foreach (KeyValuePair<string, Dictionary<string, IlVarInfo>> currentIncomeList in fiscalIls)
                {
                    string incomeListName = currentIncomeList.Key;
                    Dictionary<string, IlVarInfo> componentsInfo = currentIncomeList.Value;

                    // then add the incomelist variables
                    foreach (KeyValuePair<string, IlVarInfo> component in componentsInfo)
                    {
                        string variableName = component.Key;
                        IlVarInfo varInfo = component.Value;

                        //We add the variables grouped in incomelists. This is because the componentsIncomeList (outer loop)
                        //is comming grouped in years and can contain different variables in different years.
                        string aggregateKey = incomeListName + "_" + variableName;
                        if (!tempStatisticsAgregate.ContainsKey(incomeListName))   // If the incomelist it doesn't exist, add it
                            tempStatisticsAgregate.Add(incomeListName, new Dictionary<string, ExternalStatisticAggregate>());
                        //The key for the variable is the income list + variable name
                        if (!tempStatisticsAgregate[incomeListName].ContainsKey(aggregateKey))   // If the variable doesn't exist, add it
                        {
                            tempStatisticsAgregate[incomeListName].Add(aggregateKey, new ExternalStatisticAggregate() {
                                Name = variableName,
                                Description = varInfo.description,
                                IncomeList = varInfo.incomeList,
                                CanBeDeleted = false
                            });
                        }
                        tempStatisticsAgregate[incomeListName][aggregateKey].YearValues.Add(year, new ExternalStatisticAggregateValues());
                    }

                }
            }
            // Then we use the grouped array to populate the flat externalStatisticsAgregate
            foreach (string ils in tempStatisticsAgregate.Keys)
            {
                foreach (string aggregateKey in tempStatisticsAgregate[ils].Keys)
                {
                    if (!externalStatisticsAgregate.ContainsKey(aggregateKey))
                        externalStatisticsAgregate.Add(aggregateKey, tempStatisticsAgregate[ils][aggregateKey]);
                }
            }

            return externalStatisticsAgregate;
        }

        static public Dictionary<string, ExternalStatisticsComponent> ReadIncomeListComponentsFromXMLFile(string countryShortName, List<String> yearsOrSystems, bool doEarns = false, bool doYears = false)
        {
            Dictionary<string, ExternalStatisticsComponent> componentsIncomeLists = new Dictionary<string, ExternalStatisticsComponent>();
            CountryConfigFacade _countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(countryShortName, true);

            //Here we need to read from the currently open country, the short name and all the systems.
            foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRowsOrdered())
            {
                string baselineCurrent = countryShortName + "_" + systemRow.Year; // if working with years, do baselines only
                if ((doYears && (!yearsOrSystems.Contains(systemRow.Year, true) || !baselineCurrent.Equals(systemRow.Name, StringComparison.InvariantCultureIgnoreCase))) || (!doYears && !yearsOrSystems.Contains(systemRow.Name, true))) continue;

                //Then we iterate and populate a dictionary of components (one component per system). The key will be the year.
                ExternalStatisticsComponent component = new ExternalStatisticsComponent(countryShortName, systemRow.Name, out Dictionary<string, ExeXml.ExStatDict> externalStatistics, doEarns);
                if (component != null)
                {
                    componentsIncomeLists.Add(doYears ? systemRow.Year : systemRow.Name, component);
                }
            }

            return componentsIncomeLists;
        }

        static Dictionary<string, ExternalStatisticDistributional> GetDistributionalComponents()
        {
            Dictionary<string, ExternalStatisticDistributional> distComponents = new Dictionary<string, ExternalStatisticDistributional>();
            foreach (List<string> comp in InDepthDefinitions.ROWS_INEQUALITY)
            {
                distComponents.Add(comp[0], new ExternalStatisticDistributional(ExternalStatisticsForm.DISTRIBUTIONAL_INEQUALITY, comp[0], comp[1], "", ""));
            }
            foreach (List<string> comp in InDepthDefinitions.ROWS_POVERTY)
            {
                distComponents.Add(comp[0], new ExternalStatisticDistributional(ExternalStatisticsForm.DISTRIBUTIONAL_POVERTY, comp[0], comp[1], "", ""));
            }
            return distComponents;
        }
    }

    public class ExternalStatsTuple
    {
        internal string incomeList;
        internal string name;
        internal string description;
        internal string yearValues;
        internal string comment;
        internal string source;
        internal string tableName;
        internal string destination;

        public ExternalStatsTuple() { }
        
        public ExternalStatsTuple(string incomeList, string name, string description, string yearValues, string comment, string source, string tableName, string destination)
        {
            this.incomeList = incomeList;
            this.name = name;
            this.description = description;
            this.yearValues = yearValues;
            this.comment = comment;
            this.source = source;
            this.tableName = tableName;
            this.destination = destination;
        }
    }
}
