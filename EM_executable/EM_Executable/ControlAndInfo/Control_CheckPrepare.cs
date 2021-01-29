using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;

namespace EM_Executable
{
    public partial class Control
    {
        private void CheckAndPrepare()
        {
            Dictionary<double, ExeXml.Fun> allFun = new Dictionary<double, ExeXml.Fun>(); // key: order - necessary to do TakePar in spine-order, see below

            // 1st STEP: IDENTIFY FUNCTIONS
            foreach (var pol in infoStore.country.cao.pols) // policies are only function-containers, not relevant for the run
            {                                               // (except from perhaps error-reporting)
                foreach (var f in pol.Value.funs)
                {
                    ExeXml.Fun xmlFun = f.Value; string funID = f.Key;
                    Description funDescription = new Description(pol.Value, xmlFun, funID); string funName = xmlFun.Name.ToLower();
                    double order = GetOrder(pol.Value.order, xmlFun.order); bool recognised = true;

                    if (funName == DefFun.ArithOp.ToLower()) infoStore.spine.Add(order, new FunArithOp(infoStore));
                    else if (funName == DefFun.Elig.ToLower()) infoStore.spine.Add(order, new FunElig(infoStore));
                    else if (funName == DefFun.BenCalc.ToLower()) infoStore.spine.Add(order, new FunBenCalc(infoStore));
                    else if (funName == DefFun.SchedCalc.ToLower()) infoStore.spine.Add(order, new FunSchedCalc(infoStore));
                    else if (funName == DefFun.Min.ToLower()) infoStore.spine.Add(order, new FunMin(infoStore));
                    else if (funName == DefFun.Max.ToLower()) infoStore.spine.Add(order, new FunMax(infoStore));
                    else if (funName == DefFun.Allocate.ToLower()) infoStore.spine.Add(order, new FunAllocate(infoStore));
                    else if (funName == DefFun.DefVar.ToLower()) infoStore.spine.Add(order, new FunDefVar(infoStore, false));
                    else if (funName == DefFun.DefConst.ToLower()) infoStore.spine.Add(order, new FunDefVar(infoStore, true));
                    else if (funName == DefFun.DefIl.ToLower()) infoStore.spine.Add(order, new FunDefIL(infoStore));
                    else if (funName == DefFun.Store.ToLower()) infoStore.spine.Add(order, new FunStore(infoStore));
                    else if (funName == DefFun.Restore.ToLower()) infoStore.spine.Add(order, new FunRestore(infoStore));
                    else if (funName == DefFun.IlVarOp.ToLower()) infoStore.spine.Add(order, new FunILVarOp(infoStore));
                    else if (funName == DefFun.RandSeed.ToLower()) infoStore.spine.Add(order, new FunRandSeed(infoStore));
                    else if (funName == DefFun.CallProgramme.ToLower()) infoStore.spine.Add(order, new FunCallProgramme(infoStore));
                    else if (funName == DefFun.DefInput.ToLower()) infoStore.spine.Add(order, new FunDefInput(infoStore));
                    else if (funName == DefFun.DefTu.ToLower()) infoStore.spine.Add(order, new FunDefTU(infoStore));
                    else if (funName == DefFun.UpdateTu.ToLower()) infoStore.spine.Add(order, new FunUpdateTU(infoStore));
                    else if (funName == DefFun.Uprate.ToLower()) infoStore.spine.Add(order, new FunUprate(infoStore));
                    else if (funName == DefFun.SetDefault.ToLower()) infoStore.spine.Add(order, new FunSetDefault(infoStore));
                    else if (funName == DefFun.DefOutput.ToLower()) infoStore.spine.Add(order, new FunDefOutput(infoStore));
                    else if (funName == DefFun.Loop.ToLower()) infoStore.spine.Add(order, new FunLoop(infoStore));
                    else if (funName == DefFun.UnitLoop.ToLower()) infoStore.spine.Add(order, new FunUnitLoop(infoStore));
                    else if (funName == DefFun.Totals.ToLower()) infoStore.spine.Add(order, new FunTotals(infoStore));
                    else if (funName == DefFun.Scale.ToLower()) infoStore.spine.Add(order, new FunScale(infoStore));
                    else if (funName == DefFun.DropUnit.ToLower()) infoStore.spine.Add(order, new FunDropKeepUnit(infoStore, false));
                    else if (funName == DefFun.KeepUnit.ToLower()) infoStore.spine.Add(order, new FunDropKeepUnit(infoStore, true));
                    else if (funName == DefFun.AddHHMembers.ToLower()) infoStore.spine.Add(order, new FunAddHHMembers(infoStore));
                    else if (funName == DefFun.IlArithOp.ToLower()) infoStore.spine.Add(order, new FunIlArithOp(infoStore));
                    else
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = true, message = $"Unkown function is ignored: {funDescription.Get()}" });
                        recognised = false;
                    }

                    // identification of parameters is moved below, to be done in spine-order (this is at the stage of moving only necessary for FunScale)
                    //if (recognised) infoStore.spine[order].TakePar(funDescription, xmlFun.pars);
                    if (recognised) { infoStore.spine[order].description = funDescription; allFun.Add(order, f.Value); }
                }
            }

            // 2nd STEP: IDENTIFY (KNOWN) PARAMETERS AND CHECK FOR COMPLETENESS
            foreach (var f in infoStore.spine) f.Value.TakePar(f.Value.description, allFun[f.Key].pars);

            // 3rd STEP: PUT TUs, VARIABLES, QUERIES, LOOPS, etc. (i.e. stuff that's used by other parameters) INTO RESPECTIVE INDICES
            foreach (FunBase fun in infoStore.spine.Values) fun.ProvideIndexInfo();

            // 4th STEP: ANALYSE PARAMETER-VALUES
            foreach (FunBase fun in infoStore.spine.Values)
                if (fun.description.GetFunName().ToLower() != DefFun.DefTu.ToLower()) // for DefTU this function is called by ParTU, i.e.
                    fun.CheckAndPrepare();                                            // on first usage (see ParTU for futher explanation)
        }

        private double GetOrder(double polOrder, double funOrder)
        {
            return polOrder * 10000.0 + funOrder;
        }
    }
}
