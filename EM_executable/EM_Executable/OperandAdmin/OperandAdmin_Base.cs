using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary> administrates all "operands", i.e. everything that can be used as an operand in formulas or conditions </summary>
    internal partial class OperandAdmin
    {
        // -----------------------------------------------------------------------------------------------------------------------------
        // the main "product" of this class: the index of all operands (as recognised by the parameter-checking procedure)
        private Dictionary<string, Operand> indexOperands = new Dictionary<string, Operand>(StringComparer.OrdinalIgnoreCase);
        // an additional list of non-numeric variables (mainly to allow transfer from input- to output-data)
        internal Dictionary<string, int> indexStringVars = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // -----------------------------------------------------------------------------------------------------------------------------

        // two additional "products":
        // index of loops-ids (defined by Loop-function): used by Store-function, for checking if loop exists
        internal Dictionary<string, FunLoop> indexLoopIDs = new Dictionary<string, FunLoop>(); // value: true: UnitLoop, false: Loop
        // Store-functions (key=postfix/postloop): used by Restore for checking if respective Store exists and overtaking a link
        internal Dictionary<string, FunStore> indexStoreFuns = new Dictionary<string, FunStore>();

        // index of variables as declared in varconfig.xml (key=name, value=is monetary)
        // (this is not a "product" of this class, but provided by the XMLHandler)
        internal Dictionary<string, bool> indexVarConfig = null;

        // -----------------------------------------------------------------------------------------------------------------------------

        private InfoStore infoStore = null;

        internal OperandAdmin(InfoStore _infoStore)
        {
            infoStore = _infoStore;

            // add the "static" elements to the operand-index, i.e.all queries ...
            foreach (string queryName in DefinitionAdmin.GetQueryNamesAndAliases())
                RegisterOperand(name: queryName,
                                description: null, // is used for error-messages only, and this should not fail
                                operand: new Operand(_parType: DefPar.PAR_TYPE.QUERY,
                                                     _creatorFun: string.Empty,
                                                     _isGlobal: false,          // is ignored and actually set in ParQuery.CheckAndPrepare
                                                     _isMonetary: false,        // not relevant for a query (i.e. is a variable-attribute)
                                                     _isWriteable: false,       // ditto
                                                     _addToHHPersonList: false, // ditto
                                                     _readFromFile: false,      // ditto
                                                     _isInitialised: true));    // ditto

            // register fundamental variables, IDs, DAG, DGN, ...
            foreach (string fv in new List<string>() {
                    DefVarName.IDHH, DefVarName.IDPERSON,
                    DefVarName.IDMOTHER, DefVarName.IDFATHER, DefVarName.IDPARTNER,
                    DefVarName.DAG, DefVarName.DGN, DefVarName.DEC, DefVarName.DWT,
                    DefVarName.DMS, DefVarName.LES, DefVarName.LOC,
                    DefVarName.DEFAULT_ELIGVAR })
            {
                bool readFromFile = fv != DefVarName.DEFAULT_ELIGVAR;
                RegisterOperand(name: fv,
                                description: null, // see above
                                operand: new Operand(_parType: DefPar.PAR_TYPE.VAR,
                                                     _creatorFun: string.Empty,
                                                     _isGlobal: false,
                                                     _isMonetary: false,
                                                     _isWriteable: true, // in principle though usually useless
                                                     _addToHHPersonList: true,
                                                     _readFromFile: readFromFile,
                                                     _isInitialised: true));
            }
        }
    }
}
