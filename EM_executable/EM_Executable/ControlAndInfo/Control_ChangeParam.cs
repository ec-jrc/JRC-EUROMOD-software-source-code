using EM_Common;
using System.Collections.Generic;
using EM_XmlHandler;
using System;

namespace EM_Executable
{
    public partial class Control
    {
        // important note on new handling of ChangeParam:
        // - ChangeParam will only allow for read-time changes and does not allow for changing switches anymore
        // - a new function ChangeSwitch will allow for changing switches of policies/functions at run- or read-time
        //
        // justification:
        // - the only reason for run-time parameter changes are loops:
        //   as far as I can see this option is only used by MTR and LMA to switch policies/functions on/off
        // - other applications just 'misuse' Param_CondVal (AT, BE), i.e. they do not provide a RunCond and thus c(sh)ould use Param_NewVal
        //   (e.g. AT switches stuff off for SORESI, in a ChangeParam located in a policy that is only on for SORESI)
        //
        // actual reason for the abolishment:
        // full application of run-time parameter changes is very complex (close to impossible, consider e.g. formula-interpretation ...)


        // changes to old ChangeParam:
        // - parameter Param_CondVal is abolished (i.e. no run-time changes anymore - but see ApplyChangeSwitch),
        //   respectively renamed to Param_NewVal where the change actually concerns a read-time-change
        // - parameter RunCond is abolished, respectively replaced by 
        // - parameter Database (non-unique): usage like in Uprate/SetDefault, except not provided means apply for any dataset here
        // examples: Database = sk_2019, Param_Id = 9ac9d5e6-bbfe-4e5c-a87e-940b33a5457e, Param_NewVal = adp*0.5 (e.g. if adp exists only in this data)
        //           (in an AddOn:) Param_Id = yem_sk_#2.1, Param_NewVal = { dag <= 6 }
        // (note: symbolic IDs are already replaced at this stage (by add-on integration))
        private void ApplyChangeParam()
        {
            foreach (var cp in GetSpecificFuns(DefFun.ChangeParam))
            {
                ExeXml.Fun cpFun = cp.Value; string cpFunId = cp.Key;

                // gather the information (consisting of whether the function applies to the current dataset and the list of changes)
                Dictionary<string, Tuple<string, string>> changeList = new Dictionary<string, Tuple<string, string>>();
                List<string> applyDatasets = new List<string>();
                foreach (ExeXml.Par par in cpFun.pars.Values)
                {
                    if (par.Name.ToLower() == DefPar.ChangeParam.Dataset.ToLower())
                        applyDatasets.Add(par.val);
                    else if (par.Name.ToLower() == DefPar.ChangeParam.Param_Id.ToLower())
                        AddToChangeList(changeList, par.Group, par.val, false, cpFunId);
                    else if (par.Name.ToLower() == DefPar.ChangeParam.Param_NewVal.ToLower())
                        AddToChangeList(changeList, par.Group, par.val, true, cpFunId);
                    else infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"Change param ({cpFunId}): unknown parameter {par.Name}" });
                }

                // check if the ChangeParam applies to the currently used dataset
                bool applies = applyDatasets.Count == 0; // if there is no Dataset parameter the change applies (see explanation above)
                foreach (string dataName in applyDatasets) if (infoStore.IsUsedDatabase(dataName)) { applies = true; break; }

                if (applies)
                {
                    // check if the IDs exist and (if yes) apply the changes
                    foreach (var change in changeList) // remark: if there are many changes, it may be more efficient to
                    {                                  // loop only once over all pars, but I assume that's not very likely
                        string group = change.Key, changeId = change.Value.Item1, changeNewVal = change.Value.Item2;
                        if (changeId == null || changeNewVal == null)
                        {
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                                message = $"Change param ({cpFunId}): insufficient definition of group {group}" });
                            continue;
                        }

                        bool found = false;
                        foreach (var pol in infoStore.country.cao.pols.Values)
                        {
                            foreach (var fun in pol.funs.Values)
                            {
                                if (fun.pars.ContainsKey(changeId)) { fun.pars[changeId].val = changeNewVal; found = true; break; }
                                if (found) break;
                            }
                            if (found) break;
                        }

                        if (!found) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"Change param ({cpFunId}): parameter id {changeId} not found" });
                    }
                }

                // finally, switch ChangeParam-function off: thus DropOff will delete it
                cpFun.on = false;
            }
        }

        private List<KeyValuePair<string, ExeXml.Fun>> GetSpecificFuns(string funName)
        {
            List<KeyValuePair<string, ExeXml.Fun>> funs = new List<KeyValuePair<string, ExeXml.Fun>>();
            foreach (var pol in infoStore.country.cao.pols)
            {
                if (!pol.Value.on) continue;
                foreach (var fun in pol.Value.funs)
                    if (fun.Value.Name.ToLower() == funName.ToLower() && fun.Value.on)
                        funs.Add(new KeyValuePair<string, ExeXml.Fun>(fun.Key, fun.Value));
            }
            return funs;
        }
    }
}
