using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using EM_XmlHandler;

namespace EM_Executable
{
    public partial class Control
    {
        // implementation of new function ChangeSwitch, which partly replaces run-time changes of ChangeParam (see ApplyChangeParam)
        // parameters provided:
        // - optional parameter RunCond (if present: run-time switch-change, if not present: read-time switch-change)
        // - parameters for switching on/off policies/functions:
        //   parameter-group: PolFun: id/name of policy/function
        //                    SwitchNewVal: on/off
        // examples: Run_Cond = { $ScenarioX = 1 }, PolFun (Group 1) = bsa_cy, NewSwitch (Group 1) = off,
        //                                          PolFun (Group 2) = A18317A2-25AC-4E58-AFA2-92E80623550F, NewSwitch (Group 2) = on
        //           (in an AddOn:) Run_Cond = { $Sum_xyz > 100 }, PolFun (Group 1) = tco_cy, NewSwitch (Group 1) = off,
        //                                                         PolFun (Group 2) = tin_cy#1, NewSwitch (Group 2) = off
        // (note: symbolic IDs are already replaced at this stage(by add-on integration))
        private void ApplyChangeSwitch()
        {
            foreach (var cs in GetSpecificFuns(DefFun.ChangeSwitch))
            {
                ExeXml.Fun fun = cs.Value; string funId = cs.Key;

                // gather the information by reading parameters: there may be a Run_Cond (optional),
                // the other parameters are a list of switch changes, defined by the parameters PolFun (=id) and SwitchNewVal (=on/off)
                ReadInfo(fun, funId, out Dictionary<string, Tuple<string, string>> changes,
                                                    out ExeXml.Par parRunCond, out List<ExeXml.Par> parDatabaseName);

                // gather the functions which are affected by the switch-changes (i.e. all functions of a policy or a specific function)
                // and check for unknown ids or invalid switches
                CheckInfo(funId, changes, out Dictionary<ExeXml.Fun, bool> switchInfo);

                // apply the switch changes, taking run-cond into account
                ApplyInfo(switchInfo, parRunCond, parDatabaseName);
                
                // finally, switch ChangeSwitch-function off: thus DropOff will delete it
                fun.on = false;
            }
        }

        private void ReadInfo(ExeXml.Fun fun, string funId, out Dictionary<string, Tuple<string, string>> changes,
                              out ExeXml.Par parRunCond, out List<ExeXml.Par> parDatabaseName)
        {
            parRunCond = null; parDatabaseName = new List<ExeXml.Par>();
            changes = new Dictionary<string, Tuple<string, string>>(); // key: group, value.item1: fun/pol-ident, value.item2: on/off           
            foreach (var par in fun.pars.Values)
            {
                if (par.Name.ToLower() == DefPar.Common.Run_Cond.ToLower()) // Run_Cond = { xxx }
                    parRunCond = par;
                else if (par.Name.ToLower() == DefQuery.Par.DataBasename.ToLower()) // #_DatabaseName is the only footnote-parameter possible
                    parDatabaseName.Add(par);                                       // for global conditions (i.e. the Run_Cond)
                else if (par.Name.ToLower() == DefPar.ChangeSwitch.PolFun.ToLower()) // PolFun = Guid / PolFun = policy-name
                    AddToChangeList(changes, par.Group, par.val, false, funId);
                else if (par.Name.ToLower() == DefPar.ChangeSwitch.Switch_NewVal.ToLower()) // SwitchNewVal = on/off
                    AddToChangeList(changes, par.Group, par.val, true, funId);
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = false,
                    message = $"Change switch ({funId}): unknown parameter {par.Name}"
                });
            }

            if (parRunCond == null) return;

            // make sure that footnote-parameters (i.e. #_DatabasName) do not get into conflict with existing footnote-parameters (i.e. same group)
            List<int> replaceGroups = new List<int>();
            foreach (ExeXml.Par parDB in parDatabaseName)
            {
                if (!int.TryParse(parDB.Group, out int g)) continue; // ignore this rather unlikely case (just keep the strange group)
                parDB.Group = GetNewGroup(g).ToString(); replaceGroups.Add(g);
            }
            replaceGroups.Sort(); replaceGroups.Reverse(); // avoid replacing ...#1...#12... with ...#9901...#99012...
            foreach (int group in replaceGroups)           // and hope there is no #9
                parRunCond.val = parRunCond.val.Replace($"#{group}", $"#{GetNewGroup(group)}");

            int GetNewGroup(int group) { return group + 9900; }
        }

        // used above (i.e. for ChangeSwitch) as well as in Control_ChangeParam.cs (i.e. for ChangeParam)
        private void AddToChangeList(Dictionary<string, Tuple<string, string>> changeList,
                                     string group, string item, bool isItem2, string funId)
        {
            if (changeList.ContainsKey(group)) // was the other part of the group already found?
            {
                if ((isItem2 && changeList[group].Item2 == null) || // is the current info already available for this group?
                    (!isItem2 && changeList[group].Item1 == null))
                    changeList[group] = new Tuple<string, string>(isItem2 ? changeList[group].Item1 : item,
                                                                  isItem2 ? item : changeList[group].Item2);
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = false,
                    message = $"Change param ({funId}): unclear definition of group {group}"
                });
            }
            else changeList.Add(group, isItem2 ? new Tuple<string, string>(null, item) : new Tuple<string, string>(item, null));
        }

        private void CheckInfo(string switchFunId, Dictionary<string, Tuple<string, string>> changes,
                               out Dictionary<ExeXml.Fun, bool> switchInfo)
        {
            switchInfo = new Dictionary<ExeXml.Fun, bool>(); // the final switch is set in context with the run-cond, here only gather info
            foreach (var change in changes)
            {
                string group = change.Key, polFunIdent = change.Value.Item1, onOff = change.Value.Item2; bool on = true;
                if (polFunIdent == null || onOff == null)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"Change param ({switchFunId}): insufficient definition of group {group}" });
                    continue;
                }

                switch (onOff)
                {
                    case DefPar.Value.ON: on = true; break;
                    case DefPar.Value.OFF: on = false; break;
                    default: infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                                message = $"Change switch ({switchFunId}): invalid change {onOff}" }); continue;
                }

                bool found = false;
                foreach (var p in infoStore.country.cao.pols)
                {
                    ExeXml.Pol pol = p.Value; string polId = p.Key;
                    if (polId.ToLower() == polFunIdent.ToLower() || pol.name.ToLower() == polFunIdent.ToLower()) // policy identified by id or name
                    {
                        if (pol.on != on) // otherwise nothing to do: change from on to on or off to off
                        {
                            foreach (ExeXml.Fun fun in pol.funs.Values)
                            {
                                if (!fun.on) continue; // a switched off function inside the policy is off, whatever the switch is set to
                                fun.on = pol.on; // the fun (currently on) must take the switch of the pol, because the pol is switched on (see below)
                                switchInfo.TryAdd(fun, on);
                            }
                            pol.on = true; // the policy must be on, otherwise the function-switches have no effect
                        }
                        found = true; break;
                    }

                    if (p.Value.funs.ContainsKey(polFunIdent)) // function (must be) identfied by id
                    {
                        if (p.Value.funs[polFunIdent].on != on) switchInfo.Add(p.Value.funs[polFunIdent], on);
                        found = true; break;
                    }
                }
                if (!found) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"Change switch ({switchFunId}): identifier {polFunIdent} not found" });
            }
        }

        private void ApplyInfo(Dictionary<ExeXml.Fun, bool> switchInfo, ExeXml.Par switchFunRunCond, List<ExeXml.Par> switchFunDatabaseName)
        {
            foreach (var si in switchInfo)
            {
                ExeXml.Fun fun = si.Key; bool on = si.Value;

                // read-time change - simply change the switch as required
                if (switchFunRunCond == null) { fun.on = on; continue; }
                
                // run-time change: implement switch by editing or adding the function's RunCond
                var hasRunCond = from p in fun.pars.Values
                                 where p.Name.ToLower() == DefPar.Common.Run_Cond.ToLower()
                                 select p;
                // function does not yet have a RunCond: add a copy of the ChangeSwitch-RunCond
                if (hasRunCond == null || hasRunCond.Count() == 0)
                {
                    if (fun.Name.ToLower() != DefFun.DefTu.ToLower() && // DefTU and DefIL do not accept a RunCond (does not make sense)
                        fun.Name.ToLower() != DefFun.DefIl.ToLower())   // and it does no harm if these definitions exist
                    {
                        ExeXml.Par newRunCond = new ExeXml.Par() { Name = DefPar.Common.Run_Cond };
                        // function is currently on, switch off if RunCon is fulfilled
                        // i.e. run as long as ChangeSwitch-RunCond is not fulfilled
                        if (fun.on) newRunCond.val = $"!({switchFunRunCond.val})";
                        // function is currently off, switch on if RunCond is fulfilled
                        // needs to be switched on, otherwise it is dropped by DropOff
                        // but do not run until ChangeSwitch-RunCond is fulfilled
                        else { fun.on = true; newRunCond.val = switchFunRunCond.val; }
                        fun.pars.Add(Guid.NewGuid().ToString(), newRunCond);
                    }
                }
                // function has a RunCond: merge with the ChangeSwitch-RunCond
                else
                {
                    ExeXml.Par runCond = hasRunCond.First();
                    // function is currently on, switch off if RunCon is fulfilled
                    // run if current RunCond is fulfilled and as long as ChangeSwitch-RunCond is not fulfilled
                    if (fun.on) runCond.val = $"!({switchFunRunCond.val}) & ({runCond.val})";
                    // function is currently off, switch on if RunCond is fulfilled
                    // needs to be switched on, otherwise it is dropped by DropOff
                    // but do not run until ChangeSwitch-RunCond is fulfilled, but still also fulfill current RunCond
                    else { fun.on = true; runCond.val = $"({switchFunRunCond.val}) & ({runCond.val})"; }
                }
                // transfer a possible #_DatabaseName-footnote from the switch-function to the function it is applied on
                foreach (ExeXml.Par dbName in switchFunDatabaseName) fun.pars.Add(Guid.NewGuid().ToString(), dbName);
            }
        }
    }
}
