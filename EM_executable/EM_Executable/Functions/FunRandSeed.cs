using EM_Common;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EM_Executable
{
    internal class FunRandSeed : FunInSpineBase
    {
        internal FunRandSeed(InfoStore infoStore) : base(infoStore) { }

        private int seed = 1;

        object randLock = new object();     // used for thread-safety

        bool hasRun = false;

        protected override void PrepareNonCommonPar()
        {
            ParNumber parSeed = GetUniquePar<ParNumber>(DefPar.RandSeed.Seed);
            seed = parSeed != null ? (int)parSeed.GetValue() :
                DefinitionAdmin.GetParDefault<int>(DefFun.RandSeed, DefPar.RandSeed.Seed);
        }

        // RandSeed is the only non-TU based function that runs on HH instead of individuals! 
        internal override string GetTUName()
        {
            return HHAdmin.DUMMY_HOUSEHOLD_TU;
        }

        internal override void Run(HH hh, List<Person> tu)
        {
            if (!hasRun)        // if this RandSeed has never run yet, then produce seeds for all HHs
            {
                lock (randLock)
                {
                    if (!hasRun)        // double-check after the lock, just in case...
                    {
                        // if this RandSeed has not run before, then add new seeds for this RandSeed to all HHs
                        infoStore.hhAdmin.SetSeed(description.funID, seed);
                        hasRun = true;
                    }
                }
            }
            hh.ReadSeed(description.funID);  // move to the next seed for this specific HH
        }
    }
}
