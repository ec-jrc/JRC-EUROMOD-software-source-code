using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunUnitLoop : FunLoop
    {
        internal FunUnitLoop(InfoStore infoStore) : base(infoStore) { }

        private string eligUnit = string.Empty;
        private ParCond parEligUnitCond = null;
        private string who = DefPar.Value.WHO_ALL;
        private int varIndexNElig = -1, varIndexIsElig = -1, varIndexIsEligInIter = -1, varIndexIsCurElig = -1;

        protected override void PrepareNonCommonPar()
        {
            PrepareStartEndPar();  // same as base
            PrepareIterationPar(); // own implementation for UnitLoop
        }

        private void PrepareIterationPar()
        {
            ParTU parEligUnit = GetUniquePar<ParTU>(DefPar.UnitLoop.Elig_Unit); if (parEligUnit != null) eligUnit = parEligUnit.name;
            parEligUnitCond = GetUniquePar<ParCond>(DefPar.UnitLoop.Elig_Unit_Cond);
            ParCateg parWho = GetUniquePar<ParCateg>(DefPar.UnitLoop.Elig_Unit_Cond_Who); if (parWho != null) who = parWho.GetCateg();
        }

        internal override void ProvideIndexInfo()
        {
            base.ProvideIndexInfo();

            foreach (string ulVar in new List<string>() { DefVarName.UNITLOOP_N_ELIG,
                                                          DefVarName.UNITLOOP_IS_ELIG,
                                                          DefVarName.UNITLOOP_IS_ELIG_IN_ITER,
                                                          DefVarName.UNITLOOP_IS_CUR_ELIG })
                infoStore.operandAdmin.RegisterVar(name: ulVar + loopID,
                                                   creatorFun: DefFun.UnitLoop,
                                                   description: description,
                                                   isMonetary: false,
                                                   isGlobal: false,
                                                   isWriteable: false,
                                                   setInitialised: true);
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            if (loopID == null) return;
            varIndexNElig = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.UNITLOOP_N_ELIG + loopID);
            varIndexIsElig = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.UNITLOOP_IS_ELIG + loopID);
            varIndexIsEligInIter = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.UNITLOOP_IS_ELIG_IN_ITER + loopID);
            varIndexIsCurElig = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.UNITLOOP_IS_CUR_ELIG + loopID);
        }

        private void InitVar(HH hh, out int nElig)
        {
            nElig = 0;
            foreach (List<Person> tu in hh.GetTUs(eligUnit))
            {
                bool isMet = parEligUnitCond == null ? true : FunInSpineBase.IsCondMetByTU(hh, tu, parEligUnitCond, who);
                if (isMet) ++nElig;
                hh.SetTUValue(isMet ? 1 : 0, varIndexIsElig, tu); // on TU-level: set isULElig_loopname
                hh.SetTUValue(isMet ? nElig : 0, varIndexIsEligInIter, tu); // on TU-level: set isEligInIter_loopname
            }
            for (int p = 0; p < hh.GetPersonCount(); ++p) hh.SetPersonValue(nElig, varIndexNElig, p); // on HH-level: set nULElig_loopname
        }

        private void SetCurElig(HH hh, double curIter)
        {
            foreach (List<Person> tu in hh.GetTUs(eligUnit)) // set the variable for each TU in the HH
            {
                double eligInIter = hh.GetTUValue(varIndexIsEligInIter, tu); // assess in which iteration the TU is "elig"
                hh.SetTUValue(eligInIter == curIter ? 1 : 0, varIndexIsCurElig, tu);
            }
        }

        internal override bool DoBreak(HH hh = null)
        {
            double iterationsDone = GetCounter(hh);
            if (IsFinished(hh, iterationsDone, true)) return true;
            SetCounter(hh, iterationsDone + 1);
            return false;
        }

        internal override bool DoJumpOver(bool init, HH hh = null)
        {
            // --- 3 variables must be set before the loop is running (i.e. before first iteration) ---
            // nULElig_loopname: how many "eligible" units are there in the HH
            // isULElig_loopname: is a unit eligible (defined by Elig_Unit_Cond or just yes, if there is no such condition)
            // isEligInIter: which iteration is the unit's turn
            // --- also find out how often the loop must run ---
            // in a sequential run: the unit-loop runs as often as there are elig units in the HH with the most elig units
            // in a parallel run the unit-loop actually runs as often as necessary for the very HH (see DoBreak)
            if (init)
            {
                if (hh != null) InitVar(hh, out int dummy);
                else
                {
                    nIterations = 0;
                    foreach (HH shh in infoStore.hhAdmin.hhs) { InitVar(shh, out int nElig); nIterations = Math.Max(nIterations, nElig); }
                }

                SetCounter(hh, 1); // initialise loop-counter
                if (IsFinished(hh, 0, false)) return true;
            }

            // in each iteration: set the variable which indicates whether a unit is "eligible" in the current iteration
            double curIter = GetCounter(hh);
            if (hh != null) SetCurElig(hh, curIter);
            else foreach (HH shh in infoStore.hhAdmin.hhs) SetCurElig(shh, curIter);
            return false;
        }

        private bool IsFinished(HH hh, double iterationsDone, bool checkRunCond)
        {
            if (checkRunCond && !IsRunCondMet(hh)) return true; // mimics the old exe, which does a do-while, i.e. check RunCond at end only
                                                                // more precisely the old exe offers a parameter Run_Once_If_No_Elig
                                                                // which is not implemented here, because never used
            double iterationsRequired = hh == null
                ? nIterations // sequential run: run as often as there are max elig units (see initialisation of nIterations in DoStartWork)
                : GetIterationsRequired(hh); // parallel run: run as often as necessary for the very HH
            return iterationsDone >= iterationsRequired;
        }

        internal bool IsFinished(HH hh) // called by sequential run
        {
            double counter = GetCounter(hh);
            return counter > GetIterationsRequired(hh) || (counter > 1 && !IsRunCondMet(hh));
        }

        private double GetIterationsRequired(HH hh) // if we decide to implement the parameter Run_Once_If_No_Elig
        {                                           // one would ask whether this is false and then not do the Max
            return Math.Max(1, hh.GetPersonValue(varIndexNElig, 0));
        }

        internal string GetEligUnit() { return eligUnit; } // called by Store
    }
}
