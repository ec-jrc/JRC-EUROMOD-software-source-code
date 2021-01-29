using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunDefIL : FunOutOfSpineBase
    {
        internal FunDefIL(InfoStore infoStore) : base(infoStore) { }

        // note that at this stage parameters are available (via GetxxxPar-functions of FunBase)
        // but not yet prepared, i.e. only xmlValue is available
        internal override void ProvideIndexInfo()
        {
            ParBase parName = GetUniquePar<ParBase>(DefPar.DefIl.Name); if (parName == null) return;

            Dictionary<string, double> content = new Dictionary<string, double>();
            foreach (var part in GetPlaceholderPar<ParBase>())
            {
                string varILName = part.Key;
                content.Add(varILName, GetAddMod(part.Value)); // note that vars/ILs are 'registered' (i.e.a.o. checked) in ParIL.CheckAndPrepare (i.e. on 1st use)
            }

            ParBool parWarn = GetUniquePar<ParBool>(DefPar.DefIl.Warn_If_NonMonetary);
            bool warnIfNonMon = parWarn == null ? DefinitionAdmin.GetParDefault<bool>(DefFun.DefIl, DefPar.DefIl.Warn_If_NonMonetary) :
                ParBool.IsTrue(parWarn.xmlValue); // note: GetParBoolValueOrDefault cannot be used as parameter WarnIfNonMon is not yet checked

            infoStore.operandAdmin.RegisterIL(parName.xmlValue, DefFun.DefIl, description, content, warnIfNonMon);
        }

        private double GetAddMod(ParBase par)
        {
            string addMode = par.xmlValue.Trim(); // i.e. '+' or '-' or '+0.5' or ...
            if (addMode == "+" || addMode == "-") addMode += "1";
            if (!double.TryParse(EM_Helpers.AdaptDecimalSign(addMode), out double am)) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = false, message = $"{par.description.Get()}: invalid incomelist add-mode: {addMode}" });
            return am;
        }

        protected override void PreprocessParameters()
        {
            if (GetParGroups(DefPar.DefIl.GROUP_REGEXP).Count == 0) return;

            Dictionary<string, double> regExVar = new Dictionary<string, double>();

            foreach (var group in GetParGroups(DefPar.DefIl.GROUP_REGEXP).Values)
            {
                ParBase parDef = GetUniqueGroupPar<ParBase>(DefPar.DefIl.RegExp_Def, group);
                ParBase parFactor = GetUniqueGroupPar<ParBase>(DefPar.DefIl.RegExp_Factor, group);
                if (parDef == null) continue;
                string pattern = parDef.xmlValue;
                double addMode = parFactor == null ? 1.0 : GetAddMod(parFactor);
                foreach (string matchVar in infoStore.operandAdmin.GetMatchingVar(pattern: pattern, regExpr: true))
                    if (!regExVar.ContainsKey(matchVar)) regExVar.Add(matchVar, addMode);
            }

            infoStore.operandAdmin.AddRegExVarToILContent(GetUniquePar<ParBase>(DefPar.DefIl.Name).xmlValue, regExVar);

            // note wrt using PreprocessParameters-function instead of ReplaceVarNameByIndex-function (as FunUprate does):
            // the latter comes after CheckAndPrepare, thus ParIL would do its work first and ignore the additional content of the IL
            // thus variables defined after this DefIL (with DefVar) are not taken into account
            //
            // note on differences to "normal" (i.e. placeholder) incomelist entries:
            // - because of the above described, RegExp-entries are evaluted when the DefL appears in spine and not, as "normal" entries, on first usage of the incomelist
            // - only "registered" variables are taken into account, that means if a variable exists in data and is described in the variables file, but not
            //   used by any function before the DefIL, it is ignored (while a "normal" entry would register such a variable)
            //   uprating would however already count as usage (and one anyway gets a warning, if a data-variable is not uprated)
            //   (if this turns out to be a problem, we can improve later)
            //
            // this differnt behaviour can be justified, because this is a different parameter, I guess ...
        }
    }
}
