using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunInSpineBase : FunBase
    {
        // remark on not using a common implementation for limits:
        // there are three limit-applications:
        // - limiting the result (output-var) of a function (parameters LowLim/UpLim/Limpriority/Threshold) - see below
        // - limiting components of BenCalc (parameters Comp_LowLim/Comp_UpLim)
        // - limiting paramters or formula-operators with footnote-parameters (footnote-parameters #_UpLim/#_LowLim)
        // I have considered implementing a class that handels them uniquely
        // but given that they show quite some differences (having Limpriority/Thresehold or not, footnote-parameter or not, fun/par)
        // and that the application is "spread", i.e. parameters/read-time-call/run-time-call
        // I think it's more transparent (and maybe more performant) to copy the equal parts
        // instead of having a single implementation with x exceptions and parameters

        private ParFormula coParLowLim = null, coParUpLim = null, coParThres = null;
        private double lowLim = double.MinValue, upLim = double.MaxValue, thres = double.MinValue;
        private string limPri = DefPar.Value.NA;
        private bool doRunTimeLimCheck = false, doRunTimeThresCheck = false;

        protected void PrepareLimPar()
        {
            coParLowLim = GetUniquePar<ParFormula>(DefPar.Common.LowLim);
            coParUpLim = GetUniquePar<ParFormula>(DefPar.Common.UpLim);
            coParThres = GetUniquePar<ParFormula>(DefPar.Common.Threshold);
            ParCateg coParLimpriority = GetUniquePar<ParCateg>(DefPar.Common.Limpriority);

            // the following tries to optimise by getting the values here, if they are read-time-available (e.g. 600#y, GetSystemYear, ...)
            // and by checking for implausible values (e.g. upLim = 10, lowLim = 100) if already possible
            if (coParLowLim != null)
            {
                if (coParLowLim.IsGlobal(true)) { lowLim = coParLowLim.GetGlobalValue(); coParLowLim = null; }
                else doRunTimeLimCheck = true;
            }
            if (coParUpLim != null)
            {
                if (coParUpLim.IsGlobal(true)) { upLim = coParUpLim.GetGlobalValue(); coParUpLim = null; }
                else doRunTimeLimCheck = true;
            }
            bool checkThres = coParThres != null;
            if (coParThres != null)
            {
                if (coParThres.IsGlobal(true)) { thres = coParThres.GetGlobalValue(); coParThres = null; }
                else doRunTimeThresCheck = true;
            }
            if (coParLimpriority != null) limPri = coParLimpriority.GetCateg();

            if (!doRunTimeLimCheck) // both limits available (might be double.MinValue and/or double.MaxValue, but does no harm here)
            {
                CheckLimits(ref lowLim, ref upLim);

                if (checkThres && !doRunTimeThresCheck) // threshold and both limits available
                {
                    if (!CheckThreshold(thres, lowLim, upLim)) thres = lowLim;
                }
            }
        }

        protected virtual double ApplyOutVarLimits(double val, HH hh, List<Person> tu)
        {
            val = ApplyLowUpLimits(val, hh, tu, out double ll, out double ul);
            return ApplyThreshold(val, hh, tu, ll, ul);
        }

        private double ApplyLowUpLimits(double val, HH hh, List<Person> tu, out double ll, out double ul)
        {
            ll = coParLowLim != null ? coParLowLim.GetValue(hh, tu) : lowLim;
            ul = coParUpLim != null ? coParUpLim.GetValue(hh, tu) : upLim;
            if (doRunTimeLimCheck) CheckLimits(ref ll, ref ul, hh);
            return Math.Min(ul, Math.Max(ll, val));
        }

        private double ApplyThreshold(double val, HH hh, List<Person> tu, double ll, double ul)
        {
            double th = coParThres != null ? coParThres.GetValue(hh, tu) : thres;
            if (doRunTimeThresCheck && !CheckThreshold(th, ll, ul, hh)) return val;
            if (val >= th) return val;
            return ll == double.MinValue ? 0 : ll; // if below threshold: usually set result to zero, but do not overwrite lower limit
        }

        private void CheckLimits(ref double ll, ref double ul, HH hh = null)
        {
            if (ll <= ul) return;

            if (limPri == DefPar.Value.NA)
            {
                string hhID = hh == null ? string.Empty : $" HH {infoStore.GetIDHH(hh)}:";
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    message = $"{description.Get()}:{hhID} {ll} > {ul}, upper limit overwrites lower limit",
                    runTimeErrorId = hh == null ? string.Empty : description.funID
                });
                ll = ul;
            }
            else { if (limPri == DefPar.Value.LIMPRI_LOWER) ul = ll; else ll = ul; }
        }

        private bool CheckThreshold(double th, double ll, double ul, HH hh = null)
        {
            if (th <= ul && th >= ll) return true;

            string hhID = hh == null ? string.Empty : $" HH {infoStore.GetIDHH(hh)}:";
            if (th > ul) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            {
                isWarning = true,
                message = $"{description.Get()}:{hhID} {th} > {ul}, threshold is higher than upper limit, threshold is ignored",
                runTimeErrorId = hh == null ? string.Empty : description.funID
            });
            if (th < ll) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            {
                isWarning = true,
                message = $"{description.Get()}:{hhID} {th} < {ll}, threshold is lower than lower limit, threshold is ignored",
                runTimeErrorId = hh == null ? string.Empty : description.funID
            });
            return false;
        }
    }
}
