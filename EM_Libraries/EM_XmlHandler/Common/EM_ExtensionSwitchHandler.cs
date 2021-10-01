using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_XmlHandler
{
    public class EM_ExtensionSwitchHandler
    {
        public static void ApplySwitches(ExeXml.Country country, Dictionary<string, bool> nonDefaultSwitches = null)
        {
            // allows for (other programmes than the executable for) providing any changes to the default switches (e.g. via the run-tool)
            TakeSwitches(country, nonDefaultSwitches);

            // take care of a "feature" that is mainly there for compatibility with the version before March 2020, where the switches of functions within extension-policies were visible instead of set to switch
            // namly that such functions can be permanantly off instead of switchable
            List<ExeXml.Fun> permanentOffFunctions = TakeCareOfPermanentOffFunctions(country);

            // lists for taking into account that pol/fun/par can be switched by more than one extension and that 'on' always wins
            List<string> onIds = new List<string>();
            Dictionary<string, ExeXml.Fun> parsToRemove = new Dictionary<string, ExeXml.Fun>();

            foreach (var extension in country.extensions.Values)
            {
                foreach (var extPol in extension.polIds)
                {
                    string polId = extPol.Key;
                    bool isBasePol = extPol.Value == true; // true: a base-policy, that is switched off if the extension is on

                    if (!country.cao.pols.ContainsKey(polId)) continue;
                    ExeXml.Pol pol = country.cao.pols[polId];

                    bool? switchOn = null;
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
                    foreach (var pol in country.cao.pols)
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
                    foreach (var pol in country.cao.pols)
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

        private static List<ExeXml.Fun> TakeCareOfPermanentOffFunctions(ExeXml.Country country)
        {
            // make a list of permanently off functions within an extension-policy, to restore the off after all extensions are taken into account
            // i.e. switch them off again, if they were switched on by any extension
            List<ExeXml.Fun> permOff = new List<ExeXml.Fun>();
            foreach (var extension in country.extensions.Values)
            {
                foreach (var extPol in extension.polIds)
                {
                    string polId = extPol.Key; bool baseOff = extPol.Value;
                    if (extension.on == false || baseOff == true ||
                        !country.cao.pols.ContainsKey(polId)) continue;
                    foreach (ExeXml.Fun fun in country.cao.pols[polId].funs.Values)
                        if (!fun.on) permOff.Add(fun);
                }
            }
            return permOff;
        }

        public static void TakeSwitches(ExeXml.Country country, Dictionary<string, bool> extensionSwitches)
        {
            if (extensionSwitches == null) return;
            foreach (var extensionSwitch in extensionSwitches)
            {
                if (country.extensions.ContainsKey(extensionSwitch.Key))
                    country.extensions[extensionSwitch.Key].on = extensionSwitch.Value;
                //else // a programming-error: the run-tool should not show the switch, if a country doesn't implement it
                //    throw new Exception($"Unknown extension-id {extensionSwitch.Key}");
                else continue; // this can happen because the run-tool does in fact not look whether the concerned policy exists
                               // e.g. in a public version the switch-policy may be removed (e.g. Austrian web model)
            }                  // thus the transformer ignores the extension, while the run-tool still sends "switch it off"
        }

        public static string GetExtensionIdByName(string name, ExeXml.Country country, EMPath emPath, Communicator communicator)
        {
            // first try if the name belongs to a local extension (where we have got the name, as it is stored in the country file) ...
            var lext = from e in country.extensions
                       where (e.Value.localLongName != null && e.Value.localLongName.ToLower() == name.ToLower()) ||
                             (e.Value.localShortName != null && e.Value.localShortName.ToLower() == name.ToLower())
                       select e;
            if (lext.Any()) return lext.First().Key;

            // ... only if not found, need to read gloabl extensions
            var gext = from e in ExeXmlReader.ReadExtensions(emPath.GetExtensionsFilePath(), communicator)
                       where (e.Value.Item1 != null && e.Value.Item1.ToLower() == name.ToLower()) ||
                             (e.Value.Item2 != null && e.Value.Item2.ToLower() == name.ToLower())
                       select e;
            return gext.Any() ? gext.First().Key : null;
        }
    }
}
