using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HH
    {
        // -----------------------------------------------------------------------------------------------------------------------------
        // the random numbers administration:
        // each HH will hold a list of seeds with a seed for each RandSeed function called in the spine
        // once the first HH (could be any HH in multi-threading) stumbles upon a RandSeed, a new seed is added to each HH
        // each HH also holds its own Random object that accepts these seeds and produces random numbers for the specific HH

        internal Dictionary<string,int> randSeedList = new Dictionary<string, int>();  // the list of seeds for this HH

        Random random = new Random();                       // the random number generator for this HH

        // this function forces the current HH to move to the next calculated seed 
        internal void ReadSeed(string funID)
        {
            random = new Random(randSeedList[funID]);
        }

        // this function will add a new Seed in the randSeedList
        internal void SetSeed(string funID, int seed)
        {
            randSeedList.Add(funID, seed);
        }

        // this function returns the next random number for this HH
        internal double GetNextRandom(Description callerInfo)
        {
#if OLD_EXE_COMPARISON
            if (GetNextOldExeRand(out double next, callerInfo)) return next;
#endif
            return random.NextDouble();

        }

#if OLD_EXE_COMPARISON
        // These methods are meant to simulate the same random numbers that the old executable produced, by reading pre-compiled lists of random numbers from text files.
        // It was introduced only for a short testing period to validate the results of the EM3 executable against EM2, and only worked for some baselines.
        private List<double> oldExeRandList = new List<double>(), oldExeRandList2 = new List<double>();
        private int oldExeRandIndex = 0, oldExeRandIndex2 = 0;
        internal void TakeOldExeRand(List<double> randList, ref int index)
        {
            switch (infoStore.country.cao.shortName.ToUpper())
            {
                case "DK": case "SK":
                    for (int i = 0; i < GetPersonCount(); ++i) oldExeRandList.Add(randList[index++]); break; // tu_individual_cc
                case "BE": case "EL": case "HU": case "IE": case "PT":
                    oldExeRandList.Add(randList[index++]); break; // tu_household_cc
                case "CY": // tu_individual_cy, comp_cond = {lcs=0} & {yem>0}
                    int iYem = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.YEM);
                    int iLcs = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LCS);
                    for (int i = 0; i < GetPersonCount(); ++i)
                        if (GetPersonValue(iLcs, i) == 0 && GetPersonValue(iYem, i) > 0) oldExeRandList.Add(randList[index++]);
                    break;
                case "UK": // rand is used twice: once in random_uk and once in bta_uk (a switch-pol) 
                    oldExeRandList.Add(randList[index]);
                    oldExeRandList2.Add(randList[index++]); break;
                case "AT":
                    for (int i = 0; i < GetTUs("tu_bcc_at").Count; ++i) oldExeRandList.Add(randList[index++]); break;
                case "FR": break; // too much effort (2 uses with different seeds (3579, 1), sub-TU, and anyway France isn't comparable
            }
        }

        internal bool GetNextOldExeRand(out double next, Description callerInfo)
        {
            next = 0;
            if (oldExeRandIndex >= oldExeRandList.Count) return false;
            switch (infoStore.country.cao.shortName.ToUpper())
            {
                case "UK":
                    if (callerInfo.GetPolName().ToLower() == "random_uk") { next = oldExeRandList[oldExeRandIndex++]; return true; }
                    else if (oldExeRandIndex2 >= oldExeRandList2.Count) return false;
                    next = oldExeRandList2[oldExeRandIndex2++]; return true;
                default: next = oldExeRandList[oldExeRandIndex++]; return true;
            }
        }
#endif
    }
}
