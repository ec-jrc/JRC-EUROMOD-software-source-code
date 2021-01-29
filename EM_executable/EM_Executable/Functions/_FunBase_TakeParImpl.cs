using EM_Common;
using System;
using System.Collections.Generic;
using EM_XmlHandler;

namespace EM_Executable
{
    internal partial class FunBase // this part (_FunBas_TakeParImpl) contains the actual implementation of TakePar (in _FunBase_Main)
    {
        /// <summary>
        /// try to identify parameters and, if valid, generate the respective ParXXX and put into respective lists (see above)
        /// </summary>
        private void ClassifyParameters(Dictionary<string, ExeXml.Par> xmlParList, DefinitionAdmin.Fun funDef)
        {
            int dummyGroupNumber = -9999; // this is for placeholder-parameters, see below
            foreach (var xmlPar in xmlParList)
            {
                Description parDescription = new Description(description, xmlPar.Value, xmlPar.Key);
                string xmlParName = xmlPar.Value.Name;

                // first check if it is an 'ordinary' parameter ...
                DefinitionAdmin.Par parDef = funDef.GetParDef(xmlParName);
                if (parDef != null)
                {
                    string officialParName = funDef.GetOfficalParName(xmlParName); // is (in contrast to xmlParName) case-sensitive and
                                                                                   // corresponds to constants as defined in Common-library
                                                                                   // generate parameter ...
                    ParBase par = GeneratePar(xmlPar, parDef, parDescription);
                    // ... and put it into the list it belongs too ...
                    if (parDef.isFootnote) // ... footnote-list
                    {
                        if (GetGroupNumber(xmlPar.Value, parDescription, out int dummy))
                        {
                            if (!footnotePar.ContainsKey(xmlPar.Value.Group))
                                footnotePar.Add(xmlPar.Value.Group, new Dictionary<string, ParBase>());
                            if (!footnotePar[xmlPar.Value.Group].ContainsKey(officialParName))
                                footnotePar[xmlPar.Value.Group].Add(officialParName, par);
                            else ReportDoubleDef(parDescription);
                        }
                    }
                    else if (parDef.maxCount == 1) // ... unique-parameter-list
                    {
                        if (!uniquePar.ContainsKey(officialParName))
                        {
                            uniquePar.Add(officialParName, par);
                            if (infoStore.runConfig.warnAboutUselessGroups && !string.IsNullOrEmpty(xmlPar.Value.Group)) ReportUselessGroup(parDescription);
                        }
                        else ReportDoubleDef(parDescription);
                    }
                    else // ... non-unique-parameter-list
                    {
                        if (!nonUniquePar.ContainsKey(officialParName)) nonUniquePar.Add(officialParName, new List<ParBase>());
                        nonUniquePar[officialParName].Add(par);
                        if (infoStore.runConfig.warnAboutUselessGroups && !string.IsNullOrEmpty(xmlPar.Value.Group)) ReportUselessGroup(parDescription);
                    }
                    continue;
                }
                // ... if not successuful, check if it is a group parameter ...
                parDef = funDef.GetGroupParDef(xmlParName, out string groupName);
                if (parDef != null)
                {
                    if (GetGroupNumber(xmlPar.Value, parDescription, out int groupNo))
                    {
                        // generate parameter ...
                        ParBase par = GeneratePar(xmlPar, parDef, parDescription);
                        // ... and put it into group-parameter-list
                        if (!groupPar.ContainsKey(groupName))
                            groupPar.Add(groupName, new SortedList<int, List<ParBase>>());
                        if (!groupPar[groupName].ContainsKey(groupNo))
                            groupPar[groupName].Add(groupNo, new List<ParBase>());
                        if (parDef.maxCount > 1 || GetUniqueGroupPar<ParBase>(xmlPar.Value.Name, groupPar[groupName][groupNo]) == null)
                            groupPar[groupName][groupNo].Add(par);
                        else ReportDoubleDef(parDescription);
                    }
                    continue;
                }
                // ... if still not successful, it could still be placehoder parameter ...
                if (funDef.AllowsForPlaceholders())
                {
                    parDef = funDef.GetPlaceholderDef(out string phGroupName);
                    ParBase par = GeneratePar(xmlPar, parDef, parDescription, true);
                    if (phGroupName == null) // non-group placeholder, as e.g. in SetDefault, DefIL
                    {
                        if (!placeholderPar.TryAdd(xmlParName, par))
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                                { isWarning = true, message = $" {parDescription.Get()}: {xmlParName} double definition (is ignored)" });
                    }
                    else // group placeholder, as e.g. in DefVar (PH, IsGlobal, IsMonetary), Uprate (PH, FactorCond)
                    {
                        GetGroupNumber(xmlPar.Value, parDescription, // note that this returns a dummy-group if the user didn't indicate one
                            out int groupNo, true); // as placeholders only actually need a group if they are "further specified" (e.g. uprating-factors: factor_condition)
                        if (!groupPar.ContainsKey(phGroupName))
                            groupPar.Add(phGroupName, new SortedList<int, List<ParBase>>());
                        if (!groupPar[phGroupName].ContainsKey(groupNo))
                            groupPar[phGroupName].Add(groupNo, new List<ParBase>());
                        groupPar[phGroupName][groupNo].Add(par);
                    }
                    continue;
                }
                // ... now we give up
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = true, message = $" {parDescription.Get()}: unknown parameter" });
            }

            if (infoStore.runConfig.warnAboutUselessGroups)
                foreach (var groupType in groupPar)
                {
                    if (groupType.Key.ToLower() == DefPar.SchedCalc.Band_XXX.ToLower()) continue;
                    foreach (var group in groupType.Value)
                        if (group.Key >= 0 && group.Value.Count == 1) ReportUselessGroup(group.Value[0].description);
                }

            // internal function for generating parameters
            ParBase GeneratePar(KeyValuePair<string, ExeXml.Par> xmlPar, DefinitionAdmin.Par parDef,
                                Description parDescription, bool isPlaceholder = false)
            {
                ParBase par;
                switch (parDef.valueType)
                {
                    case DefPar.PAR_TYPE.FORMULA: par = new ParFormula(infoStore); break;
                    case DefPar.PAR_TYPE.CONDITION: par = new ParCond(infoStore); break;
                    case DefPar.PAR_TYPE.BOOLEAN: par = new ParBool(infoStore); break;
                    case DefPar.PAR_TYPE.NUMBER: par = new ParNumber(infoStore); break;
                    case DefPar.PAR_TYPE.TEXT: par = new ParBase(infoStore); break;
                    case DefPar.PAR_TYPE.VAR: par = new ParVar(infoStore); break;
                    case DefPar.PAR_TYPE.OUTVAR: par = new ParOutVar(infoStore); break;
                    case DefPar.PAR_TYPE.IL: par = new ParIL(infoStore); break;
                    case DefPar.PAR_TYPE.TU: par = new ParTU(infoStore); break;
                    case DefPar.PAR_TYPE.VARorIL: par = new ParVarIL(infoStore); break;
                    case DefPar.PAR_TYPE.CATEG: par = new ParCateg(infoStore, parDef.categValues); break;
                    case DefPar.PAR_TYPE.PLACEHOLDER: par = new ParBase(infoStore); break;
                    default: throw new Exception($"Not yet properly implemented parameter value type {parDef.valueType}");
                }
                par.description = parDescription;
                par.xmlValue = xmlPar.Value.val;
                par.description.isPlaceholder = isPlaceholder;
                return par;
            }

            bool GetGroupNumber(ExeXml.Par p, Description parDescription, out int groupNumber, bool allowDummy = false)
            {
                groupNumber = dummyGroupNumber++;  // placeholders (e.g.uprating factors) only actually need a group
                if (string.IsNullOrEmpty(p.Group)) // if they are "further specified" (e.g.factor_condition)
                {
                    if (allowDummy) return true;
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $" {parDescription.Get()}: missing group" });
                    return false;
                }
                if (int.TryParse(p.Group, out groupNumber)) return true;
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $" {parDescription.Get()}: invalid group {p.Group} (must be an integer number)" });
                return false;
            }

            void ReportDoubleDef(Description parDescription)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $" {parDescription.Get()}: defined more than once" });
            }

            void ReportUselessGroup(Description parDescription)
            {
                if (parDescription.GetFunName() != DefFun.Store)    // add a special exception for Store, as it has its own reporting
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = true, message = $" {parDescription.Get()}: group has no use" });
            }
        }

        /// <summary> check for compulsory parameters </summary>
        private void CheckForCompleteness(Dictionary<string, ExeXml.Par> xmlParList, DefinitionAdmin.Fun funDef)
        {
            // check if compulsory non-group parameters are available
            foreach (var p in funDef.GetParList())
            {
                string parName = p.Key; DefinitionAdmin.Par parDef = p.Value;
                if (parDef.minCount == 0) continue; // optional parameter (e.g. Allocate's Share_Between)
                                                    // (note that footnote-parameters are always optional)
                bool found = false;
                if (parDef.maxCount == 1) // check for unique compulsory parameter (e.g. ArithOp's Formula)
                {
                    if (GetUniquePar<ParBase>(parName) != null) found = true;
                    else // could still be available as substitute (e.g. output_var not available, but output_add_var)
                    {
                        foreach (string substitute in parDef.substitutes)
                            if (GetUniquePar<ParBase>(substitute) != null) { found = true; break; };
                    }
                }
                if (parDef.maxCount > 1) // check for non-unique parameter, where at least one must be defined (e.g. SetDefault's Dataset)
                {
                    if (GetNonUniquePar<ParBase>(parName).Count > 0) found = true;
                    else // unlikely, but could still be available as substitute
                    {
                        foreach (string substitute in parDef.substitutes)
                            if (GetNonUniquePar<ParBase>(substitute).Count > 0) { found = true; break; };
                    }
                }

                if (!found) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $" {description.Get()}: compulsory parameter {parName} is missing" });
            }

            // check if compulsory groups, and compulsory parameter within groups are available
            foreach (var groupDef in funDef.GetGroupParList())
            {
                if (groupDef.minCount > 0) // check for compulsory group, e.g. BenCalc's Comp_X-group
                {
                    if (!groupPar.ContainsKey(groupDef.groupName)) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $" {description.Get()}: compulsory group {groupDef.groupName} is missing" });
                }

                // whether or not the group is compulsory it can (if existent) ask for completeness, i.e. have compulsory parts
                if (!groupPar.ContainsKey(groupDef.groupName)) continue; // no such group exists
                foreach (var p in groupDef.par) // loop over the group-definition's parameters (e.g. Comp_Cond, Comp_perTU, Comp_LowLim, ...)
                {
                    string parName = p.Key; DefinitionAdmin.Par parDef = p.Value;
                    if (parDef.minCount == 0) continue; // optional parameter (e.g. Comp_LowLim)

                    // check for compulsory parameter (e.g. Comp_Cond)
                    foreach (var g in GetParGroups(groupDef.groupName)) // loop over all existent groups
                    {                                                   // (e.g. all components of an actual BenCalc)
                        int groupNo = g.Key; List<ParBase> groupPar = g.Value;
                        int found = 0;
                        if (parDef.maxCount == 1) // check for unique compulsory group-parameter (e.g. BenCalc's Comp_Cond)
                        {
                            if (GetUniqueGroupPar<ParBase>(parName, groupPar) != null) ++found;
                            foreach (string substitute in parDef.substitutes)
                                if (GetUniqueGroupPar<ParBase>(substitute.ToLower(), groupPar) != null) ++found;
                            if (found > 1) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                                { isWarning = false, message = $" {description.Get()}: double definition of group-parameter {parName} (group {groupNo})" });
                        }
                        if (parDef.maxCount > 1) // check for non-unique compulsory group-parameter (e.g. Uprate's AggVarPart)
                        {
                            if (GetNonUniqueGroupPar<ParBase>(parName, groupPar).Count > 0) ++found;
                            foreach (string substitute in parDef.substitutes)
                                if (GetNonUniqueGroupPar<ParBase>(substitute.ToLower(), groupPar).Count > 0) ++found;
                        }
                        if (found == 0) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                            { isWarning = false, message = $" {description.Get()}: compulsory group-parameter {parName} (group {groupNo}) is missing" });
                    }
                }
            }
        }
    }
}
