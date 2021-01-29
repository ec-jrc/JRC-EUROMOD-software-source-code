using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    public partial class Control
    {
        private void SwitchOffUprateSetDefault()
        {
            foreach (string funName in new List<string>() { DefFun.Uprate, DefFun.SetDefault })
            {
                foreach (var fun in GetSpecificFuns(funName))
                {
                    bool isApplicable = false;
                    foreach (var par in from p in fun.Value.pars.Values where p.Name.ToLower() == DefPar.Uprate.Dataset.ToLower() select p)
                        if (infoStore.IsUsedDatabase(par.val)) isApplicable = true;
                    if (!isApplicable) fun.Value.on = false;
                }
            }
        }
    }
}
