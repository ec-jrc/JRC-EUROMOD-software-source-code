using System.Collections.Generic;

namespace EM_Executable
{
    internal class ParCond : ParBase
    {
        internal ParCond(InfoStore infoStore) : base(infoStore) { }

        ParFormula parFormula = null;

        internal override void CheckAndPrepare(FunBase fun)
        {
            parFormula = new ParFormula(infoStore, true) { xmlValue = xmlValue, description = this.description };
            parFormula.CheckAndPrepare(fun);
        }

        internal override void ReplaceVarNameByIndex()
        {
            parFormula.ReplaceVarNameByIndex();
        }

        /// <summary> used for assessing global conditions, e.g. RunCond, Loop.BreakCond </summary>
        internal bool GetGlobalValue()
        {
            if (infoStore.hhAdmin == null && !IsGlobal(true))
                return true; // note: this allows calling the function at read-time
                             // which is ok with things like {IsUsedDatabase#1}, {GetSystemYear=2016}, {1}, etc. (IsGlobal(true) == true)
                             // but does not work with global variables, which may change over run-time (IsGlobal(true) == false)
                             // as the function returns true for the latter at read-time it must be called again at run-time (see e.g. FunDefVar)
            return parFormula.GetGlobalValue() != 0;
        }

        /// <summary> 
        /// get whether the condition is true for a specific person
        /// note: this is strictly individual, that means that e.g. incomelists and monetary-variables are evaluated for the person
        /// </summary>
        internal bool GetPersonValue(HH hh, Person person) 
        {
            return parFormula.GetValue(hh, new List<Person>() { person }, person) != 0; 
        }

        /// <summary> get whether the condition is true for each individual in the taxunit </summary>
        internal Dictionary<byte, bool> GetTUValues(HH hh, List<Person> tu, bool getPersonValues = false)
        {
            Dictionary<byte, bool> values = new Dictionary<byte, bool>();

            Dictionary<string, double> operands = new Dictionary<string, double>();
            bool doTU = true; // attempt to enhance speed: calculate TU-level operands only once
            foreach (Person person in tu)
            {
                // prepare the operands
                foreach (var op in parFormula.operands)
                {
                    List<Person> curTU = op.Value.alternativeTU == null ? tu : new List<Person>() { person };
                    Person curPerson = !op.Value.isTULevel || getPersonValues && op.Value.alternativeTU == null ? person : null;
                    if (doTU) // 1st person: get all operands (individual ops for current (i.e. 1st) person, others on TU-level) ...
                    {
                        operands.Add(op.Key, op.Value.GetValue(hh, curTU, curPerson));
                    }
                    else // ... all other persons: only get individual-level operands (for current person)
                    {
                        if (op.Value.isGlobal || (op.Value.isTULevel && !getPersonValues && op.Value.alternativeTU == null)) continue;

                        operands[op.Key] = op.Value.GetValue(hh, curTU, curPerson);
                    }
                }
                // evaluate the formula (using the operand-values assessed above)
                values.Add(person.indexInHH, parFormula.GetValue(operands) != 0);
                doTU = false;
            }
            return values;
        }

        /// <summary>
        /// does condition refer to variables which are equal for each person, this is necessary for checking RunCond
        /// strong = true: condition may only contain global queries and numbers, i.e. no global variables (which may change at run-time)
        /// i.e. strong means is-read-time-available
        /// </summary>
        internal bool IsGlobal(bool strong = false)
        {
            return parFormula.IsGlobal(strong);
        }
    }
}
