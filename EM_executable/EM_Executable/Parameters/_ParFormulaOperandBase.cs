using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary> base class for all parameters that can be used in a formula </summary>
    internal abstract class ParBase_FormulaComponent : ParBase
    {
        internal ParBase_FormulaComponent(InfoStore infoStore) : base(infoStore) { }

        internal bool isGlobal = true;   // this is used in ConditionParam.IsGlobalCond (for RunCond checking)
        internal bool isTULevel = false; // ParIL and ParVar set this true, the latter only for monetary variables (to achieve TU-summation)
                                         // ParQuery treats it depending on query (e.g. IsHead - false, nPersonInTU - true)
                                         // used e.g. for optimising condition-evaluation

        internal abstract double GetValue(HH hh, List<Person> tu, Person person = null);
    }
}
