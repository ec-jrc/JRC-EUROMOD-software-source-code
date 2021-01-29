using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary> paramter can contain either a variable or an IL </summary>
    internal class ParVarIL : ParBase
    {
        internal ParVarIL(InfoStore infoStore) : base(infoStore) { }

        ParVar parVal = null; ParIL parIL = null;
        internal bool isMonetary = false;       // should this varil be formatted like monetary

        internal override void CheckAndPrepare(FunBase fun)
        {
            GetFootnoteNumbers(xmlValue, out string potentialIL);

            if (infoStore.operandAdmin.Exists(potentialIL) &&
                infoStore.operandAdmin.GetParType(potentialIL) == DefPar.PAR_TYPE.IL)
            {
                parIL = new ParIL(infoStore) { xmlValue = xmlValue, description = this.description };
                parIL.CheckAndPrepare(fun);
            }
            else
            {
                parVal = new ParVar(infoStore) { xmlValue = xmlValue, description = this.description };
                parVal.CheckAndPrepare(fun);
            }

            if (infoStore.operandAdmin.GetIsMonetary(GetName())) { isMonetary = true; }
        }

        internal override void ReplaceVarNameByIndex()
        {
            if (parVal == null) parIL.ReplaceVarNameByIndex(); else parVal.ReplaceVarNameByIndex();
        }

        internal double GetValue(HH hh, List<Person> tu, Person person = null)
        {
            return parVal == null ? parIL.GetValue(hh, tu, person) : parVal.GetValue(hh, tu, person);
        }

        internal string GetName() { return parVal == null ? parIL.name : parVal.name; }
    }
}
