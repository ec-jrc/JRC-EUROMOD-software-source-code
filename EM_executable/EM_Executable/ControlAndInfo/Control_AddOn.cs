using EM_Common;
using System.Collections.Generic;
using System.Linq;
using EM_XmlHandler;
using System;

namespace EM_Executable
{
    public partial class Control
    {
        private void IntegrateAddOns(List<ExeXml.AddOn> addOns)
        {
            // allows for integrating more than one add-on, with order being relevant,
            // i.e. any add-on integrated first, is regarded as "normal" part of the country, for an add-on integrated afterwards
            foreach (var addOn in addOns) IntegrateAddOn(addOn);
        }

        private void IntegrateAddOn(ExeXml.AddOn addOn)
        {
            // replace =cc= and =sys=
            ReplaceCCSys(addOn.cao.pols);
            ReplaceCCSys(addOn.polAOControl);

            // replace symbolic ids (e.g. output_std_fr_#1.1) by their GUID in the country-file
            ReplaceSymbolicIDs(addOn);

            // integrate the add-on elements by interpreting the AddOn_Pol/Func/Par functions in the ao_control policy 
            foreach (ExeXml.Fun addOnFun in addOn.polAOControl.funs.Values.OrderBy(o => o.order))
            {
                if (!addOnFun.on) continue;
                Description funDescription = new Description(addOn.polAOControl, addOnFun);
                if (addOnFun.Name.ToLower() == DefFun.AddOn_Pol.ToLower()) IntegratePol(addOn, addOnFun, funDescription);
                else if (addOnFun.Name.ToLower() == DefFun.AddOn_Func.ToLower()) IntegrateFun(addOn, addOnFun, funDescription);
                else if (addOnFun.Name.ToLower() == DefFun.AddOn_Par.ToLower()) IntegratePar(addOnFun, funDescription);
                else if (addOnFun.Name.ToLower() == DefFun.AddOn_ExtensionSwitch.ToLower()) continue; // already handled in Control.TakeAddOnExtensionSwitches
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{funDescription.Get()}: invalid add-on function: {addOnFun.Name}" });
            }
        }

        internal void IntegratePol(ExeXml.AddOn addOn, ExeXml.Fun addOnPol, Description funDescription)
        {
            string polName = null; // the name of the policy to add
            string linkPolName = null; // the name of the policy after or before which to add the policy
            bool before = true;

            // interpret the parameters of the AddOn_Pol function
            foreach (var par in addOnPol.pars.Values)
            {
                if (par.val == DefPar.Value.NA) continue;
                Description parDescription = new Description(funDescription, par);
                if (par.Name.ToLower() == DefPar.AddOn_Pol.Pol_Name.ToLower()) polName = par.val;
                else if (par.Name.ToLower() == DefPar.AddOn_Pol.Insert_Before_Pol.ToLower()) linkPolName = par.val;
                else if (par.Name.ToLower() == DefPar.AddOn_Pol.Insert_After_Pol.ToLower()) { linkPolName = par.val; before = false; }
                else if (par.Name.ToLower() == DefPar.AddOn_Pol.Allow_Duplicates.ToLower()) { } // do nothing, just allow duplicates always
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{parDescription.Get()}: unknown parameter: {par.Name}" });
            }

            // check for compulsory parameters
            if (polName == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{funDescription.Get()}: parameter {DefPar.AddOn_Pol.Pol_Name} not defined" });
                return;
            }
            if (linkPolName == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{funDescription.Get()}: neither {DefPar.AddOn_Pol.Insert_Before_Pol} nor {DefPar.AddOn_Pol.Insert_After_Pol} defined" });
                return;
            }

            // integrate the policy ...
            // ... in the country file, find the policy after or before which to integrate the policy
            if (!FindPol(addOn, linkPolName, infoStore.country.cao.pols, out ExeXml.Pol linkPol, out string dummy)) return;
            // ... in the add-on file, find the policy to add 
            if (!FindPol(addOn, polName, addOn.cao.pols, out ExeXml.Pol pol, out string polID)) return;
            // ... check if policy was added before, if yes, make a clone with different ids
            if (infoStore.country.cao.pols.ContainsKey(polID)) { polID = Guid.NewGuid().ToString(); pol = ClonePol(pol); }
            // ... find an appropriate order
            pol.order = MakeOrder(linkPol.order, (from p in infoStore.country.cao.pols.Values select p.order).ToList(), before);
            // ... add the policy
            infoStore.country.cao.pols.Add(polID, pol);
        }

        private ExeXml.Pol ClonePol(ExeXml.Pol pol)
        {
            ExeXml.Pol clone = new ExeXml.Pol { name = pol.name, on = pol.on, order = pol.order };
            foreach (ExeXml.Fun fun in pol.funs.Values) clone.funs.Add(Guid.NewGuid().ToString(), CloneFun(fun));
            return clone;
        }

        private ExeXml.Fun CloneFun(ExeXml.Fun fun)
        {
            ExeXml.Fun clone = new ExeXml.Fun { Name = fun.Name, on = fun.on, order = fun.order };
            foreach (ExeXml.Par par in fun.pars.Values)
            {
                ExeXml.Par clonePar = new ExeXml.Par() { Name = par.Name, Group = par.Group, order = par.order, val = par.val };
                clone.pars.Add(Guid.NewGuid().ToString(), clonePar);
            }
            return clone;
        }

        private void IntegrateFun(ExeXml.AddOn addOn, ExeXml.Fun addOnFun, Description funDescription)
        {
            string funID = null; // the id of the function to add
            string linkFunID = null; // the id of the function after or before which to add the function
            bool before = true;

            // interpret the parameters of the AddOn_Func function
            foreach (var par in addOnFun.pars.Values)
            {
                if (par.val == DefPar.Value.NA) continue;
                Description parDescription = new Description(funDescription, par);
                if (par.Name.ToLower() == DefPar.AddOn_Func.Id_Func.ToLower()) funID = par.val;
                else if (par.Name.ToLower() == DefPar.AddOn_Func.Insert_Before_Func.ToLower()) linkFunID = par.val;
                else if (par.Name.ToLower() == DefPar.AddOn_Func.Insert_After_Func.ToLower()) { linkFunID = par.val; before = false; }
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = true, message = $"{parDescription.Get()}: unknown parameter: {par.Name}" });
            }

            // check for compulsory parameters
            if (funID == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{funDescription.Get()}: parameter {DefPar.AddOn_Func.Id_Func} not defined" });
                return;
            }
            if (linkFunID == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{funDescription.Get()}: neither {DefPar.AddOn_Func.Insert_Before_Func} nor {DefPar.AddOn_Func.Insert_After_Func} defined" });
                return;
            }

            // integrate the function ... 
            // ... in the country file, find the function after or before which to integrate the function
            if (!FindFun(linkFunID, infoStore.country.cao.pols, funDescription, out ExeXml.Fun linkFun, out ExeXml.Pol parentPol)) return;
            // ... in the add-on file, find the function to add
            if (!FindFun(funID, addOn.cao.pols, funDescription, out ExeXml.Fun fun, out ExeXml.Pol dummy)) return;
            // ... check if function was added before, if yes, make a clone with different ids
            if (!parentPol.funs.ContainsKey(funID)) { funID = Guid.NewGuid().ToString(); fun = CloneFun(fun); }
            // ... find an appropriate order
            fun.order = MakeOrder(linkFun.order, (from f in parentPol.funs.Values select f.order).ToList(), before);
            // ... add the function
            parentPol.funs.Add(funID, fun);
        }

        private void IntegratePar(ExeXml.Fun addOnPar, Description funDescription)
        {
            string insertFunID = null;
            Dictionary<string, ExeXml.Par> parToAdd = new Dictionary<string, ExeXml.Par>();

            // interpret the parameters of the AddOn_Par function
            foreach (var p in addOnPar.pars)
            {
                if (p.Value.val == DefPar.Value.NA) continue;
                if (p.Value.Name.ToLower() == DefPar.AddOn_Par.Insert_Func.ToLower()) insertFunID = p.Value.val;
                else parToAdd.Add(p.Key, p.Value); // any other parameter is an add-on-parameter (i.e. should be added to the insert-function)
            }
            // check for the only compulsory parameter
            if (insertFunID == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{funDescription.Get()}: parameter {DefPar.AddOn_Par.Insert_Func} not defined" });
                return;
            }

            // find the insert-function in the country file
            if (!FindFun(insertFunID, infoStore.country.cao.pols, funDescription, out ExeXml.Fun insertFun, out ExeXml.Pol dummy)) return;
            // add the parameters to the insert-function (note: order of parameters is irrelevant for the executable)
            insertFun.pars.AddRange(parToAdd);
        }

        private bool FindPol(ExeXml.AddOn addOn, string polName, Dictionary<string, ExeXml.Pol> pols,
                             out ExeXml.Pol foundPol, out string foundPolID)
        {
            foreach (var p in pols)
                if (p.Value.name.ToLower() == polName.ToLower())
                    { foundPol = p.Value; foundPolID = p.Key; return true; }

            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                message = $"{addOn.cao.shortName}: policy {polName} not found" });
            foundPol = null; foundPolID = null; return false;
        }

        private bool FindFun(string funID, Dictionary<string, ExeXml.Pol> pols, Description description,
                             out ExeXml.Fun foundFun, out ExeXml.Pol parentPol)
        {
            foreach (ExeXml.Pol pol in pols.Values)
                if (pol.funs.ContainsKey(funID))
                    { foundFun = pol.funs[funID]; parentPol = pol; return true; }

            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                message = $"{description.Get()}: unknown function-id {funID}" });
            foundFun = null; parentPol = null; return false;
        }

        private double MakeOrder(double refOrder, List<double> existingOrders, bool before)
        {
            existingOrders.Sort();
            int iRef = existingOrders.IndexOf(refOrder);
            double nextOrder = before ? (iRef - 1 < 0 ? 0 : existingOrders[iRef - 1])
                                      : (iRef + 1 == existingOrders.Count ? refOrder + 1 : existingOrders[iRef + 1]);
            return (refOrder + nextOrder) / 2;
        }

        private void ReplaceSymbolicIDs(ExeXml.AddOn addOn)
        {
            // make a Dictonary with symbolic-id as key and guid-id as value, for all functions and parameters
            // symbolic-ids look like this: for functions: polName_#funOrder, e.g. neg_fr_#2, for parameters: polName_#funOrder.parOrder, e.g. neg_fr_#2.2
            // special case DefTU and DefIL, which look like this: polName_#tuilName[.parOrder], e.g. IlsDef_fr_#ils_dispy respectively TUDef_fr_#tu_hh_oecd_co.2
            Dictionary<string, string> allIds = new Dictionary<string, string>();
            foreach (var pol in infoStore.country.cao.pols.Values)
                foreach (var fun in pol.funs)
                {
                    string iltuName = null; // take care of special case as noted above
                    if (fun.Value.Name.ToLower() == DefFun.DefIl.ToLower() || fun.Value.Name.ToLower() == DefFun.DefTu.ToLower())
                    {
                        var np = from p in fun.Value.pars where p.Value.Name.ToLower() == DefPar.DefIl.Name.ToLower() select p; // is also 'name' for DefTU
                        if (np.Count() > 0) iltuName = np.First().Value.val;
                    }
                    allIds.TryAdd(AddOn.ComposeSymbolicID(pol.name, fun.Value.order.ToString()), fun.Key);     // allow both: special and "regular" version
                    if (iltuName != null) allIds.TryAdd(AddOn.ComposeSymbolicID(pol.name, iltuName), fun.Key); // e.g. TUDef_fr_#tu_hh_oecd_co and TUDef_fr_#5
                    foreach (var par in fun.Value.pars)
                    {
                        allIds.TryAdd(AddOn.ComposeSymbolicID(pol.name, fun.Value.order.ToString(), par.Value.order), par.Key);
                        if (iltuName != null) allIds.TryAdd(AddOn.ComposeSymbolicID(pol.name, iltuName, par.Value.order), par.Key);
                    }
                }

            foreach (var aoPol in addOn.cao.pols) ReplaceSymbolicIDs(aoPol.Value);
            ReplaceSymbolicIDs(addOn.polAOControl);

            void ReplaceSymbolicIDs(ExeXml.Pol pol)
            {
                foreach (var fun in pol.funs.Values)
                    foreach (ExeXml.Par par in fun.pars.Values)
                        if (allIds.ContainsKey(par.val.ToLower())) par.val = allIds[par.val.ToLower()]; // if is a symbolic-id replace by guid-id
            }
        }
    }
}
