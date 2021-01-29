using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    public partial class Control
    {
        private void ReplaceCCSys(Dictionary<string, EM_XmlHandler.ExeXml.Pol> pols)
        {
            foreach (var pol in pols) ReplaceCCSys(pol.Value);
        }

        private void ReplaceCCSys(EM_XmlHandler.ExeXml.Pol pol)
        {
            foreach (var fun in pol.funs.Values)
                foreach (var par in fun.pars.Values)
                {
                    par.val = par.val.Replace(DefPar.Value.PLACEHOLDER_CC, infoStore.country.cao.shortName.ToLower());
                    par.val = par.val.Replace(DefPar.Value.PLACEHOLDER_SYS, infoStore.country.sys.name.ToLower());
                }
        }
    }
}
