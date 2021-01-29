using EM_Common;
using EM_XmlHandler;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary> each (XML-)function inherits from this class </summary>
    internal partial class FunBase // this part (_FunBase_Main) contains the main functions
    {                              // whereas _FunBase_ParAdmin contains the parameter administration
                                   // and _FunBase_TakeParImpl the actual implementation of TakePar
        protected InfoStore infoStore = null;
        internal Description description = null; // this is mainly used for 'describing' the function in e.g. error messages

        internal FunBase(InfoStore _infoStore) { infoStore = _infoStore; }

        // general common parameters (further common parameters for spine-functions in FunInSpineBase)
        protected ParTU coParTU = null;
        protected ParCond coParRunCond = null;
        protected bool runCondMet_ReadTime = true;

        /// <summary> general parameter registration with the help of the parameter definition in Common-library </summary>
        internal void TakePar(Description _description, Dictionary<string, ExeXml.Par> xmlParList)
        {
            description = _description;

            // get the parameter definition of the function from the Common-library
            DefinitionAdmin.Fun funDef = DefinitionAdmin.GetFunDefinition(description.GetFunName());

            // first, try to identify parameters and, if valid, generate the respective ParXXX and put into respective list
            ClassifyParameters(xmlParList, funDef);

            if (description.GetFunName().ToLower() == DefFun.Scale.ToLower()) (this as FunScale).SetParScaleFactor();

            // then, check for compulsory parameters
            CheckForCompleteness(xmlParList, funDef);
        }

        /// <summary>
        /// this is overloaded only by a couple of the derived classes (FunDefVar, FunDefIL, FunDefTU, FunLoop, ...)
        /// in order to prepare the operand-index, which holds elements (variables, ILs, TUs)
        /// that can be used in parameter-values, i.e. mainly formulas and conditions (see class OperandAdmin)
        /// </summary>
        internal virtual void ProvideIndexInfo() { }

        /// <summary>
        /// this function has three main tasks:
        /// - checking parameters for correctness and completeness (if not already done by the general check in TakePar)
        /// - preparing parameters for run (e.g. formula- and condition-parsing)
        /// - complete the operand-index: e.g. if a parameter is using a variable that is not yet in the operand-index 
        ///   (see ProvideIndexInfo) this must (if correct) be a variable declared in the vardesc-file (see OperandAdmin.CheckRegistration)
        ///   thus, a.o., after checking we will know which variables need to be read from file
        /// </summary>
        internal void CheckAndPrepare()
        {
            PreprocessParameters(); // function specific preparations before the general check

            // first check and prepare footnote parameters, because they are supposed to be 'consumed' by the other parameters
            foreach (var dicPar in footnotePar.Values)
                foreach (ParBase fnPar in dicPar.Values)
                    fnPar.CheckAndPrepare(this);

            // then prepare Run_Cond parameter
            List<ParBase> nonFootnotePar = GetPlainParList();
            PrepareRunCond(nonFootnotePar);
            if (!runCondMet_ReadTime) // do not even prepare other parameters if read-time-Run_Cond is not fulfilled (e.g. function is only applied with another dataset)
                return;               // to avoid that e.g. variables are requested by the reading-process though they are actually not used in the end
            
            // then check and prepare all other parameters
            foreach (ParBase p in nonFootnotePar) p.CheckAndPrepare(this);

            // put parameters into concrete variables to make them easily accessible (for optimising speed)
            // and perform further special checks, if necessary
            // do this for ...
            PrepareCommonPar();    // ... common parameters
            PrepareNonCommonPar(); // ... and the parameters of each derived class

            // check, if all footnotes were 'consumed'
            foreach (var fGroup in footnotePar)
                foreach (var fPar in fGroup.Value)
                    if (!footnoteUsage.ContainsKey(fGroup.Key) || !footnoteUsage[fGroup.Key].Contains(fPar.Key))
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = true, message = $"{fPar.Value.description.Get()}: unused footnote" });
        }

        private void PrepareRunCond(List<ParBase> nonFootnotePar)
        {
            coParRunCond = GetUniquePar<ParCond>(DefPar.Common.Run_Cond);
            if (coParRunCond == null) return;

            coParRunCond.CheckAndPrepare(this);
            if (!coParRunCond.IsGlobal()) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{coParRunCond.description.Get()}: condition must use global variables only" });
            else if (coParRunCond.IsGlobal(true)) // if read-time checking is possible (e.g. for IsUsedDataset, GetSystemYear, ...)
                runCondMet_ReadTime = IsRunCondMet();
            nonFootnotePar.Remove(coParRunCond); // remove, to not check and prepare again with the other parameters
        }

        internal virtual void PrepareCommonPar()
        {
            coParTU = GetUniquePar<ParTU>(DefPar.Common.TAX_UNIT);
        }

        /// <summary>
        /// put parameters into concrete variables to make them easily accessible (for optimising speed)
        /// and perform further special checks (on top of those peforemd in TakePar), if necessary
        /// </summary>
        protected virtual void PrepareNonCommonPar() { }

        /// <summary>
        /// preprocess parameters, set default values, do text replacements etc.
        /// (anything that needs to be performed before the parameter CheckAndPrepare)
        /// </summary>
        protected virtual void PreprocessParameters() { }

        /// <summary>
        /// each parameter that hosts a variable will replace its name by its index in the HH.personVarList (for fast access)
        /// </summary>
        internal virtual void ReplaceVarNameByIndex()
        {
            if (runCondMet_ReadTime) foreach (ParBase p in GetPlainParList(true)) p.ReplaceVarNameByIndex();
        }

        protected bool IsRunCondMet(HH hh = null)
        {
            if (coParRunCond == null) return true;

            return (hh == null) ? coParRunCond.GetGlobalValue() // for non-spine functions return values of first person in data
                                : coParRunCond.GetPersonValue(hh, new Person(0)); // for spine functions return values of first person in HH
        }                                                                         // (allows for using loopcounters in global condition)
    }
}
