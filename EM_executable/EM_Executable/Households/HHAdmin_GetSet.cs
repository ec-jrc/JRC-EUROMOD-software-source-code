using EM_Common;
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
        /// This function will accept a seed and calculate new seeds for each HH.
        /// By default each HH draws its seed from one shared sequence, i.e. in input order,
        /// so a household's seed depends on its position in (and the composition of) the data.
        /// With seedByHHId the seed is instead a deterministic mix of the RandSeed seed and
        /// the household's idhh: draws become independent of input order, of the presence of
        /// other households, and of subsetting, while staying reproducible for a given seed.
        /// </summary>
        internal void SetSeed(string funID, int seed, bool seedByHHId = false)
        {
            if (seedByHHId)
            {
                int indexIDHH = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDHH);
                foreach (HH hh in hhs)
                    hh.SetSeed(funID, GetHHIdKeyedSeed(seed, hh.GetPersonValue(indexIDHH, 0)));
                return;
            }
            Random random = new Random(seed);
            foreach (HH hh in hhs)
                hh.SetSeed(funID, random.Next());
        }

        /// <summary> mix seed and household id into a per-household seed (SplitMix64-style avalanche) </summary>
        private static int GetHHIdKeyedSeed(int seed, double hhId)
        {
            unchecked
            {
                ulong z = ((ulong)(uint)seed << 32) ^ (ulong)(long)hhId;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                z ^= z >> 31;
                return (int)(z >> 33); // non-negative 31-bit seed for Random
            }
        }
    }
}
