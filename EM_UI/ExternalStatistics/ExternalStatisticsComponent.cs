using EM_Common;
using EM_Common_Win;
using EM_Transformer;
using EM_XmlHandler;
using EM_Statistics.ExternalStatistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_UI.ExternalStatistics
{
    public class ExternalStatisticsComponent
    {
        internal string systemName = null;
        internal string countryShortName = null;
        internal string systemYear = null;
        internal bool isBaseline = false;

        internal Dictionary<string, Dictionary<string, IlVarInfo>> fiscalIls = new Dictionary<string, Dictionary<string, IlVarInfo>>(StringComparer.OrdinalIgnoreCase);

        public ExternalStatisticsComponent(string countryShortNameParam, string systemNameParam, out Dictionary<string, ExeXml.ExStatDict> externalStatistics, bool doEarns = false)
        {
            countryShortName = countryShortNameParam;
            systemName = systemNameParam;

            List<string> all_ils = new List<string>(InDepthDefinitions.ILS_ALL);
            if (doEarns) all_ils.Add(InDepthDefinitions.ILS_EARNS);

            foreach (string il in all_ils)
                fiscalIls.Add(il, new Dictionary<string, IlVarInfo>(StringComparer.OrdinalIgnoreCase));

            string emPathFolder = UISessionInfo.GetEuromodFilesFolder();
            EMPath emPath = new EMPath(emPathFolder);
            if (!File.Exists(emPath.GetCountryFilePath(countryShortName, true)))
                    throw new Exception($"system info cannot be retrieved: country file {emPath.GetCountryFilePath(countryShortName, true)} does not exist.");

            if (!EM3Country.Transform(emPathFolder, countryShortName, out List<string> errors))
                throw new Exception($"transforming to EM3 failed: {string.Join(";", errors)}");

            string w = string.Empty; Communicator communicator = new Communicator()
            {
                errorAction = new Action<Communicator.ErrorInfo>
                        (ei => { w += $"{Path.GetFileName(emPath.GetFolderOutput())}: {ei.message}" + Environment.NewLine; })
            };

            ExeXml.Country countryContent = ExeXmlReader.ReadCountry(emPath.GetCountryFilePath(countryShortName),
                                                    systemName, null, // null for dataIdentifier
                                                    false, communicator, true); // false for 'ignorePrivate', true for 'readComment'

            externalStatistics = countryContent.extStats;
            AnalyseCountryFile(countryContent,
                        out Dictionary<string, Dictionary<string, IlVarInfo>> contentILs); // not (yet) flat content of incomelists; key: incomelist-name, value: incomelist-content: names + factors of variables/incomelists);


            foreach (string key in fiscalIls.Keys)
            {
                GetContentIL(key, fiscalIls[key]);
            }

            void GetContentIL(string ilNames, Dictionary<string, IlVarInfo> contentIL)
            {
                foreach (string ilName in ilNames.Split(','))
                {
                    if (!contentILs.ContainsKey(ilName)) return;
                    foreach (var v in GetFlatIl(contentILs[ilName], ilName))
                    {
                        string varName = v.Key; IlVarInfo varInfo = v.Value;
                        if (!contentIL.ContainsKey(varName))
                            contentIL.Add(varName, new IlVarInfo() { description = varInfo.description, incomeList = ilName, subtract = varInfo.subtract });
                        else throw new Exception($"Duplicate variable '{varName}' found in income list: {ilName}");
                    }
                }
            }

            Dictionary<string, IlVarInfo> GetFlatIl(Dictionary<string, IlVarInfo> il, string il_name, bool substract = false)
            {
                Dictionary<string, IlVarInfo> flatIl = new Dictionary<string, IlVarInfo>();
                foreach (var entry in il)
                {
                    string entryName = entry.Key; IlVarInfo entryVarInfo = entry.Value;
                    if (il_name.Equals(InDepthDefinitions.ILS_EXTSTAT_OTHER) || !contentILs.ContainsKey(entryName)) // variable
                    {
                        if (!flatIl.ContainsKey(entryName)) flatIl.Add(entryName, entryVarInfo);
                    }
                    else // another incomelist
                    {
                        foreach (var subIlEntry in GetFlatIl(contentILs[entryName], entryName, entryVarInfo.subtract))
                        {
                            string subIlEntryName = subIlEntry.Key; IlVarInfo subILEntryVarInfo = subIlEntry.Value;
                            if (!flatIl.ContainsKey(subIlEntryName)) flatIl.Add(subIlEntryName, subILEntryVarInfo);
                        }
                    }
                }
                return flatIl;
            }
        }

        private static void AnalyseCountryFile(ExeXml.Country countryContent, out Dictionary<string, Dictionary<string, IlVarInfo>> contentILs)
        {
            Dictionary<string, Dictionary<string, IlVarInfo>> _contentILs = new Dictionary<string, Dictionary<string, IlVarInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (ExeXml.Pol pol in countryContent.cao.pols.Values)
            {
                if (!pol.on) continue;
                foreach (var fun in pol.funs.Values)
                {
                    if (!fun.on) continue;
                    if (fun.Name.ToLower() == DefFun.DefIl.ToLower()) AnalyseDefIL(fun);
                    else if (fun.Name.ToLower() == DefFun.DefVar.ToLower()) AnalyseDefVar(fun, false);
                    else if (fun.Name.ToLower() == DefFun.DefConst.ToLower()) AnalyseDefVar(fun, true);
                }
            }
            contentILs = _contentILs; 

            void AnalyseDefIL(ExeXml.Fun fun)
            {
                string ilName = null; Dictionary<string, IlVarInfo> content = new Dictionary<string, IlVarInfo>(StringComparer.OrdinalIgnoreCase);
                content.Add("", new IlVarInfo() { description = fun.comment });
                foreach (var par in fun.pars.Values.OrderBy(x => x.order))
                {
                    if (par.Name.ToLower() == DefPar.DefIl.Name.ToLower()) ilName = par.val;
                    else if (!GetNamedParList(fun.Name).Contains(par.Name, true))
                        content.Add(par.Name, new IlVarInfo() { description = par.comment, subtract = FactorToSubstract(par.val) });
                }
                if (ilName == null || _contentILs.ContainsKey(ilName)) return;
                _contentILs.Add(ilName, content);

                bool FactorToSubstract(string parVal)
                {
                    double factor = double.NaN; parVal = parVal.Trim();
                    if (parVal == "+") return false;
                    else if (parVal == "-") return true;
                    else
                    {
                        if (parVal.StartsWith("+")) parVal = parVal.Substring(1);
                        if (double.TryParse(parVal, out double d)) factor = d;
                    }
                    return factor < 0;
                }
            }

            void AnalyseDefVar(ExeXml.Fun fun, bool isConst)
            {
                Dictionary<string, string> varAndGroup = new Dictionary<string, string>();
                List<string> groupsNonMonVars = new List<string>();
                foreach (var par in fun.pars.Values) // just assess whether variables are monetary and ignore other complexities (dataset, systemyear, condition) for simplicity
                {
                    if (!GetNamedParList(fun.Name).Contains(par.Name, true)) varAndGroup.TryAdd(par.Group, par.Name);
                    else if (par.Name.ToLower() == DefPar.DefVar.Var_Monetary.ToLower() && par.val.ToLower() == DefPar.Value.NO)
                        groupsNonMonVars.Add(par.Group);
                }
            }

            List<string> GetNamedParList(string funName)
            {
                List<string> namedParList = DefinitionAdmin.GetFunDefinition(funName, false).GetParList().Keys.ToList();
                foreach (Dictionary<string, DefinitionAdmin.Par> parList in from g in DefinitionAdmin.GetFunDefinition(funName, false).GetGroupParList() select g.par)
                    namedParList.AddRange(from p in parList select p.Key);
                return namedParList;
            }
        }

        internal Dictionary<string, IlVarInfo> GetIncomelistContent(string ilName)
        {
            if (fiscalIls.ContainsKey(ilName)) return fiscalIls[ilName];
            throw new Exception($"Programm error: unknown incomelist {ilName}");
        }

    }

    
}
