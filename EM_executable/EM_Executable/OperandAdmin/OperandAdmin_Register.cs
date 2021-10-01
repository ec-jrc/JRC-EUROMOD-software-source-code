using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class OperandAdmin
    {
        /// <summary>
        /// basically adds a variable defined by DefVar-function to the operand-index
        /// but it is also used to add variables produced by functions Loop, Store, Totals and DefInput
        /// </summary>
        internal void RegisterVar(string name, string creatorFun, Description description,
                                  bool isMonetary, bool isGlobal, bool isWriteable, bool setInitialised, bool readFromFile = false, bool warnForDuplicates = true)
        {
            bool addToHHPersonList = true;
            RegisterOperand(name, description,
                new Operand(DefPar.PAR_TYPE.VAR, creatorFun, isMonetary, isGlobal, isWriteable,
                            readFromFile, addToHHPersonList, setInitialised), warnForDuplicates);
        }

        /// <summary> adds an incomelist (defined by DefIL- or Store-function) to the operand-index </summary>
        internal void RegisterIL(string name, string creatorFun, Description description,
                                 Dictionary<string, double> content, bool warnIfNonMon, bool warnForDuplicates = true)
        {
            RegisterOperand(name, description,
                new Operand(_parType: DefPar.PAR_TYPE.IL,
                            _creatorFun: creatorFun, // either DefIL or Store
                            _isMonetary: true,
                            _isGlobal: false,
                            _isWriteable: false,
                            _readFromFile: false,
                            _addToHHPersonList: false,
                            _isInitialised: true)
                { content = content, warnIfNonMon = warnIfNonMon }, warnForDuplicates);
        }

        internal void AddRegExVarToILContent(string ilName, Dictionary<string, double> regExVar) { Get(ilName, true).content.AddRange(regExVar); }

        /// <summary>
        /// checks whether operand (used by a parameter) already is in the operand-index and, if not, adds it
        /// the latter means that it is not an IL nor a query nor 'amount' nor defined by DefVar
        /// (because ILs, queries and 'amount's are added before analysing parameters and a DefVar earlier in the spine)
        /// consequently it must be defined in the variables file (or by Store-function, see below)
        /// </summary>
        /// <param name="isOutVar"> if the operand is used by an output-parameter (output_(add_)var, result_var) it can be set initialised </param>
        /// <param name="warnIfNotInit"> if the operand is 'used' it needs to checked if it is initialised </param>
        internal void CheckRegistration(string name, bool isOutVar, bool warnIfNotInit, Description description)
        {
            // this boolean makes the not working warning for not initialised Output_Add_Var work, with minimal risk of introducing unwanted side-effects
            bool isNotInitialisedOutputAddVar = isOutVar && warnIfNotInit && !indexOperands.ContainsKey(name);

            if (!indexOperands.ContainsKey(name)) // check if already registered ...
            {
                if (indexVarConfig.ContainsKey(name)) // ... if not, is variable defined in variables file?
                {
                    RegisterVar(name: name,
                                creatorFun: string.Empty, // we may want to specify this later, if it is used anywhere
                                description: description,
                                isMonetary: indexVarConfig[name],
                                isGlobal: false,
                                isWriteable: true,
                                setInitialised: !IsSimulated(name) | isOutVar,
                                readFromFile: !IsSimulated(name));
                }
                else
                {
                    // last possibility: it is sth like yem_loop, i.e. a Store-operand which is used e.g. in the nth iteration of a loop
                    if (!infoStore.IsProspectiveStoreOperand(name)) // actual registration takes place once Store is in turn (in spine-order)
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{description.Get()}: unknown variable {name}" });
                    return; // exit function, i.e. no further checking if registration fails
                }
            }

            //Final check in case the variable has been included in indexOperands
            isNotInitialisedOutputAddVar = isOutVar && warnIfNotInit && !indexOperands.ContainsKey(name);

            // finally check correct use
            if ((warnIfNotInit && !indexOperands[name].isInitialised) || isNotInitialisedOutputAddVar) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = true, message = $"{description.Get()}: use of not initialised variable {name}" });
            if (isOutVar && !indexOperands[name].isWriteable) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = false, message = $"{description.Get()}: {name} is not a valid output variable" });
        }

        private void RegisterOperand(string name, Description description, Operand operand, bool warnForDuplicates = true)
        {
            if (!indexOperands.ContainsKey(name)) indexOperands.Add(name, operand);
            else if (warnForDuplicates) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = true, message = $"{description.Get()}: double definition of {name}" });
        }

        internal static bool IsSimulated(string varName) { return varName.EndsWith(DefGeneral.POSTFIX_SIMULATED); }
    }
}
