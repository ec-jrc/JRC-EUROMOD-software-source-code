using EM_Common;

namespace EM_Executable
{
    /// <summary> variable-parameters for setting values, i.e. output_var, output_add_var and result_var </summary>
    internal class ParOutVar : ParBase
    {
        internal ParOutVar(InfoStore infoStore) : base(infoStore) { }

        internal string name = string.Empty;
        internal int index = -1;

        internal override void CheckAndPrepare(FunBase fun)
        {
            name = xmlValue;
            infoStore.operandAdmin.CheckRegistration(name, true, // isOutVar
                    description.GetParName() == DefPar.Common.Output_Add_Var, // warnIfNotInit
                    description);
            
            // remark: the warnIfNotInit for OutputAddVar does in fact not work, because the registration sets the variable initialised
            // because it is an output-variable, I did not change this because it is not trivial and in fact correct
            // the old exe only issued this warning because variables were initialised with VOID, now we are intialising with 0
        }

        internal override void ReplaceVarNameByIndex() { index = infoStore.operandAdmin.GetIndexInPersonVarList(name); }
    }
}
