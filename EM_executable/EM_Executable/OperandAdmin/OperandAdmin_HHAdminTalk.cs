﻿using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class OperandAdmin // *** note that this part contains the communication with HHAdmin during data-reading ***
    {
        // called by HHAdmin, to assess which variables need to be read
        internal List<string> GetVarsToRead()
        {
            return (from op in indexOperands where op.Value.readFromFile select op.Key).ToList();
        }

        // called by HHAdmin, to request if a variable it did not find in data is provided by SetDefault
        internal bool IsProvidedByDefault(string varName, bool useCommonDefault)
        {
            if (useCommonDefault)
            {
                indexOperands[varName].providedByDefault = true; // register as provided by default (for take-up in list of existing vars)
                return true;                                     // see GetPersonVarsNotRead (op.Value.providedByDefault)
            }
            return indexOperands[varName].providedByDefault;
        }

        // called by HHAdmin to get variables which ought to be added to HH.personVarList, but are not read from file
        // (i.e. simulated vars, vars generated by DefVar, by default, loop-counters, ...)
        internal List<string> GetPersonVarsNotRead()
        {
            return (from op in indexOperands
                    where op.Value.addToHHPersonList && (!op.Value.readFromFile || op.Value.providedByDefault)
                    select op.Key).ToList();
        }

        // called by HHAdmin to "inform" about the final variable-index in the HH.personVarList
        internal void SetVarIndex(Dictionary<string, int> varIndex, Dictionary<string, int> stringVarIndex)
        {
            foreach (var vi in varIndex) indexOperands[vi.Key].indexInPersonVarList = vi.Value;
            foreach (var vi in stringVarIndex) indexStringVars[vi.Key] = vi.Value;

            // remove string-variables which do not exist in data from index
            List<string> notFound = (from svi in indexStringVars where svi.Value == -1 select svi.Key).ToList();
            foreach (string nf in notFound) indexStringVars.Remove(nf);
        }
    }
}
