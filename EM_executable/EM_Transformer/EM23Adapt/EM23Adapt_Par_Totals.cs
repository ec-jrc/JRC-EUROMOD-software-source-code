using EM_Common;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private const string AGG_VAR = "agg_var";
        private const string AGG_IL = "agg_il";

        // merge Agg_Var and Agg_IL to Agg (can have parameter type var-or-il, which is more handy for executable)
        private void AdaptTotals_Par(EM2Item par, EM2Item fun)
        {
            if (fun.name.ToLower() != DefFun.Totals.ToLower()) return;

            if (par.name.ToLower() == AGG_VAR ||
                par.name.ToLower() == AGG_IL) par.name = DefPar.Totals.Agg;
        }
    }
}
