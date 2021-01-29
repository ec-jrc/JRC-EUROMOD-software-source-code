using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    /// <summary>
    /// variable-parameters for delivering values, i.e. mostly parts of formulas
    /// (one rare example of usage outside formulas: elig_var)
    /// </summary>
    internal class ParVar : ParBase_FormulaComponent
    {
        internal ParVar(InfoStore infoStore) : base(infoStore)  { }

        internal string name = string.Empty;
        private int index = -1;
        internal DefPar.PREFIX target = DefPar.PREFIX.NONE;

        internal override void CheckAndPrepare(FunBase fun)
        {
            ExtractStandardFootnotes(xmlValue, out name, fun);
            ParsePrefix(name, out name);

            if (!infoStore.operandAdmin.Exists(name))
                infoStore.operandAdmin.CheckRegistration(name, 
                        false, // isOutVar
                        true,  // warnIfNotInit
                        description);

            if (!infoStore.operandAdmin.Exists(name)) return; // registration failed

            isTULevel = infoStore.operandAdmin.GetIsMonetary(name);
            isGlobal = infoStore.operandAdmin.GetIsGlobal(name); // global could be true if e.g. this variable is "consuming" a loop-counter
        }
        // try to parse, issue error if not found! (this should only be called if there is a prefix - see constructor)
        private void ParsePrefix(string name, out string cleanName)
        {
           int comumnIndex =  name.IndexOf(":");
            if (comumnIndex>-1)     // if there is a prefix, use it
            {
                string targetString = name.Substring(0, comumnIndex);
                switch (targetString)
                {
                    case "head":
                        target = DefPar.PREFIX.HEAD; break;
                    case "partner":
                        target = DefPar.PREFIX.PARTNER; break;
                    default:
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $"{description.Get()}: invalid token found '{target}':{name}"});
                        break;
                }
            }
            else    // else, treat target accordingly
            {
                target = DefPar.PREFIX.NONE;
            }
            cleanName = name.Substring(comumnIndex+1);
        }

        internal override void ReplaceVarNameByIndex()
        {
            index = infoStore.operandAdmin.GetIndexInPersonVarList(name);
        }

        // person != null: value is requested for a specific person
        // person == null: monetary: value is requested for tu, non-monetary: value is taken from head
        internal override double GetValue(HH hh, List<Person> tu, Person person = null)
        {
            if (alternativeTU != null) tu = hh.GetAlternativeTU(alternativeTU, tu, description);
            double val;
            if (target == DefPar.PREFIX.NONE)
                val = person == null && isTULevel ? hh.GetTUValue(index, tu)
                                                  : hh.GetPersonValue(index, person == null ? tu[0].indexInHH
                                                                                            : person.indexInHH);
            else val = GetValueTarget(hh, tu, person);
            return ApplyLimits(val, hh, tu, person);
        }


        internal double GetValueTarget(HH hh, List<Person> tu, Person person = null)
        {
            double val;
            if (isTULevel)
            {
                val = hh.GetTUValue(index, tu);
            }
            else
            {
                if (person == null || target == DefPar.PREFIX.HEAD)
                    val = hh.GetPersonValue(index, tu[0].indexInHH);
                else if (target == DefPar.PREFIX.PARTNER)
                    if (tu[0].partnerIndexInHH == byte.MaxValue)    // "no partner found! issue error" This is NOT the case in the old executable... for now, just give the Head value instead
                    {
                        //                        infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        //                        { isWarning = false, message = $"{description.Get()}: no partner found in assessment unit" });
                        val = double.NaN;
                    }
                    else
                    {
                        val = hh.GetPersonValue(index, tu[0].partnerIndexInHH);
                    }
                else
                    val = hh.GetPersonValue(index, person.indexInHH);
            }
            return val;
        }
    }
}
