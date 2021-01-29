using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HHAdmin
    {
        // note on why the Get/Set-functions below directly access HH.personVarList, instead of using the Get/Set-functions of HH
        // the latter approach may be more consistent (and would allow to set HH.personVarList private, unfortunately there is no friend in C#)
        // but this is definitely overruled by the higher performance of the former

        /// <summary> set a variable to the same value for all persons (e.g. for initialising a loop-counter) </summary>
        internal void GlobalSetVar(int varIndex, double value)
        {
            foreach (HH hh in hhs)
                for (int personIndex = 0; personIndex < hh.personVarList.Count; ++personIndex)
                    hh.personVarList[personIndex][varIndex] = value;
        }

        /// <summary> get the value of a global variable, i.e. a variable that is equal for all persons (e.g. a loop-counter) </summary>
        internal double GlobalGetVar(int varIndex) { return hhs[0].personVarList[0][varIndex]; }

        /// <summary> multiply all variables read from file of each person by a factor (used for input-currency-conversion) </summary>
        internal void GlobalScaleFileReadVars(double factor) { ScaleVars(infoStore.operandAdmin.GetReadVarIndices(monetaryOnly: true), factor); }

        private void ScaleVars(List<int> varIndices, double factor) // helper-function: see usage above
        {
            foreach (HH hh in hhs)
                for (int personIndex = 0; personIndex < hh.personVarList.Count; ++personIndex)
                    foreach (int varIndex in varIndices)
                        hh.personVarList[personIndex][varIndex] *= factor;
        }

        /// <summary>
        /// This function will accept a seed and calculate new seeds for each HH
        /// </summary>
        internal void SetSeed(string funID, int seed)
        {
            Random random = new Random(seed);
            foreach (HH hh in hhs)
                hh.SetSeed(funID, random.Next());
        }
    }
}
