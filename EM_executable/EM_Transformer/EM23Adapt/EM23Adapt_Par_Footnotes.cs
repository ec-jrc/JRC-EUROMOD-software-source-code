using EM_Common;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptFootnotes_Par(EM2Item par)
        {
            if (par.name.ToLower() == "#_lowlim_amount") par.name = DefPar.Footnote.LowLim;
            if (par.name.ToLower() == "#_uplim_amount") par.name = DefPar.Footnote.UpLim;
            if (par.name.ToLower() == "#_means") par.name = DefQuery.Par.Val;
        }
    }
}
