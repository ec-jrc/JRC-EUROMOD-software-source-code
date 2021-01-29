using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        // also see "explanation wrt new handling of ChangeParam" in Explanations.cs
        private void AdaptChangeParam_Fun(EM2Item fun, EM2Country.Content ctryContent, List<string> errors)
        {
            if (fun.name.ToLower() != DefFun.ChangeParam.ToLower()) return;

            // === G A T H E R  I N F O R M A T I O N ===
            // get all parameters of the ChangeParam ...
            // i.e. Param_Id, Param_NewVal, Param_CondVal, RunCond, #_DataBasename, #_xxx (i.e. other footnote-parameters)
            var allPar = from p in ctryContent.parameters
                         where p.Value.properties.ContainsKey(EM2TAGS.FUNCTION_ID) && p.Value.properties[EM2TAGS.FUNCTION_ID] == fun.id
                         select p;
            // ... and put them into lists that allow deciding on what to do with them
            string idRunCond = null;                                   // id of Run_Cond-parameter
            List<string> idDataBaseFootnotes = new List<string>();     // ids of footnote-parameters #_DataBase (referred by Run_Cond)
            List<string> idsParamId_ChangeSwitch = new List<string>(); // ids of Param_Id-parameters, which refer to a function or policy
            List<string> idsVal_ChangeSwitch = new List<string>();     // ids of Param_New/CondVal-parameters with value on/off/toggle
            List<string> idsParamId_ChangeParam = new List<string>();  // ids of Param_Id-parameters, which refer to a parameter
            List<string> idsVal_ChangeParam = new List<string>();      // ids of Param_CondVal-parameters with another value than on/off/toggle
            List<string> idsOtherFootnotes = new List<string>();       // ids of other footnote-parameters than #_DataBase (rather unlikely)

            bool hasChangeParam_Par = false;  // has parameters that belong into a ChangeParam function
            bool hasChangeSwitch_Par = false; // has parameters that belong into a ChangeSwitch function
            foreach (var par in allPar)
            {
                // --- Param_Id ---
                if (par.Value.name.ToLower() == DefPar.ChangeParam.Param_Id.ToLower())
                {   // Param_Id refers to a policy or function -> belongs into a ChangeSwitch function
                    bool? refersToPolFun = RefersToPolFun(par.Value.id);
                    if (refersToPolFun == true) { idsParamId_ChangeSwitch.Add(par.Value.id); hasChangeSwitch_Par = true; }
                    // Param_Id refers to a parameter -> belongs into a ChangeParam function
                    else if (refersToPolFun == false) { idsParamId_ChangeParam.Add(par.Value.id); hasChangeParam_Par = true; }
                    //else: parameter is n/a in all systems
                }

                // --- Run_Cond ---
                else if (par.Value.name.ToLower() == DefPar.Common.Run_Cond.ToLower()) idRunCond = par.Key;

                // --- #_DataBase ---
                else if (par.Value.name.ToLower() == DefQuery.Par.DataBasename.ToLower()) idDataBaseFootnotes.Add(par.Key);

                // --- Param_CondVal or Param_NewVal ---
                else if (par.Value.name.ToLower() == DefPar.ChangeParam.Param_NewVal.ToLower() ||
                         par.Value.name.ToLower() == DefPar.ChangeParam.Param_CondVal.ToLower())
                {
                    // Param_Cond/NewVal's value = off/on/toggle -> belongs into a ChangeSwitch function
                    bool? isSwitchChange = IsSwitchChange(par.Value.id);
                    if (isSwitchChange == true) { idsVal_ChangeSwitch.Add(par.Value.id); hasChangeSwitch_Par = true; }
                    // Param_Cond/NewVal's value = some parameter-change -> belongs into a ChangeParam function
                    else if (isSwitchChange == false) { idsVal_ChangeParam.Add(par.Value.id); hasChangeParam_Par = true; }
                    //else: parameter is n/a in all systems
                }

                // --- other (valid) parameter can only be another footnote parameter than #_DataBase ---
                else idsOtherFootnotes.Add(par.Key);
            }

            // === T R A N S F O R M   A C T I O N S ===
            if (hasChangeSwitch_Par && hasChangeParam_Par) // splitting the function in two is complicated (must invent ids ...) and is not
            {                                              // necessary for existing countries/add-ons, thus do not forsee before necessary ...
                // AddError("impossible to assign all parameters to either a ChangeParam or ChangeSwitch function");
                // return;

                // unfortunately it seems that we need a solution for this a bit complicated case, thus:
                SeparateSwitchFun(); hasChangeSwitch_Par = false;
            } // note (!hasChangeSwitch_Par && !hasChangeParam_Par) is theoretically possible, if all pars are n/a, but that wouldn't do any harm

            // --- change to ChangeSwitch function --- 
            if (hasChangeSwitch_Par)
            {
                fun.name = DefFun.ChangeSwitch;
                foreach (string id in idsParamId_ChangeSwitch) // rename 'Param_Id' to 'PolFun'
                    ctryContent.parameters[id].name = DefPar.ChangeSwitch.PolFun;
                foreach (string id in idsVal_ChangeSwitch) // rename 'Param_CondVal'/'Param_NewVal' to 'SwitchNewVal'
                {                                          // and change possible 'toggle'-value to 'off'
                    ctryContent.parameters[id].name = DefPar.ChangeSwitch.Switch_NewVal;
                    foreach (var sysPar in GetSysPar(id))
                        if (XmlHelpers.RemoveCData(sysPar.value).ToLower() == DefPar.Value.TOGGLE) sysPar.value = DefPar.Value.OFF;
                    // else: no change necessary for on / off / n/a
                }
                // no change necessary for other parameters, i.e. Run_Cond and associated footnotes
            }

            // --- adapt ChangeParam function ---
            if (hasChangeParam_Par)
            {
                // if 'RunCond={IsUsedDatabase#1}' transform to 'Database=xxx', otherwise error (e.g. an actual run-time RunCond or a more complicated RunCond)
                if (idRunCond != null && !TransformRunCond()) return;

                // rename from Param_CondVal to Param_NewVal if necessary
                foreach (string id in idsVal_ChangeParam) ctryContent.parameters[id].name = DefPar.ChangeParam.Param_NewVal;

                // no change necessary for Param_Id (keeps Guid or symbolic id)
                // ignore other parameters - if there are e.g. unnecessary footnotes, this is also wrong for the old executable
            }

            // === H E L P E R   F U N C T I O N S ===
            void AddError(string error)
            {
                errors.Add($"{ctryContent.general.name}: Failed to transform {fun.name} ({fun.id}): {error} (hint: case not yet handled by transformer)");
            }

            bool TransformRunCond()
            {
                bool runCondDefined = false;
                List<EM2Country.SysItem> runCondVals = GetSysPar(idRunCond).ToList();
                foreach (EM2Country.SysItem runCondVal in runCondVals)
                {
                    string runCond = XmlHelpers.RemoveCData(runCondVal.value).Replace(" ", "").ToLower();
                    if (runCond == DefPar.Value.NA) continue;
                    // remove everything that is allowed in a simple enough run-cond, to then check for empty string
                    for (int i = 999; i >= 1; --i) runCond = runCond.Replace($"isuseddatabase#{i}", "");
                    runCond = runCond.Replace("{}", "").Replace("|", "");
                    if (runCond != string.Empty) AddError("too complex Run_Cond"); // still continue, so the country still runs
                    runCondDefined = true;
                }
                if (runCondDefined) // rename '#_Database' to 'Dataset' if RunCond is actually {IsUsedDatabase#1}
                {                   // (system values are ok, i.e. contain the respective dataset-name)
                    if (idDataBaseFootnotes.Count() == 0) { AddError("missing database specification"); return false; }
                    foreach (string idDataBaseFootnote in idDataBaseFootnotes)
                        ctryContent.parameters[idDataBaseFootnote].name = DefPar.ChangeParam.Dataset;
                }
                // remove the RunCond and its system values
                ctryContent.parameters.Remove(idRunCond);
                foreach (EM2Country.SysItem runCondVal in runCondVals) ctryContent.sysPar.Remove(runCondVal);

                return true;
            }

            bool? RefersToPolFun(string id_Param_Id)
            {
                bool allNa = true;
                foreach (EM2Country.SysItem sysPar in GetSysPar(id_Param_Id))
                {
                    string id = (XmlHelpers.RemoveCData(sysPar.value));
                    if (id == DefPar.Value.NA) continue; allNa = false;
                    if (EM_Helpers.IsGuid(id)) // i.e. most likely this is a country-file (and not an add-on)
                    {
                        if (!ctryContent.parameters.ContainsKey(id)) return true; // a bit unsafe, as only "most likely" a country-file
                    }                                                             // i.e. could still be a country-par-guid in an add-on
                    else // i.e. must be an add-on (unless this is an invalid use of symbolic-id in a country file or otherwise invalid)
                    {
                        if (!AddOn.IsParSymbolicID(id)) return true;
                    } // note: a maybe "safer" way to assess whether the id refers to a parameter or to a policy or function
                }     // would be to analyse the corresponding ParamCond/NewVal (on/off/toggle->Pol/Fun), but that's complex to code
                if (allNa) return null; return false;
            }

            bool? IsSwitchChange(string id_Param_NewVal)
            {
                bool allNa = true;
                foreach (EM2Country.SysItem sysPar in GetSysPar(id_Param_NewVal))
                {
                    string newVal = (XmlHelpers.RemoveCData(sysPar.value)).ToLower();
                    if (newVal == DefPar.Value.OFF || newVal == DefPar.Value.ON || newVal == DefPar.Value.TOGGLE) return true;
                    if (newVal != DefPar.Value.NA) allNa = false;
                }
                if (allNa) return null; return false;
            }

            IEnumerable<EM2Country.SysItem> GetSysPar(string parId)
            {
                return (from sp in ctryContent.sysPar where sp.itemID == parId select sp);
            }

            void SeparateSwitchFun()
            {
                // first generate a separate ChangeSwitch function ...
                EM2Item switchFun = EM2Item.Copy(fun, ctryContent.sysFun, out List<EM2Country.SysItem> switchFunSysVals);
                switchFun.name = DefFun.ChangeSwitch;
                switchFun.order = 1234567; // unimportant for a ChangeSwitch
                ctryContent.functions.Add(switchFun.id, switchFun);
                ctryContent.sysFun.AddRange(switchFunSysVals);

                // ... then "move" parameters from existing ChangeParam to new ChangeSwitch ...
                foreach (string id in idsParamId_ChangeSwitch)
                {
                    ctryContent.parameters[id].partentId = switchFun.id; // move ...
                    ctryContent.parameters[id].properties[EM2TAGS.FUNCTION_ID] = switchFun.id;
                    ctryContent.parameters[id].name = DefPar.ChangeSwitch.PolFun; // ... and rename 'Param_Id' to 'PolFun'

                    foreach (var x in ctryContent.parameters[id].properties)
                        RandColor.WriteLine($"{x.Key}={x.Value}");
                }
                foreach (string id in idsVal_ChangeSwitch)
                {
                    ctryContent.parameters[id].partentId = switchFun.id; // move ...
                    ctryContent.parameters[id].properties[EM2TAGS.FUNCTION_ID] = switchFun.id;
                    ctryContent.parameters[id].name = DefPar.ChangeSwitch.Switch_NewVal; // ... rename 'Param_CondVal'/'Param_NewVal' to 'SwitchNewVal' ...
                    foreach (var sysPar in GetSysPar(id)) // ... and change possible 'toggle'-value to 'off'
                        if (XmlHelpers.RemoveCData(sysPar.value).ToLower() == DefPar.Value.TOGGLE) sysPar.value = DefPar.Value.OFF;
                }

                // ... finally copy possible run-condition ...
                if (idRunCond == null) return;
                EM2Item runCond = EM2Item.Copy(ctryContent.parameters[idRunCond], ctryContent.sysPar, out List<EM2Country.SysItem> runCondSysVals);
                runCond.partentId = switchFun.id; runCond.properties[EM2TAGS.FUNCTION_ID] = switchFun.id;
                ctryContent.parameters.Add(runCond.id, runCond);
                ctryContent.sysPar.AddRange(runCondSysVals);

                // ... and its footnotes
                foreach (string footNoteId in idDataBaseFootnotes)
                {
                    EM2Item footNote = EM2Item.Copy(ctryContent.parameters[footNoteId], ctryContent.sysPar, out List<EM2Country.SysItem> footNoteSysVals);
                    footNote.partentId = switchFun.id; footNote.properties[EM2TAGS.FUNCTION_ID] = switchFun.id;
                    ctryContent.parameters.Add(footNote.id, footNote);
                    ctryContent.sysPar.AddRange(footNoteSysVals);
                }
            }
        }
    }
}
