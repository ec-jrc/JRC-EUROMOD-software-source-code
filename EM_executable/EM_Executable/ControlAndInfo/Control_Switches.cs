using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;
using EM_Common;

namespace EM_Executable
{
    public partial class Control
    {
        private void HandleExtensionSwitches(List<ExeXml.AddOn> addOns)
        {
            // first overwrite the default switch settings in the country file, if the user changed them (via config-settings sent by the run-tool)
            //TakeExtensionSwitches(infoStore.runConfig.extensionSwitches);
            EM_ExtensionSwitchHandler.TakeSwitches(infoStore.country, infoStore.runConfig.extensionSwitches);

            // then overwrite with any instructions from extensions, the means the priority of switches to be on or off is as follows:
            // highest: add-ons (via AddOn_ExtensionSwitch), next: user settings (via run-tool), lowest: default settings (via set-switches-dialog)
            TakeAddOnExtensionSwitches(addOns);

            EM_ExtensionSwitchHandler.ApplySwitches(infoStore.country);
        }
        
        private void TakeAddOnExtensionSwitches(List<ExeXml.AddOn> addOns)
        {
            // make a list of extensions that are switched by the add-on(s), if several add-ons switch the same extension, the last one wins
            Dictionary<string, bool> aoExtensionSwitches = new Dictionary<string, bool>(); // key: extension-id, value: on/off (on=true)

            foreach (ExeXml.AddOn addOn in addOns) // run over all add-ons
                foreach (ExeXml.Fun aoFun in addOn.polAOControl.funs.Values) // search for add-on-function AddOn_ExtensionSwitch
                {
                    if (!aoFun.on || aoFun.Name.ToLower() != DefFun.AddOn_ExtensionSwitch.ToLower()) continue;

                    // first sort the groups together (there may be several groups of Extension_Id or Extension_Name + Extension_Switch)
                    Dictionary<string, List<ExeXml.Par>> parGroups = new Dictionary<string, List<ExeXml.Par>>();
                    foreach (ExeXml.Par aoPar in aoFun.pars.Values)
                    {
                        if (!parGroups.ContainsKey(aoPar.Group)) parGroups.Add(aoPar.Group, new List<ExeXml.Par>());
                        parGroups[aoPar.Group].Add(aoPar);
                    }

                    // check if the content of each group is ok
                    Description fDesc = new Description(addOn.polAOControl, aoFun); // just for error-messages
                    foreach (var parGroup in parGroups)
                    {
                        string extID = null, extName = null, extSwitch = null;
                        List<string> dataPatterns = new List<string>(), sysPatterns = new List<string>();
                        foreach (ExeXml.Par par in parGroup.Value)
                        {
                            if (par.val == DefPar.Value.NA) continue;
                            if (par.Name.ToLower() == DefPar.AddOn_ExtensionSwitch.Extension_Id.ToLower()) extID = par.val.Trim().ToLower();
                            else if (par.Name.ToLower() == DefPar.AddOn_ExtensionSwitch.Extension_Name.ToLower()) extName = par.val.Trim().ToLower();
                            else if (par.Name.ToLower() == DefPar.AddOn_ExtensionSwitch.Extension_Switch.ToLower()) extSwitch = par.val.Trim().ToLower();
                            else if (par.Name.ToLower() == DefPar.AddOn_ExtensionSwitch.Dataset.ToLower()) dataPatterns.Add(par.val.Trim());
                            else if (par.Name.ToLower() == DefPar.AddOn_ExtensionSwitch.System.ToLower()) sysPatterns.Add(par.val.Trim());
                            else MakeWarning($"unknown parameter {par.Name} is ignored", false);
                        }

                        if (extID == null && extName == null && extSwitch == null &&
                            dataPatterns.Count() == 0 && sysPatterns.Count() == 0) continue; // probably all set to n/a

                        if (extID == null && extName == null)
                        {
                            MakeWarning($"neither {DefPar.AddOn_ExtensionSwitch.Extension_Name} nor {DefPar.AddOn_ExtensionSwitch.Extension_Id} defined");
                            continue;
                        }

                        if (extID != null)
                        {
                            if (!infoStore.country.extensions.ContainsKey(extID)) { MakeWarning($"unknown extension id {extID}"); continue; }
                        }
                        if (extName != null)
                        {
                            if (extID == null)
                            {
                                extID = EM_ExtensionSwitchHandler.GetExtensionIdByName(extName, infoStore.country, infoStore.runConfig.pathHandler, infoStore.communicator);
                                if (extID == null) { MakeWarning($"unknown extension name {extName}"); continue; }
                            }
                            else MakeWarning($"{DefPar.AddOn_ExtensionSwitch.Extension_Name} is ignored, as {DefPar.AddOn_ExtensionSwitch.Extension_Id} is defined as well", false);

                            // note that no warning is issued, if a global extension exists, but there is no implementation for the country
                            // or even only no switch for the system-data-combination
                            // one could easily catch this, by some rearranging of code, but I think it's safer to avoid a maybe unwanted error-messsage
                        }

                        if (extSwitch != null)
                        {
                            if (extSwitch != DefPar.Value.ON && extSwitch != DefPar.Value.OFF)
                                { MakeWarning($"invalid value for {DefPar.AddOn_ExtensionSwitch.Extension_Switch}, must be {DefPar.Value.ON} or {DefPar.Value.OFF}"); continue; }
                        }
                        else { MakeWarning($"parameter {DefPar.AddOn_ExtensionSwitch.Extension_Switch} missing"); continue; }

                        bool patternMatches = dataPatterns.Count == 0;
                        foreach (string pattern in dataPatterns)
                            if (infoStore.IsUsedDatabase(pattern)) { patternMatches = true; break; }
                        if (!patternMatches) continue;

                        patternMatches = sysPatterns.Count == 0;
                        foreach (string pattern in sysPatterns)
                            if (EM_Helpers.DoesValueMatchPattern(pattern, infoStore.country.sys.name)) { patternMatches = true; break; }
                        if (!patternMatches) continue;

                        aoExtensionSwitches.Add(extID, extSwitch == DefPar.Value.ON);

                        void MakeWarning(string message, bool addIgnore = true)
                        {
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                            {
                                isWarning = true,
                                message = $"{fDesc.Get()} (group {parGroup.Key}): {message}" + (addIgnore ? ", switch-redefinition is ignored" : string.Empty)
                            });
                        }
                    }
                }

            EM_ExtensionSwitchHandler.TakeSwitches(infoStore.country, aoExtensionSwitches);
        }

        private void DropOff() // drop everything that is off (note: n/a was already removed by XMLHandler)
        {
            List<KeyValuePair<string, ExeXml.Pol>> delPols = new List<KeyValuePair<string, ExeXml.Pol>>();
            foreach (var pol in infoStore.country.cao.pols)
            {
                if (!pol.Value.on) { delPols.Add(pol); continue; }

                List<string> delFunIds = new List<string>();
                foreach (var fun in pol.Value.funs)
                    if (!fun.Value.on ||    // if function is off
                        (!fun.Value.pars.Any() && !fun.Value.Name.Equals(DefFun.RandSeed))      // or if function has no parameters, except randseed which works without parameters (TODO this should be more inclusive!!! break & scale work by accident, also think of future possible new functions without parameters)
                        ) delFunIds.Add(fun.Key);
                foreach (string delFunId in delFunIds) pol.Value.funs.Remove(delFunId);
            }
            foreach (var delPol in delPols) infoStore.country.cao.pols.Remove(delPol.Key);
        }
    }
}
