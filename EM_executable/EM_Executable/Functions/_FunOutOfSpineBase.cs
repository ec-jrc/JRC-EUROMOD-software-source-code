using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary>
    /// each "regular tax-ben" function inherits from this class
    /// more concrete: each function with DefinitionAdmin.Fun.defaultRunOption != IN_SPINE 
    /// </summary>
    internal class FunOutOfSpineBase  : FunBase
    {
        internal FunOutOfSpineBase(InfoStore infoStore) : base(infoStore)
        {
        }

        internal virtual void Run()
        {
            if (!IsRunCondMet()) return;
            DoFunWork();
        }

        protected virtual void DoFunWork() { }
    }
}
