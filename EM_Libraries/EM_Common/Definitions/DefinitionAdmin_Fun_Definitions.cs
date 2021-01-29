using System;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        private static void DefineFun()
        {
            funDefs = new Dictionary<string, Fun>(StringComparer.OrdinalIgnoreCase);

            Fun fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "a simple calculator, allowing for the most common arithmetical operations." };
            DefPar.ArithOp.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.ArithOp, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "is most frequently used for determining the eligibility for receiving benefits. However, it also allows for determining the liability for paying taxes, as well as evaluating other conditions." };
            DefPar.Elig.Add(fun); DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun);
            DefPar.Common.Add(fun: fun, addLimits: false, addRound: false,
                outvarMode: DefPar.Common.OUTVAR_ADD_MODE.OPTIONAL_NOADD, outvarDefault: DefVarName.DEFAULT_ELIGVAR);
            funDefs.Add(DefFun.Elig, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "allows for modelling a wide range of policy instruments, in particular benefits.\nThe result is calculated as a sum of 'components', where the value of a component is only added if a certain condition is fulfilled by at least one member of the assessment unit.\nThe following stylised formulas illustrates the approach:\nresult = Sum (Comp_perTU if Comp_Cond = true) \nresult = Sum (Comp_perElig * nElig if Comp_Cond = true)" };
            DefPar.BenCalc.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.BenCalc, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "allows for the implementation of the most common (tax) schedules." };
            DefPar.SchedCalc.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.SchedCalc, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "a simple minimum calculator." };
            DefPar.Min.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.Min, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "a simple maximum calculator." };
            DefPar.Max.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.Max, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the definition of intermediate variables." };
            DefPar.DefVar.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.DefVar, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the definition of constants." };
            DefPar.DefConst.Add(fun); DefPar.Footnote.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.DefConst, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SYSTEM, description = "allows for the definition of incomelists." };
            DefPar.DefIl.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.DefIl, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SYSTEM, description = "allows for the definition of assessment units.\nNote that parameters may use variables with the prefixes 'head:' or 'partner:'. These prefixes can be used with variables only, not with incomelists or queries.\nAlso note that '{Default}' can be used to further define any default condition (as indicated in Default Value).\n" };
            DefPar.DefTu.Add(fun); DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun);
            funDefs.Add(DefFun.DefTu, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the re-assessment of assessment units." };
            DefPar.UpdateTu.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.UpdateTu, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the uprating of monetary dataset variables." };
            DefPar.Uprate.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.Uprate, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the definition of model output." };
            DefPar.DefOutput.Add(fun);
            DefPar.Common.Add(fun: fun, outvarMode: DefPar.Common.OUTVAR_ADD_MODE.NOT, addResultVar: false, addLimits: false, addRound: false);
            DefQuery.AddDBNamePar(fun); // do not add all footnote parameters, but only the one that can be used in RunCond
            funDefs.Add(DefFun.DefOutput, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SPECIAL, description = "allows for repeating a part (or all) of the tax-benefit calculations." };
            DefPar.Loop.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.Loop, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SPECIAL, description = "allows for repeating part (or all) of the tax-benefit calculation." };
            DefPar.UnitLoop.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun); DefPar.Footnote.Add(fun);
            funDefs.Add(DefFun.UnitLoop, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "provides a 'save as' functionality for variables, incomelists and components of incomelists." };
            DefPar.Store.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.Store, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "sets variables back to some previous value (stored by function Store)." };
            DefPar.Restore.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.Restore, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for operations on the content (i.e. the variables) of an incomelist." };
            DefPar.IlVarOp.Add(fun); DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun);
            DefPar.Common.Add(fun: fun, tuMode: DefPar.Common.TU_ADD_MODE.NOT, outvarMode: DefPar.Common.OUTVAR_ADD_MODE.NOT, addResultVar: false,
                              addLimits: false, addRound: false); // that is: only Who_Must_Be_Elig, Elig_Var and Run_Cond
            funDefs.Add(DefFun.IlVarOp, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for the calculation of aggregates of variables or incomelists over the whole population or a selected subgroup." };
            DefPar.Totals.Add(fun); DefQuery.AddDBNamePar(fun);
            DefPar.Common.Add(fun: fun, outvarMode: DefPar.Common.OUTVAR_ADD_MODE.NOT, addResultVar: false, addElig: false,
                              addLimits: false, addRound: false); // that is: only TAX_UNIT and Run_Cond
            funDefs.Add(DefFun.Totals, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.POLICY, description = "allows for (re)allocating amounts (incomes, benefits, taxes) between members of assessment units." };
            DefPar.Allocate.Add(fun); DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun);
            DefPar.Common.Add(fun: fun, addLimits: false, addRound: false);
            funDefs.Add(DefFun.Allocate, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "sets the starting point for generating a series of pseudorandom numbers." }; 
            DefPar.RandSeed.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.RandSeed, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SYSTEM, description = "allows for the setting of default values for not existent dataset variables." };
            DefPar.SetDefault.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.SetDefault, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for calling an external application.\nNote that the function is only available under Windows as it uses platform specific code." };
            DefPar.CallProgramme.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.CallProgramme, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for reading values for one or more EUROMOD variables from a text file." };
            DefPar.DefInput.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.DefInput, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for scaling monetary variables and monetary parameters." };
            DefPar.Scale.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.Scale, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for dropping individuals, families or households with special characteristics from the calculations." };
            DefPar.DropUnit.Add(fun); DefQuery.AddAllPar(fun); DefPar.Footnote.Add(fun);
            DefPar.Common.Add(fun: fun, outvarMode: DefPar.Common.OUTVAR_ADD_MODE.NOT, addResultVar: false, addElig: false,
                              addLimits: false, addRound: false); // that is: only TAX_UNIT and Run_Cond
            funDefs.Add(DefFun.DropUnit, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for keeping only individuals, families or households with special characteristics within the calculations." };
            DefPar.KeepUnit.Add(fun); DefQuery.AddAllPar(fun); DefPar.Footnote.Add(fun);
            DefPar.Common.Add(fun: fun, outvarMode: DefPar.Common.OUTVAR_ADD_MODE.NOT, addResultVar: false, addElig: false,
                              addLimits: false, addRound: false); // that is: only TAX_UNIT and Run_Cond
            funDefs.Add(DefFun.KeepUnit, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.OUTSIDE_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for adding persons to households." };
            DefPar.AddHHMembers.Add(fun); DefPar.Common.AddRunCond(fun);
            DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun); // for the formulas and conditions
            funDefs.Add(DefFun.AddHHMembers, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SPECIAL, description = "allows the user to break the run at any point inside the spine." };
            DefPar.Break.Add(fun);
            funDefs.Add(DefFun.Break, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.IN_SPINE, kind = Fun.KIND.SPECIAL, description = "allows for an arithmetic operation on the variables of one or more incomelists." };
            DefPar.IlArithOp.Add(fun); DefPar.Footnote.Add(fun); DefQuery.AddAllPar(fun); DefPar.Common.AddRunCond(fun);
            funDefs.Add(DefFun.IlArithOp, fun);

            // the following functions are "special", in the sense that they are not requested by Control.CheckAndPrepare
            // they are however necessary for the UI and possible future uses

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.ADDON, description = "add-on function: provides a short description of the add-on and specifies for which systems the add-on is applicable." };
            DefPar.AddOn_Applic.Add(fun);
            funDefs.Add(DefFun.AddOn_Applic, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.ADDON, description = "add-on function: allows for adding a policy." };
            DefPar.AddOn_Pol.Add(fun);
            funDefs.Add(DefFun.AddOn_Pol, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.ADDON, description = "add-on function: allows for adding a function." };
            DefPar.AddOn_Func.Add(fun);
            funDefs.Add(DefFun.AddOn_Func, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.ADDON, description = "add-on function: allows for adding parameters." };
            DefPar.AddOn_Par.Add(fun);
            funDefs.Add(DefFun.AddOn_Par, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.ADDON, description = "allows add-ons to switch extensions on or off, these settings have priority over default-switches and switches set via the user interface." };
            DefPar.AddOn_ExtensionSwitch.Add(fun);
            funDefs.Add(DefFun.AddOn_ExtensionSwitch, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SPECIAL, description = "allows for changing the values of parameters of other functions." };
            DefPar.ChangeParam.Add(fun);
            funDefs.Add(DefFun.ChangeParam, fun);

            fun = new Fun() { runMode = DefFun.RUN_MODE.NOT_APPLICABLE, kind = Fun.KIND.SPECIAL };
            DefPar.ChangeSwitch.Add(fun); DefPar.Common.AddRunCond(fun); DefQuery.AddDBNamePar(fun);
            funDefs.Add(DefFun.ChangeSwitch, fun);

            foreach (var f in funDefs)
            {
                f.Value.description = f.Value.description == string.Empty ? GetFunDescription(f.Key) : f.Value.description;
                foreach (var p in f.Value.par)
                    p.Value.description = p.Value.description == string.Empty ? GetParDescription(f.Key, p.Key) : p.Value.description;
                foreach (var pg in f.Value.parGroups) foreach (var p in pg.par)
                    p.Value.description = p.Value.description == string.Empty ? GetParDescription(f.Key, p.Key) : p.Value.description;
            }
        }
    }
}
