using EM_XmlHandler;
using System.Collections.Generic;
using System.Linq;
using EM_Common;
using System;

namespace EM_Executable
{
    public partial class Control
    {
        private void HandleExtensionSwitches(List<ExeXml.AddOn> addOns)
        {
            // first overwrite the default switch settings in the country file, if the user changed them (via config-settings sent by the run-tool)
            TakeExtensionSwitches(infoStore.runConfig.extensionSwitches);

            // then overwrite with any instructions from extensions, the means the priority of switches to be on or off is as follows:
            // highest: add-ons (via AddOn_ExtensionSwitch), next: user settings (via run-tool), lowest: default settings (via set-switches-dialog)
            TakeAddOnExtensionSwitches(addOns);

            // take care of a "feature" that is mainly there for compatibility with the version before March 2020, where the switches of functions within extension-policies were visible instead of set to switch
            // namly that such functions can be permanantly off instead of switchable
            List<ExeXml.Fun> permanentOffFunctions = TakeCareOfPermanentOffFunctions();

            // lists for taking into account that pol/fun/par can be switched by more than one extension and that 'on' always wins
            List<string> onIds = new List<string>(); 
            Dictionary<string, ExeXml.Fun> parsToRemove = new Dictionary<string, ExeXml.Fun>();

            foreach (var extension in infoStore.country.extensions.Values)
            {
                foreach (var extPol in extension.polIds)
                {
                    string polId = extPol.Key;
                    bool isBasePol = extPol.Value == true; // true: a base-policy, that is switched off if the extension is on
                    
                    if (!infoStore.country.cao.pols.ContainsKey(polId)) continue;
                    ExeXml.Pol pol = infoStore.country.cao.pols[polId];

                    bool ?switchOn = null;
                    if (extension.on == true) switchOn = isBasePol ? false : true; // extension-switch is on: added-as-on ✔: on; added-as-off ✘: off
                    else if (!isBasePol) switchOn = false; // extension-switch is off or n/a: added-as-on ✔: off; added-as-off ✘: do not touch

                    if (switchOn == true) pol.on = true;
                    if (switchOn == false && !onIds.Contains(polId)) pol.on = false;

                    // add the policy and all its functions and parameters, which do not have an own setting, to the switched-on (i.e. winning) elements
                    if (switchOn == true)
                    {
                        onIds.AddUnique(polId);
                        foreach (var f in pol.funs)
                        {
                            if (!extension.funIds.ContainsKey(f.Key))
                            {
                                string funId = f.Key; ExeXml.Fun fun = f.Value;
                                fun.on = true; onIds.AddUnique(funId);
                                foreach (var parId in fun.pars.Keys)
                                    if (!extension.parIds.ContainsKey(parId))
                                    {
                                        if (parsToRemove.ContainsKey(parId)) parsToRemove.Remove(parId);
                                        onIds.AddUnique(parId);
                                    }
                            }
                        }
                    }
                }
                foreach (var extFun in extension.funIds)
                {
                    string funId = extFun.Key;
                    bool isBaseFun = extFun.Value == true; // true: a base-function, that is switched off if the extension is on
                    
                    ExeXml.Fun fun = null;
                    foreach (var pol in infoStore.country.cao.pols)
                    {
                        var funs = from f in pol.Value.funs where f.Key == funId select f; if (!funs.Any()) continue;
                        fun = funs.First().Value;
                        if (fun.pars.Count == 0) fun = null; // this takes care of functions which are set to n/a including all their parameters for this system, i.e. ignores them
                        break;
                    }
                    if (fun == null) continue;

                    // see explanation above, however note: if parent-policy is switched off, all included functions are off as well
                    // if parent-policy is switched on, included functions can still be switched off
                    bool? switchOn = null;
                    if (extension.on == true) switchOn = isBaseFun ? false : true; // extension-switch is on: added-as-on ✔: on; added-as-off ✘: off
                    else if (!isBaseFun) switchOn = false; // extension-switch is off or n/a: added-as-on ✔: off; added-as-off ✘: do not touch

                    if (switchOn == true) fun.on = true;
                    if (switchOn == false && !onIds.Contains(funId)) fun.on = false;

                    // add the function and all its parameters, which do not have an own setting, to the switched-on (i.e. winning) elements
                    if (switchOn == true)
                    {
                        onIds.AddUnique(funId);
                        foreach (var parId in fun.pars.Keys)
                            if (!extension.parIds.ContainsKey(parId))
                            {
                                if (parsToRemove.ContainsKey(parId)) parsToRemove.Remove(parId);
                                onIds.AddUnique(parId);
                            }
                    }
                }
                foreach (var extPar in extension.parIds)
                {
                    string parId = extPar.Key; bool isBasePar = extPar.Value == true;

                    // other than for pol/fun we do not need the par, but its id and the parent-fun, because we need to delete the parameter in case 
                    ExeXml.Fun parentFun = null; 
                    foreach (var pol in infoStore.country.cao.pols)
                    {
                        foreach (var fun in pol.Value.funs)
                            if ((from p in fun.Value.pars where p.Key == parId select p).Any()) { parentFun = fun.Value; break; }
                        if (parentFun != null) break;
                    }
                    if (parentFun == null) continue;

                    bool? keepPar = null;
                    if (extension.on == true) keepPar = isBasePar ? false : true; // extension-switch is on: added-as-on ✔: keep par, added-as-off ✘: remove par
                    else if (!isBasePar) keepPar = false; // extension-switch is off or n/a: added-as-on ✔: remove par, added-as-off ✘: do not touch

                    if (keepPar == true && parsToRemove.ContainsKey(parId)) parsToRemove.Remove(parId);
                    if (keepPar == false && !onIds.Contains(parId)) parsToRemove.TryAdd(parId, parentFun);
                    if (keepPar == true) onIds.AddUnique(parId);
                }
            }
            foreach (var ptr in parsToRemove) ptr.Value.pars.Remove(ptr.Key); // remove the respective parameters only after taking all extensions into account 
            foreach (ExeXml.Fun pof in permanentOffFunctions) pof.on = false;
        }

        private List<ExeXml.Fun> TakeCareOfPermanentOffFunctions()
        {
            // make a list of permanently off functions within an extension-policy, to restore the off after all extensions are taken into account
            // i.e. switch them off again, if they were switched on by any extension
            List<ExeXml.Fun> permOff = new List<ExeXml.Fun>();
            foreach (var extension in infoStore.country.extensions.Values)
            {
                foreach (var extPol in extension.polIds)
                {
                    string polId = extPol.Key; bool baseOff = extPol.Value;
                    if (extension.on == false || baseOff == true ||
                        !infoStore.country.cao.pols.ContainsKey(polId)) continue;
                    foreach (ExeXml.Fun fun in infoStore.country.cao.pols[polId].funs.Values)
                        if (!fun.on) permOff.Add(fun);
                }
            }
            return permOff;
        }

        private void TakeExtensionSwitches(Dictionary<string, bool> extensionSwitches)
        {
            foreach (var extensionSwitch in extensionSwitches)
            {
                if (infoStore.country.extensions.ContainsKey(extensionSwitch.Key))
                    infoStore.country.extensions[extensionSwitch.Key].on = extensionSwitch.Value;
                //else // a programming-error: the run-tool should not show the switch, if a country doesn't implement it
                //    throw new Exception($"Unknown extension-id {extensionSwitch.Key}");
                else continue; // this can happen because the run-tool does in fact not look whether the concerned policy exists
                               // e.g. in a public version the switch-policy may be removed (e.g. Austrian web model)
            }                  // thus the transformer ignores the extension, while the run-tool still sends "switch it off"
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
                                extID = GetIdByName(extName);
                                if (extID == null) { MakeWarning($"unknown extension name {extName}"); continue; }
                            }
                            else MakeWarning($"{DefPar.AddOn_ExtensionSwitch.Extension_Name} is ignored, as {DefPar.AddOn_ExtensionSwitch.Extension_Id} is defined as well", false);

                            // note that no warning is issued, if a global extension exsits, but there is no implementation for the country
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

            TakeExtensionSwitches(aoExtensionSwitches);

            string GetIdByName(string name)
            {
                // first try if the name belongs to a local extension (where we have got the name, as it is stored in the country file) ...
                var lext = from e in infoStore.country.extensions
                           where (e.Value.localLongName != null && e.Value.localLongName.ToLower() == name) ||
                                 (e.Value.localShortName != null && e.Value.localShortName.ToLower() == name)
                           select e;
                if (lext.Count() > 0) return lext.First().Key;

                // ... only if not found, need to read gloabl extensions
                var gext = from e in ExeXmlReader.ReadExtensions(infoStore.runConfig.pathHandler.GetExtensionsFilePath(), infoStore.communicator)
                           where (e.Value.Item1 != null && e.Value.Item1.ToLower() == name) ||
                                 (e.Value.Item2 != null && e.Value.Item2.ToLower() == name)
                           select e;
                return gext.Count() > 0 ? gext.First().Key : null;
            }
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
