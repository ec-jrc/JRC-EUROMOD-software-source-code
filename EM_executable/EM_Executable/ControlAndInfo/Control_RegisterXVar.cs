using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Executable
{
    public partial class Control
    {
        private const string PRICE_INITIAL = "p", QUANT_INITIAL = "q", EXP_INITIAL = "x", YSHARE_INITIAL = "xs";
        private const string CREATOR_X = "CREATOR_X";

        private void RegisterXVar(List<string> allDataVars)
        {
            foreach (string varName in allDataVars)
            {
                if (!IsXVar(varName)) continue;
                infoStore.operandAdmin.RegisterVar(name: varName,
                            creatorFun: CREATOR_X,
                            description: null,
                            isMonetary: varName.StartsWith(EXP_INITIAL) && !varName.StartsWith(YSHARE_INITIAL),
                            isGlobal: false,
                            isWriteable: true,
                            setInitialised: true,
                            readFromFile: true);
            }
        }

        private bool IsXVar(string varName)
        {
            if (!varName.StartsWith(PRICE_INITIAL) && !varName.StartsWith(QUANT_INITIAL) && !varName.StartsWith(EXP_INITIAL)) return false;
            int n = varName.StartsWith(YSHARE_INITIAL) ? 2 : 1;
            if (varName.Length <= n) return false;
            foreach (char c in varName.Substring(n)) if (!"0123456789".Contains(c)) return false; // the rest must be only numbers
            return true;
        }
    }
}
