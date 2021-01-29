using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary>
    /// base class for all parameters
    /// it serves directly (i.e. non derived) for text-parameters and placeholder-parameters
    /// </summary>
    internal class ParBase
    {
        protected InfoStore infoStore = null;
        internal ParBase(InfoStore _infoStore) { infoStore = _infoStore; }

        internal Description description = null; // this is mainly used for 'describing' the parameter in e.g. error messages
        internal string xmlValue = string.Empty;

        // a parameter could have (one or all) of these footnote-parameters
        // example: a ParVar with xmlValue=yem#1 could "possess" footnote-parameters #upLim, #lowLim, #_Level (all with group 1)
        private ParFormula pLowLim = null, pUpLim = null; // #_LowLim, #_UpLim
        private string limPri = DefPar.Value.NA;
        private double lowLim = double.MinValue, upLim = double.MaxValue;
        private bool doRunTimeLimCheck = false;
        internal string alternativeTU = null; // #_Level

        internal virtual void CheckAndPrepare(FunBase fun) { }

        // replace variable-names by their index in HH-array (see HHAdmin.indexOfVarInData for purpose)
        internal virtual void ReplaceVarNameByIndex() { }

        /// <summary>
        /// extracts the footnotes of a parameter and returns the respective numbers, also provides the "cleanded" value
        /// example: yem#1#3 (is abstract, but possible, could e.g. share #upLim with one parameter (using group 1) and #_Level with another (using group 3))
        ///          returns {"1","3"}, cleanedValue=yem
        /// </summary>
        protected List<string> GetFootnoteNumbers(string value, out string cleanedValue)
        {
            List<string> numbers = new List<string>(); cleanedValue = value;

            string[] split = cleanedValue.Split('#');
            if (cleanedValue.Length > 0) cleanedValue = split[0];

            for (int i = 1; i < split.Length; ++i) numbers.Add(split[i]);
            return numbers;
        }

        /// <summary>
        /// checks if the standard footnote-parameters, i.e. #_LowLim, #_UpLim, #_Level, are applied on the parameter
        /// if so, stores them in lowLim, upLim and alternativeTU (see above)
        /// also provides the "cleaned" value (e.g. yem#1 -> yem)
        /// note that the footnote-parameters are assessed via the (mother)function (parameter fun)
        /// also note that the only other (not here handled) footnote-parameters are 'amount', which is handled by ParFormula
        /// and query parameters which will not issue a "missing" (see below) but let the query check if it can use the footnote
        /// </summary>
        protected void ExtractStandardFootnotes(string value, out string cleanedValue, FunBase fun)
        {
            List<string> numbers = GetFootnoteNumbers(value, out cleanedValue); // it's unlikely, but yem#1#3 is possible (see GetFootnoteNumbers)
            if (numbers.Count == 0) return; // no footnotes

            // limits (see remark in _FunInSpineBase_Lim wrt not using a common implementation for limits)
            pLowLim = fun.GetFootnotePar<ParFormula>(DefPar.Footnote.LowLim, numbers);
            if (pLowLim != null)
            {
                if (pLowLim.IsGlobal(true)) { lowLim = pLowLim.GetGlobalValue(); pLowLim = null; }
                else doRunTimeLimCheck = true;
            }

            pUpLim = fun.GetFootnotePar<ParFormula>(DefPar.Footnote.UpLim, numbers);
            if (pUpLim != null)
            {
                if (pUpLim.IsGlobal(true)) { upLim = pUpLim.GetGlobalValue(); pUpLim = null; }
                else doRunTimeLimCheck = true;
            }

            ParCateg pLimpriority = fun.GetFootnotePar<ParCateg>(DefPar.Footnote.LimPriority, numbers);
            if (pLimpriority != null) limPri = pLimpriority.GetCateg();

            if (!doRunTimeLimCheck) // both limits available (might be double.MinValue and/or double.MaxValue, but does no harm here)
                CheckLimits(ref lowLim, ref upLim);

            // alternative TU
            ParBase parAltTU = fun.GetFootnotePar<ParBase>(DefPar.Footnote.Level, numbers);
            if (parAltTU != null) alternativeTU = parAltTU.xmlValue;

            // if none of the standard footnotes is defined in the function #x points to nowhere ...
            if (GetType() == typeof(ParQuery)) // ... except for queries, which may have own footnotes
                return; // (this is slightly negligent, as it accepts e.g. IsMarried#1 without respective footnote-parameter)

            if (pLowLim == null && lowLim == double.MinValue &&
                pUpLim == null && upLim == double.MaxValue &&
                parAltTU == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: missing footnote parameter for {value}" });
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
                    // to be precise one would need a runTimeErrorId per formula/condition-operand
                    // but I think one can neglect this unlikely and not harmful impreciseness
                    runTimeErrorId = hh == null ? string.Empty : description.parID
                });
                ll = ul;
            }
            else { if (limPri == DefPar.Value.LIMPRI_LOWER) ul = ll; else ll = ul; }
        }

        protected double ApplyLimits(double value, HH hh, List<Person> tu, Person person)
        {
            double ll = pLowLim != null ? pLowLim.GetValue(hh, tu, person) : lowLim;
            double ul = pUpLim != null ? pUpLim.GetValue(hh, tu, person) : upLim;
            if (doRunTimeLimCheck) CheckLimits(ref ll, ref ul, hh);
            return Math.Min(ul, Math.Max(ll, value));
        }
    }
}
