using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class ParIL : ParBase_FormulaComponent
    {
        internal ParIL(InfoStore infoStore) : base(infoStore) { }

        internal string name = string.Empty;

        internal class Entry
        {
            internal bool isIL = false;
            internal string ilName; internal List<Entry> ilEntries;
            internal VarSpec varSpec;
            internal double addFactor; // 1 for +, -1 for -, etc.
        }

        private List<Entry> content = new List<Entry>();

        internal override void CheckAndPrepare(FunBase fun)
        {
            ExtractStandardFootnotes(xmlValue, out name, fun);

            if (infoStore.operandAdmin.Exists(name))
            {   // theoretically it is a bit inefficient to resolve the IL in each ParIL that uses the IL (practically it is irrelevant, as unlikely to causeperformance-issues)
                // the main reason for not doing it in FunDefIL is that this would complicate creating ILs in FunStore

                content = ResolveILContent(name, infoStore.operandAdmin);
                List<Entry> flatContent = FlattenContent(content);

                // all the variables in the incomelist need to be put into the operands-index, if not already there
                // (e.g. ils_xyz contains not yet used variable abc)
                foreach (Entry entry in flatContent)
                {
                    if (!infoStore.operandAdmin.Exists(entry.varSpec.name))
                        infoStore.operandAdmin.CheckRegistration(entry.varSpec.name, false, true, description);
                    // warn if a non-monetary variable is included (if option is not deactivated)
                    if (infoStore.operandAdmin.GetWarnIfNonMon(name) && infoStore.operandAdmin.Exists(entry.varSpec.name) &&
                        !infoStore.operandAdmin.GetIsMonetary(entry.varSpec.name))
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                            message = $"{description.Get()}: incomelist {name} includes non-monetary variable {entry.varSpec.name}" });
                }
            }
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: unknown incomelist {name}" });
            isTULevel = true;
            isGlobal = false;
        }

        internal override void ReplaceVarNameByIndex()
        {
            ReplaceVarNameIndexRecursive(content);
        }

        private List<Entry> FlattenContent(List<Entry> component, double factor = 1)
        {
            List<Entry> entries = new List<Entry>();
            foreach (Entry entry in component)
                if (entry.isIL)
                    entries.AddRange(FlattenContent(entry.ilEntries, entry.addFactor * factor));
                else
                    entries.Add(new Entry()
                    {
                        ilEntries = entry.ilEntries, isIL = entry.isIL, varSpec = entry.varSpec,
                        addFactor = entry.addFactor * factor
                    } );
            return entries;
        }

        private void ReplaceVarNameIndexRecursive(List<Entry> component)
        {
            foreach (Entry entry in component)
                if (entry.isIL)
                    ReplaceVarNameIndexRecursive(entry.ilEntries);
                else
                    entry.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(entry.varSpec.name);
        }

        // person != null: value is requested for a specific person
        // person == null: value is requested for tu
        internal override double GetValue(HH hh, List<Person> tu, Person person = null)
        {
            if (alternativeTU != null) tu = hh.GetAlternativeTU(alternativeTU, tu, description);
            double value = getComponentValue(hh, tu, person, content);
            return ApplyLimits(value, hh, tu, person);
        }

        private double getComponentValue(HH hh, List<Person> tu, Person person, List<Entry> component)
        {
            double value = 0;
            foreach (Entry entry in component)
            {
                double eVal;
                if (entry.isIL)
                    eVal = getComponentValue(hh, tu, person, entry.ilEntries);
                else
                    eVal = person == null ? hh.GetTUValue(entry.varSpec.index, tu)
                                             : hh.GetPersonValue(entry.varSpec.index, person.indexInHH);
                value += entry.addFactor * eVal;
            }
            return value;
        }

        // This returns the formula one must run in the Calculator to get the IL value
        internal static List<Entry> ResolveILContent(string ilName, OperandAdmin operandAdmin)
        {
            List<Entry> newContent = new List<Entry>();
            foreach (var c in operandAdmin.GetILContent(ilName))
            {
                if (operandAdmin.Exists(c.Key) && operandAdmin.GetParType(c.Key) == DefPar.PAR_TYPE.IL)
                    newContent.Add(new Entry() { isIL = true, ilName = c.Key, ilEntries = ResolveILContent(c.Key, operandAdmin), addFactor = c.Value }); // IL: go recursive
                else
                    newContent.Add(new Entry() { isIL = false, varSpec = new VarSpec() { name = c.Key }, addFactor = c.Value }); // variable: just add to list
            }
            return newContent;
        }

        // note: this is "internal static", in order to make it useable by e.g. DefOuput, which needs it for parameter DefIL
        // it returns a list of components
        internal static List<string> GetILComponents(string ilName, OperandAdmin operandAdmin)
        {
            List<string> entries = new List<string>();
            foreach (var c in operandAdmin.GetILContent(ilName))
            {
                if (operandAdmin.Exists(c.Key) && operandAdmin.GetParType(c.Key) == DefPar.PAR_TYPE.IL)
                    entries.AddRange(GetILComponents(c.Key, operandAdmin));
                else entries.Add(c.Key);
            }
            return entries;
        }

        internal class ILparam
        {
            internal string name;
            internal double value;

            public ILparam(string _name, double _value)
            {
                name = _name;
                value = _value;
            }
        }

        // called by Store and ILVarOp
        internal List<Entry> GetFlatContent() { return FlattenContent(content); }
    }
}
