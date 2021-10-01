using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunSchedCalc  : FunInSpineBase
    {
        internal FunSchedCalc(InfoStore infoStore) : base(infoStore) { }

        private class Band: ICloneable
        {
            internal ParFormula pLowLim = null, pUpLim = null, pRate = null, pAmount = null;
            internal double lowLim = double.MinValue, upLim = double.MaxValue, rate = double.NaN, amount = double.NaN;
            internal int band;

            public object Clone()
            {
                return this.MemberwiseClone();
            }
        }

        private List<Band> bands = new List<Band>();
        internal bool doRunTimeLimCheck = false;
        internal bool doRunTimeRateCheck = false;
        private ParFormula basePar;
        private bool doAverageRates = false;
        private ParFormula quotientPar;
        private double quotient = 1;
        private ParFormula baseThresholdPar;
        private double baseThreshold = double.MinValue;
        private double roundBase = double.NaN;
        private bool simpleProg = false;

        protected override void PrepareNonCommonPar()
        {
            if (GetParGroups(DefPar.SchedCalc.Band_XXX).Count < 1)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: SchedCalc must have at least one band" });
                return;
            }
            foreach (KeyValuePair<int, List<ParBase>> bandParList in GetParGroups(DefPar.SchedCalc.Band_XXX))
            {
                Band band = new Band();
                // Prepare the Band Limits
                band.band = bandParList.Key;
                band.pLowLim = GetUniqueGroupPar<ParFormula>(DefPar.SchedCalc.Band_LowLim, bandParList.Value);
                if (band.pLowLim != null)   // if there is a lowLim
                {
                    if (band.pLowLim.IsGlobal(true)) { band.lowLim = band.pLowLim.GetGlobalValue(); band.pLowLim = null; }
                    else doRunTimeLimCheck = true;
                }
                band.pUpLim = GetUniqueGroupPar<ParFormula>(DefPar.SchedCalc.Band_UpLim, bandParList.Value);
                if (band.pUpLim != null)   // if there is a lowLim
                {
                    if (band.pUpLim.IsGlobal(true)) { band.upLim = band.pUpLim.GetGlobalValue(); band.pUpLim = null; }
                    else doRunTimeLimCheck = true;
                }
                // Prepare the Band Rate/Amount
                band.pRate = GetUniqueGroupPar<ParFormula>(DefPar.SchedCalc.Band_Rate, bandParList.Value);
                band.pAmount = GetUniqueGroupPar<ParFormula>(DefPar.SchedCalc.Band_Amount, bandParList.Value);

                if (band.pRate == null && band.pAmount == null) // Rate and Amount cannot both be missing 
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: Rate and Amount cannot both be missing in group {band.band}" });
                }
                if (band.pRate != null)
                {
                    if (band.pRate.IsGlobal(true)) { band.rate = band.pRate.GetGlobalValue(); band.pRate = null; }
                    else doRunTimeRateCheck = true;
                }
                if (band.pAmount != null)
                {
                    if (band.pAmount.IsGlobal(true)) { band.amount = band.pAmount.GetGlobalValue(); band.pAmount = null; }
                    else doRunTimeRateCheck = true;
                }
                bands.Add(band);
            }

            // check that all required limits exist
            for (int i = 1; i < bands.Count; i++)
            {
                // if both limits are missing then produce an error
                if (bands[i - 1].upLim == double.MaxValue && bands[i - 1].pUpLim == null && bands[i].lowLim == double.MinValue && bands[i].pLowLim == null)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: Band {bands[i - 1].band}: insufficient definition of upper limit. Use parameters 'band_uplim' or 'band_lowlim'(+1)." });
                }
                // if both limits are there, then produce an error
                else if ((bands[i - 1].upLim != double.MaxValue || bands[i - 1].pUpLim != null) && (bands[i].lowLim != double.MinValue || bands[i].pLowLim != null))
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: Double definition of upper limit as 'Band_UpLim {bands[i - 1].band}' and 'Band_LowLim {bands[i].band}'." });
                }
                // else, make sure that the upper limit always exist (copy from Group+1 lower limit if required)
                else if (bands[i - 1].upLim == double.MaxValue && bands[i - 1].pUpLim == null)
                {
                    if (bands[i].pLowLim == null) bands[i - 1].upLim = bands[i].lowLim; // copy the litteral limit if it exists
                    else bands[i - 1].pUpLim = bands[i].pLowLim;                        // else copy the parameter to be run at run-time
                }
            }

            // if possible, check that limits are also in order
            if (!doRunTimeLimCheck) CheckGroupLimits(bands);

            // prepare the other parameters
            basePar = GetUniquePar<ParFormula>(DefPar.SchedCalc.Base);
            if (basePar == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: Missing required parameter Base" });
            }

            ParBool doAverageRatesPar = GetUniquePar<ParBool>(DefPar.SchedCalc.Do_Average_Rates);
            doAverageRates = doAverageRatesPar != null && doAverageRatesPar.GetBoolValue();
            ParBool simpleProgPar = GetUniquePar<ParBool>(DefPar.SchedCalc.Simple_Prog);
            simpleProg = simpleProgPar != null && simpleProgPar.GetBoolValue();

            quotientPar = GetUniquePar<ParFormula>(DefPar.SchedCalc.Quotient);
            if (quotientPar != null)
                if (quotientPar.IsGlobal(true))
                    { quotient = quotientPar.GetGlobalValue(); quotientPar = null; }

            baseThresholdPar = GetUniquePar<ParFormula>(DefPar.SchedCalc.BaseThreshold);
            if (baseThresholdPar != null)
                if (baseThresholdPar.IsGlobal(true))
                { baseThreshold = baseThresholdPar.GetGlobalValue(); baseThresholdPar = null; }

            ParNumber roundBasePar = GetUniquePar<ParNumber>(DefPar.SchedCalc.Round_Base);
            if (roundBasePar != null) roundBase = roundBasePar.GetValue();
        }


        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            // first make a run-time copy of all bands (for thread-safety)
            List<Band> rtBands = new List<Band>(bands.Count);
            bands.ForEach((b) => rtBands.Add(b.Clone() as Band));
            double safeBaseThreshold = baseThreshold;
            double safeQuotient = quotient;

            // do the rouding and check the threshold & quotient
            double baseVal = basePar.GetValue(hh, tu);
            if (!double.IsNaN(roundBase)) baseVal = Math.Round(baseVal / roundBase) * roundBase;
            if (baseThresholdPar != null && !baseThresholdPar.IsGlobal(true)) safeBaseThreshold = baseThresholdPar.GetValue(hh, tu);
            if (quotientPar != null && !quotientPar.IsGlobal(true)) safeQuotient = quotientPar.GetValue(hh, tu);

            if (baseVal < safeBaseThreshold) return 0;  // if base is under the threshold, return 0

            // calculate and check the limits - we only need to calculate the upLim of each Band at runtime, except for Band 1
            if (doRunTimeLimCheck)
            {
                if (rtBands[0].pLowLim != null) rtBands[0].lowLim = rtBands[0].pLowLim.GetValue(hh, tu);
                foreach (Band band in rtBands)
                    if (band.pUpLim != null) band.upLim = band.pUpLim.GetValue(hh, tu);
                CheckGroupLimits(rtBands, description.funID, hh);
            }

            // then check if we need to calculate any rates/amounts
            if (doRunTimeRateCheck)
            {
                foreach (Band band in rtBands)
                {
                    if (band.pRate != null) band.rate = band.pRate.GetValue(hh, tu);
                    if (band.pAmount != null) band.amount = band.pAmount.GetValue(hh, tu);
                }
            }

            baseVal = baseVal / safeQuotient;

            double tax = 0;

            if (simpleProg)     // apply the max rate/amount to all income
            {
                if (rtBands.Count > 0 && baseVal > rtBands.Last().upLim)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    {
                        isWarning = true, runTimeErrorId = description.funID,
                        message = $"{description.Get()}: '{DefPar.SchedCalc.Base}' exceeds '{DefPar.SchedCalc.Band_UpLim}' of highest band ({baseVal}>{rtBands.Last().upLim}) - '{(double.IsNaN(rtBands.Last().rate) ? DefPar.SchedCalc.Band_Amount : DefPar.SchedCalc.Band_Rate)}' of highest band is applied"
                    });

                // get the highest band
                Band hb = null;
                foreach (Band b in rtBands)
                {
                    if (b.lowLim <= baseVal) hb = b;
                    if (b.upLim > baseVal) break;
                }
                if (hb == null) tax = 0;
                else if (!double.IsNaN(hb.rate)) tax = baseVal * hb.rate;
                else tax = hb.amount;
            }
            else    // apply each rate/amount to the equivalent band
            {
                double prevLim = Math.Max(rtBands[0].lowLim, 0);   // start taxing at the lowest limit of the first band
                foreach (Band b in rtBands)
                {
                    if (prevLim > baseVal) break;
                    if (!double.IsNaN(b.rate)) tax += (Math.Max(Math.Min(baseVal, b.upLim) - prevLim, 0)) * b.rate;
                    if (!double.IsNaN(b.amount)) tax += b.amount;
                    prevLim = b.upLim;
                }
            }

            tax = tax * safeQuotient;
            return tax;
        }

        private void CheckGroupLimits(List<Band> rtBands, string runTimeErrorId = "", HH hh = null)
        {
            string hhID = hh == null ? string.Empty : $" HH {infoStore.GetIDHH(hh)}:";

            for (int i = 1; i < rtBands.Count; i++)
            {
                if (rtBands[i-1].upLim > rtBands[i].upLim)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: {hhID} {rtBands[i - 1].upLim} > {rtBands[i].upLim}, upper limit of Band {rtBands[i-1].band} is higher than upper limit of Band {rtBands[i].band}.", runTimeErrorId = description.funID });
                }
            }
        }
    }
}
 