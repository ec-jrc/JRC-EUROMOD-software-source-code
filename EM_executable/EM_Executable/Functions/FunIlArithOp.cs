using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunIlArithOp : FunInSpineBase
    {
        private class OutSpecification { internal VarSpec varSpec; internal ParFormula parFormula; }
        private class ILSpecification { internal string ilName, prefix, postfix, grpNo; }

        // the variables (decomposed base-il) which need to be filled in Run by calculating the respective (adapted) formula
        private List<OutSpecification> outSpecifications = null;

        // the following variables could be temporary as they are used in one go, but to avoid long parameter-lists ...
        private ParFormula parFormula;
        private ILSpecification baseILSpecification;
        private List<ILSpecification> ilSpecifications;
        private List<string> coreVarNames; // the names of the (common) variables in the incomelists, without pre- and postfixes
        private ILSpecification outILSpecification;
        private bool warnIfDuplicateDefinition = true;

        internal FunIlArithOp(InfoStore infoStore) : base(infoStore) { }

        internal static bool HandlesCheckAndPrepare(ParFormula pf)
        { // the standard ParFormula.CheckAndPrepare cannot handle the formula as it contains placeholders (IL_COMPONENT, etc.), therefore skip
          // to later CheckAndPrepare the adapted ParFormulas (see GetAdaptedParFormula below)
            return pf.description.GetFunName().ToLower() == DefFun.IlArithOp.ToLower() &&
                   pf.description.GetParName().ToLower() == DefPar.IlArithOp.Formula.ToLower() &&
                   (pf.xmlValue.Contains(DefPar.IlArithOp.BASE_IL_COMPONENT.ToLower()) ||
                    pf.xmlValue.Contains(DefPar.IlArithOp.OUT_IL_COMPONENT.ToLower()) ||
                    pf.xmlValue.Contains(DefPar.IlArithOp.IL_COMPONENT.ToLower()));
        }

        protected override void PrepareNonCommonPar()
        {
            parFormula = GetUniquePar<ParFormula>(DefPar.IlArithOp.Formula);
            
            baseILSpecification = new ILSpecification() { ilName = GetUniquePar<ParIL>(DefPar.IlArithOp.Base_ILName).xmlValue };
            if (!CheckPrePost(baseILSpecification, GetUniquePar<ParBase>(DefPar.IlArithOp.Base_Prefix),
                                                   GetUniquePar<ParBase>(DefPar.IlArithOp.Base_Postfix))) return;
            
            coreVarNames = GetCoreVars(baseILSpecification); // assess the variables which need to be present in all decomposed ILs

            ilSpecifications = new List<ILSpecification>();
            foreach (var pg in GetParGroups(DefPar.IlArithOp.ILName))
            {
                List<ParBase> ilParList = pg.Value; 
                ILSpecification ilSpecification = new ILSpecification() { ilName = GetUniqueGroupPar<ParIL>(DefPar.IlArithOp.ILName, ilParList).xmlValue, grpNo = pg.Key.ToString() };
                if (!CheckPrePost(ilSpecification, GetUniqueGroupPar<ParBase>(DefPar.IlArithOp.Prefix, ilParList), 
                                                   GetUniqueGroupPar<ParBase>(DefPar.IlArithOp.Postfix, ilParList))) return;
                if (!CheckCoreVars(ilSpecification)) return;
                ilSpecifications.Add(ilSpecification);
            }

            outILSpecification = new ILSpecification() { ilName = GetUniquePar<ParBase>(DefPar.IlArithOp.Out_ILName).xmlValue };
            bool outVarsMustExist = parFormula.xmlValue.Contains(DefPar.IlArithOp.OUT_IL_COMPONENT.ToLower());
            bool outILExists = infoStore.operandAdmin.Exists(outILSpecification.ilName) && infoStore.operandAdmin.GetParType(outILSpecification.ilName) == DefPar.PAR_TYPE.IL;
            if (!CheckPrePost(outILSpecification, GetUniquePar<ParBase>(DefPar.IlArithOp.Out_Prefix),
                                                  GetUniquePar<ParBase>(DefPar.IlArithOp.Out_Postfix), outILExists)) return;

            if (outVarsMustExist && !outILExists)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: using {DefPar.IlArithOp.OUT_IL_COMPONENT} in {DefPar.IlArithOp.Formula} requires incomelist {outILSpecification.ilName} to exist"});
                return;
            }

            // make sure the optional parameter WarnIfNoFactor is parsed before registering operands! 
            warnIfDuplicateDefinition = GetParBoolValueOrDefault(DefFun.IlArithOp, DefPar.IlArithOp.WarnIfDuplicateDefinition);

            if (outILExists) { if (!CheckCoreVars(outILSpecification)) return; }               
            else { if (!RegisterOperands()) return; }

            PrepareFormulas();
        }

        private bool RegisterOperands()
        {
            return CloneILStructure(baseILSpecification.ilName, outILSpecification.ilName);
        }

        private bool CloneILStructure(string ilName, string outIlName)
        {
            Dictionary<string, double> content = new Dictionary<string, double>();

            foreach (ParIL.Entry varIL in ParIL.ResolveILContent(ilName, infoStore.operandAdmin))
            {
                if (!varIL.isIL)
                {
                    string corename = GetCoreVarName(varIL.varSpec.name, baseILSpecification);
                    string outVarName = GetFullVarName(corename, outILSpecification);
                    infoStore.operandAdmin.RegisterVar(
                        name: outVarName,
                        creatorFun: DefFun.Store,
                        description: description,
                        isMonetary: infoStore.operandAdmin.GetIsMonetary(varIL.varSpec.name),
                        isGlobal: infoStore.operandAdmin.GetIsGlobal(varIL.varSpec.name),
                        isWriteable: false, // cannot be used as output variable
                        setInitialised: true,
                        warnForDuplicates: warnIfDuplicateDefinition);
                    if (!infoStore.operandAdmin.Exists(outVarName)) return false; // registration failed
                    content.Add(outVarName, varIL.addFactor);
                }
                else
                {
                    string newILName = GetFullVarName(GetCoreVarName(varIL.ilName, baseILSpecification), outILSpecification);
                    CloneILStructure(varIL.ilName, newILName);
                    content.Add(newILName, varIL.addFactor);
                }
            }
            infoStore.operandAdmin.RegisterIL( // do what usually DefIL does, i.e. register the incomelist
                    name: outIlName,
                    creatorFun: DefFun.IlArithOp,
                    description: description,
                    content: content,
                    warnIfNonMon: false, 
                    warnForDuplicates: warnIfDuplicateDefinition
                    );
            return infoStore.operandAdmin.Exists(outIlName);
        }

        private void PrepareFormulas()
        {
            outSpecifications = new List<OutSpecification>();
            foreach (string coreVarName in coreVarNames)
            {
                string formula = parFormula.xmlValue;
                formula = formula.Replace(DefPar.IlArithOp.BASE_IL_COMPONENT.ToLower(), GetFullVarName(coreVarName, baseILSpecification));
                formula = formula.Replace(DefPar.IlArithOp.OUT_IL_COMPONENT.ToLower(), GetFullVarName(coreVarName, outILSpecification));
                while (formula.Contains(DefPar.IlArithOp.IL_COMPONENT.ToLower())) if (!ReplaceILComponent()) return;

                OutSpecification outSpecification = new OutSpecification()
                {
                    varSpec = new VarSpec() { name = GetFullVarName(coreVarName, outILSpecification) },
                    parFormula = new ParFormula(infoStore) { description = parFormula.description, xmlValue = formula }
                };
                outSpecification.parFormula.CheckAndPrepare(this);
                outSpecifications.Add(outSpecification);

                bool ReplaceILComponent()
                {
                    int ilc = formula.IndexOf(DefPar.IlArithOp.IL_COMPONENT.ToLower());
                    int open = formula.IndexOf("[", ilc + 1); int close = formula.IndexOf("]", open + 2);
                    if (open < 0 || close < 0)
                    { 
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{parFormula.description.Get()}: invalid specification of {DefPar.IlArithOp.IL_COMPONENT}" });
                        return false;
                    }
                    string ilIdentfier = formula.Substring(open + 1, close - open - 1);
                    string toReplace = $"{DefPar.IlArithOp.IL_COMPONENT.ToLower()}[{ilIdentfier}]";
                    ILSpecification ilSpecification = (from ils in ilSpecifications where ils.ilName == ilIdentfier select ils).FirstOrDefault();
                    if (ilSpecification == null) ilSpecification = (from ils in ilSpecifications where ils.grpNo == ilIdentfier select ils).FirstOrDefault();
                    if (ilSpecification == null)
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{parFormula.description.Get()}: {toReplace}: no parameter {DefPar.IlArithOp.ILName}={ilIdentfier} found" });
                        return false;
                    }
                    formula = formula.Replace(toReplace, GetFullVarName(coreVarName, ilSpecification));
                    return true;
                }
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (OutSpecification opVar in outSpecifications)
            {
                opVar.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(opVar.varSpec.name);
                opVar.parFormula.ReplaceVarNameByIndex();
            }
        }

        internal override void Run(HH hh, List<Person> tu) // note: IlArithOp does not have a TAX_UNIT parameter
        {                                                  // therefore tu is DUMMY_INDIVIDUAL_TU, i.e. this is called for each person
            if (!IsRunCondMet(hh)) return;
            foreach (OutSpecification os in outSpecifications) hh.SetTUValue(os.parFormula.GetValue(hh, tu), os.varSpec.index, tu);
        }

        private bool CheckPrePost(ILSpecification ilSpecification, ParBase parPrefix, ParBase parPostfix, bool checkContent = true)
        {
            ilSpecification.prefix = parPrefix?.xmlValue ?? string.Empty;
            ilSpecification.postfix = parPostfix?.xmlValue ?? string.Empty;
            if (string.IsNullOrEmpty(ilSpecification.prefix) && string.IsNullOrEmpty(ilSpecification.postfix))
            {
                return true;    // if no prefix or postfix is defined, then all vars match naming by definition.
            }
            if (!checkContent) return true;

            foreach (string v in ParIL.GetILComponents(ilSpecification.ilName, infoStore.operandAdmin))
            {
                if (v.StartsWith(ilSpecification.prefix, StringComparison.OrdinalIgnoreCase) &&
                    v.EndsWith(ilSpecification.postfix, StringComparison.OrdinalIgnoreCase)) continue;
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: {ilSpecification.ilName}: contained variables must be named {GetFullVarName("*", ilSpecification)}; " +
                              $"variable {v} does not follow this rule"});
                return false;
            }
            return true;
        }

        private string GetFullVarName(string coreVarName, ILSpecification ilSpecification)
        {
            return $"{ilSpecification.prefix}{coreVarName}{ilSpecification.postfix}";
        }

        private string GetCoreVarName(string fullVarName, ILSpecification ilSpecification)
        { 
            return fullVarName.Substring(ilSpecification.prefix.Length, fullVarName.Length - ilSpecification.prefix.Length - ilSpecification.postfix.Length);
        }

        private List<string> GetCoreVars(ILSpecification ilSpecification)
        {
            List<string> cv = new List<string>();
            foreach (string v in ParIL.GetILComponents(ilSpecification.ilName, infoStore.operandAdmin))
                cv.Add(GetCoreVarName(v, ilSpecification));
            return cv;
        }

        private bool CheckCoreVars(ILSpecification ilSpecification)
        {
            List<string> ilCoreVars = GetCoreVars(ilSpecification);
            foreach (string cv in coreVarNames)
            {
                if (ilCoreVars.Contains(cv, true)) continue;
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: {ilSpecification.ilName} does not contain variable {GetFullVarName(cv, ilSpecification)}" });
                return false;
            }
            return true;
        }
    }
}
