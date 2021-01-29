using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunBenCalc : FunInSpineBase
    {
        internal FunBenCalc(InfoStore infoStore) : base(infoStore) { }

        private class Component
        {
            internal ParCond pCond = null;
            internal ParFormula pFormula = null;
            internal bool perElig = true;
            internal ParFormula pLowLim = null, pUpLim = null;
            internal double lowLim = double.MinValue, upLim = double.MaxValue;
            internal bool doRunTimeLimCheck = false;
        }

        private List<Component> components = new List<Component>();
        private ParFormula withdraw_Base;
        private ParFormula withdraw_Rate;
        private ParFormula withdraw_Start;
        private ParFormula withdraw_End;

        protected override void PrepareNonCommonPar()
        {
            foreach (List<ParBase> compParList in GetParGroups(DefPar.BenCalc.Comp_X).Values)
            {
                Component component = new Component();

                // condition
                component.pCond = GetUniqueGroupPar<ParCond>(DefPar.BenCalc.Comp_Cond, compParList);

                // amount (per elig/TU)
                component.pFormula = GetUniqueGroupPar<ParFormula>(DefPar.BenCalc.Comp_perTU, compParList);
                if (component.pFormula != null) component.perElig = false;
                else component.pFormula = GetUniqueGroupPar<ParFormula>(DefPar.BenCalc.Comp_perElig, compParList);
                
                // limits (see remark in _FunInSpineBase_Lim wrt not using a common implementation for limits)
                component.pLowLim = GetUniqueGroupPar<ParFormula>(DefPar.BenCalc.Comp_LowLim, compParList);
                component.pUpLim = GetUniqueGroupPar<ParFormula>(DefPar.BenCalc.Comp_UpLim, compParList);
                if (component.pLowLim != null)
                {
                    if (component.pLowLim.IsGlobal(true)) { component.lowLim = component.pLowLim.GetGlobalValue(); component.pLowLim = null; }
                    else component.doRunTimeLimCheck = true;
                }
                if (component.pUpLim != null)
                {
                    if (component.pUpLim.IsGlobal(true)) { component.upLim = component.pUpLim.GetGlobalValue(); component.pUpLim = null; }
                    else component.doRunTimeLimCheck = true;
                }
                if (!component.doRunTimeLimCheck) // both limits available (might be double.MinValue and/or double.MaxValue, but does no harm here)
                    CheckComponentLimits(ref component.lowLim, ref component.upLim);

                components.Add(component);
            }

            withdraw_Base = GetUniquePar<ParFormula>(DefPar.BenCalc.Withdraw_Base);
            withdraw_Start = GetUniquePar<ParFormula>(DefPar.BenCalc.Withdraw_Start);
            withdraw_End = GetUniquePar<ParFormula>(DefPar.BenCalc.Withdraw_End);
            withdraw_Rate = GetUniquePar<ParFormula>(DefPar.BenCalc.Withdraw_Rate);

            if (withdraw_Base == null && (withdraw_Start != null || withdraw_End != null || withdraw_Rate != null)) // cannot have withdrawal without a base
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: Without a Withdraw_Base all other withdraw parameters will be ignored", runTimeErrorId = description.funID });
            }
            else if (withdraw_End != null && withdraw_Rate != null)  // Withdraw_End and Withdraw_Rate cannot co-exist
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: Withdraw_End and Withdraw_Rate cannot co-exist: Withdraw_Rate will be ignored", runTimeErrorId = description.funID });
            }
            else if (withdraw_Base != null && withdraw_End == null && withdraw_Rate == null)  // Withdraw_End and Withdraw_Rate cannot both be missing
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: Withdraw_End and Withdraw_Rate cannot both be missing: withdrawal will be ignored", runTimeErrorId = description.funID });
            }
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            double val = 0;           
            foreach (Component comp in components)
            {
                double elig = 0;
                foreach (var persElig in comp.pCond.GetTUValues(hh, tu))
                {
                    if (persElig.Value)
                    {
                        if (comp.perElig) ++elig;
                        else { elig = 1; break; } // note: even for perTU just one person (not necessarily the head)
                    }                             // must fulfill the condition (even if it is individual, like age < x)
                }
                if (elig == 0) continue;

                double valComp = comp.pFormula.GetValue(hh, tu);
                double ll = comp.pLowLim != null ? comp.pLowLim.GetValue(hh, tu) : comp.lowLim;
                double ul = comp.pUpLim != null ? comp.pUpLim.GetValue(hh, tu) : comp.upLim;
                if (comp.doRunTimeLimCheck) CheckComponentLimits(ref ll, ref ul, comp.pCond.description.parID, hh);
                valComp = Math.Min(ul, Math.Max(ll, valComp));

                val += elig * valComp;
            }

            if (withdraw_Base != null)  // if there is a withdraw base, calculate withdrawal 
            {
                double withdraw_RateVal = 0;
                double withdraw_BaseVal = withdraw_Base.GetValue(hh, tu);
                double withdraw_StartVal = (withdraw_Start == null) ? 0 : withdraw_Start.GetValue(hh, tu);
                if (withdraw_End != null)   // if there is a withdraw_End, calculate the withdraw_Rate
                {
                    double withdraw_EndVal = withdraw_End.GetValue(hh, tu);
                    withdraw_RateVal = val / (withdraw_EndVal - withdraw_StartVal);
                }
                else if (withdraw_Rate != null) // else get withdraw_End from the parameter (if both missing, rate is left to 0, so nothing is withdrawed)
                {
                    withdraw_RateVal = withdraw_Rate.GetValue(hh, tu);
                }

                val -= Math.Max(withdraw_BaseVal - withdraw_StartVal, 0) * withdraw_RateVal;
                val = Math.Max(val, 0);
            }

            return val;
        }

        // called by ParFormula to replace $Base by the actual content of this parameter
        internal string HandleBase(string formula, Description parDescription)
        {
            List<string> basePlaceHolders = new List<string>() { DefPar.Value.BENCALC_BASE, DefPar.Value.BENCALC_BASE_AMOUNT,
                                                                 DefPar.Value.BENCALC_BASE_IL, DefPar.Value.BENCALC_BASE_VAR };
            string basePH = ContainsBase();
            if (basePH == null) return formula;

            ParFormula parBase = GetUniquePar<ParFormula>(DefPar.BenCalc.Base);
            if (parBase == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{parDescription.Get()}: missing parameter {DefPar.BenCalc.Base}" });
                return formula;
            }

            do
            {
                int index = formula.ToLower().IndexOf(basePH);
                formula = formula.Remove(index, basePH.Length);
                formula = formula.Insert(index, $"({parBase.xmlValue})");
                basePH = ContainsBase();
            } while (basePH != null);

            string ContainsBase()
            {
                foreach (string placeHolder in basePlaceHolders) if (formula.ToLower().Contains(placeHolder)) return placeHolder;
                return null;
            }
            return formula;
        }

        private void CheckComponentLimits(ref double ll, ref double ul, string runTimeErrorId = "",  HH hh = null)
        {
            if (ll <= ul) return;

            string hhID = hh == null ? string.Empty : $" HH {infoStore.GetIDHH(hh)}:";
            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            {
                isWarning = true,
                message = $"{description.Get()}:{hhID} {ll} > {ul}, upper limit overwrites lower limit",
                runTimeErrorId = runTimeErrorId
            });
            ll = ul;
        }
    }
}
