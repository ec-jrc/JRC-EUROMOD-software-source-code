using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class OperandAdmin
    {
        private class Operand
        {
            internal DefPar.PAR_TYPE parType;        // NUMBER/VAR/IL/QUERY, i.e. parameter-type that can be part of formula/condition
            internal string creatorFun;              // DEFVAR/DEFIL/LOOP/STORE/TOTALS/DEFINPUT (e.g. LOOP generates loop-counter)
                                                     // this information has currently no practical relevance, but does no harm to have it
            internal bool isMonetary;
            internal bool isGlobal;                  // true if var is equal for all persons (e.g. creatorFun=DEFVAR with IsGlobal=true
                                                     //                                       or creatorFun=TOTALS, etc.)
            internal bool isWriteable;               // true, if var can be used as output variable
            internal bool providedByDefault = false; // true, if could be a file-read-var, but is actually provided by SetDefault
                                                     // or set to zero, because use-common-default is defined
            internal bool readFromFile;              // true, if (potentialy) read from file (could still be providedByDefault)
            internal bool addToHHPersonList;         // true, if a var in the HH.personVarList (used by HHAdmin to generate the list)
            internal int indexInPersonVarList = -1;  // index of a var in the HH.personVarList, -1 for other operands (querys, ILs, numbers)
            internal bool isInitialised;             // used during checking, to enable the warning "uninitialised use"

            internal Dictionary<string, double>      // only relevant for IL-operands: the IL-definition (yem,1; tin,-1; il_whatever,-0.5,...)
                content = new Dictionary<string, double>();
            internal bool warnIfNonMon = false;      // only relevant for IL-operands: issue warning if e.g. dag is part of an IL

            // note that constructur "asks" for each attribute that does not have a default-value
            internal Operand(DefPar.PAR_TYPE _parType, string _creatorFun, bool _isMonetary, bool _isGlobal,
                             bool _isWriteable, bool _readFromFile, bool _addToHHPersonList, bool _isInitialised)
            {
                parType = _parType;
                creatorFun = _creatorFun;
                isMonetary = _isMonetary;
                isGlobal = _isGlobal;
                isWriteable = _isWriteable;
                readFromFile = _readFromFile;
                addToHHPersonList = _addToHHPersonList;
                isInitialised = _isInitialised;
            }
        }
    }
}
