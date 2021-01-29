using EM_Common;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    public partial class Control
    {
        private bool HandleBreak()
        {
            // CHECK IF THERE IS ANY BREAK FUNCTION ...
            string breakFunId = null; ExeXml.Fun breakFun = null; double breakFunOrder = double.MaxValue; Description funDesc = null;
            foreach (var pol in infoStore.country.cao.pols)
            {
                if (!pol.Value.on) continue;
                foreach (var fun in pol.Value.funs)
                    if (fun.Value.on && fun.Value.Name.ToLower() == DefFun.Break.ToLower())
                    {
                        double o = GetOrder(pol.Value.order, fun.Value.order);
                        if (o < breakFunOrder) // only consider 1stBreak in spine (if there are foolishly more)
                        {
                            breakFunId = fun.Key; breakFun = fun.Value;
                            breakFunOrder = o; funDesc = new Description(pol.Value, breakFun, breakFunId);
                        }
                    }
            }
            if (breakFun == null) return true;

            // ... IF SO, CHECK PARAMETERS
            bool produceOutput = true, produceTUinfo = false; string outputFileName = string.Empty;
            foreach (var p in breakFun.pars)
            {
                string parID = p.Key; ExeXml.Par par = p.Value;
                if (par.val.ToLower() == DefPar.Value.NA) continue;
                Description parDesc = new Description(funDesc, par, parID);

                if (par.Name.ToLower() == DefPar.Break.ProduceOutput.ToLower() || par.Name.ToLower() == DefPar.Break.ProduceTUinfo.ToLower())
                {
                    bool produceX = false;
                    if (par.val.ToLower() == DefPar.Value.YES) produceX = true;
                    else if (par.val.ToLower() == DefPar.Value.NO) produceX = false;
                    else IssueErr(parDesc, "invalid value (must be yes or no)");
                    if (par.Name.ToLower() == DefPar.Break.ProduceOutput.ToLower()) produceOutput = produceX; else produceTUinfo = produceX;
                }
                else if (par.Name.ToLower() == DefPar.Break.OutputFileName.ToLower()) outputFileName = par.val;
                else IssueErr(parDesc, "unknown parameter");

            }

            if (produceOutput == false && produceTUinfo == true) IssueErr(funDesc, "TUinfo can only be produced if output is produced (parameter is ignored)", true);

            if (infoStore.communicator.errorCount > 0) return false;

            // IMPLEMENT THE BREAK
            // - main task: switch off everything after the Break function
            // - and, if necessary, search for DefOutput that can provide the output-filename (if not set by parameter OutputFileName)
            // - and, if necessary, find all TU names
            double outputFunOrder = produceOutput && outputFileName == string.Empty ? double.MaxValue : -1; // the default for OutputFileName should be the first "normal" outputfile
            List<string> tuNames = new List<string>(); string indTUName = string.Empty;
            foreach (var pol in infoStore.country.cao.pols.Values)
            {
                if (!pol.on) continue;
                foreach (var fun in pol.funs.Values)
                {
                    if (!fun.on) continue;
                    double funOrder = GetOrder(pol.order, fun.order);
                    if (funOrder > breakFunOrder) fun.on = false; // switch off everything after the Break function

                    if (fun.Name.ToLower() == DefFun.DefOutput.ToLower() &&
                        funOrder < outputFunOrder && funOrder > breakFunOrder) // get the File paramter of the first DefOutput after the Break
                    {
                        var fn = from p in fun.pars.Values where p.Name.ToLower() == DefPar.DefOutput.File.ToLower() select p.val;
                        if (fn.Count() > 0) { outputFileName = fn.First(); outputFunOrder = funOrder; }
                    }

                    // if necessary, find at least one individual TU for output function, or all (active) TUs if TUinfo should be produced
                    if (!fun.on || !produceOutput || fun.Name.ToLower() != DefFun.DefTu.ToLower()) continue;

                    var tuNamePar = from p in fun.pars.Values where p.Name.ToLower() == DefPar.DefTu.Name.ToLower() select p.val;
                    var tuTypePar = from p in fun.pars.Values where p.Name.ToLower() == DefPar.DefTu.Type.ToLower() select p.val;
                    if (tuNamePar.Count() == 0 || tuTypePar.Count() == 0) continue;
                    if (produceTUinfo) tuNames.Add(tuNamePar.First());
                    if (indTUName == string.Empty && tuTypePar.First().ToUpper() == DefPar.Value.TUTYPE_IND) indTUName = tuNamePar.First();
                }
            }

            // if necessary produce DefOutput function in redefining the Break function, which is not required anymore
            if (produceOutput == false) { breakFun.on = false; return true; }
            if (outputFileName == string.Empty) { IssueErr(funDesc, "no name for the output file found"); return false; }
            if (indTUName == string.Empty) { IssueErr(funDesc, "no DefTU with Type=IND found (required as DefOutput's TAX_UNIT)"); return false; }

            int iGroup = 0, order = 0;
            breakFun.Name = DefFun.DefOutput; breakFun.pars.Clear();
            breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.DefOutput.File, val = outputFileName, order = (++order).ToString() });
            breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.DefOutput.VarGroup, val = "*", order = (++order).ToString() });
            breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.DefOutput.ILGroup, val = "*", order = (++order).ToString() });
            breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.Common.TAX_UNIT, val = indTUName, order = (++order).ToString() });

            foreach (string tuName in tuNames)
            {
                string sGroup = (++iGroup).ToString();
                breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.DefOutput.UnitInfo_TU, val = tuName, Group = sGroup, order = (++order).ToString() });
                foreach (string uiId in new List<string> { DefPar.Value.UNITINFO_HEADID, DefPar.Value.UNITINFO_ISPARTNER, DefPar.Value.UNITINFO_ISDEPCHILD,
                        DefPar.Value.UNITINFO_ISOWNCHILD, DefPar.Value.UNITINFO_ISOWNDEPCHILD, DefPar.Value.UNITINFO_ISDEPPARENT,
                        DefPar.Value.UNITINFO_ISDEPRELATIVE, DefPar.Value.UNITINFO_ISLONEPARENT })
                    breakFun.pars.Add(Guid.NewGuid().ToString(), new ExeXml.Par() { Name = DefPar.DefOutput.UnitInfo_Id, val = uiId, Group = sGroup, order = (++order).ToString() });
            }

            void IssueErr(Description d, string m, bool isW = false)
                { infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = isW, message = $"{d.Get()}: {m}" }); }

            return true;
        }
    }
}
