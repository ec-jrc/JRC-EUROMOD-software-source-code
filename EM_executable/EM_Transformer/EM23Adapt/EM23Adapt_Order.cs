using EM_Common;
using System.Collections.Generic;
using System.Linq;
using System;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        // sort policies, functions and parameters and to achieve continuous numbers
        // the purpose of this action is to produce error messages in the executable which reflect the visual order in the UI
        // the decision to do it here instead of the XmlReader was because we need some ordering anyway for setting the run-options (see below)
        // (note that the respective orders and parent-ids were stored with the pol/fun/par-items in the reading process only for this purpose)
        private void AdaptOrder(EM2Country.Content ctryContent)
        {
            Dictionary<string, string> itemOrders = new Dictionary<string, string>();
            List<EM2Item> orderedFuns = new List<EM2Item>(); // this is for setting function's run-options (see below)

            long oPol = 1;
            foreach (var pol in ctryContent.policies.Values.OrderBy(o => o.order))
            {
                itemOrders.Add(pol.id, oPol.ToString()); ++oPol;

                long oFun = 1;
                foreach (var fun in ctryContent.functions.Values.Where(w => w.partentId == pol.id).OrderBy(o => o.order))
                {
                    itemOrders.Add(fun.id, oFun.ToString()); ++oFun;
                    orderedFuns.Add(fun);

                    long oPar = 1;
                    foreach (var par in ctryContent.parameters.Values.Where(w => w.partentId == fun.id).OrderBy(o => o.order))
                    {
                        itemOrders.Add(par.id, oPar.ToString()); ++oPar;
                    }
                }
            }

            // once we have the proper order put it to the <SysPol>/<SysFun>/<SysPar>-items were the order is actually stored in the EM3 format
            foreach (var sysPol in ctryContent.sysPol) sysPol.order = itemOrders[sysPol.itemID];
            foreach (var sysFun in ctryContent.sysFun) sysFun.order = itemOrders[sysFun.itemID];
            foreach (var sysPar in ctryContent.sysPar) sysPar.order = itemOrders[sysPar.itemID];
        }
    }
}
