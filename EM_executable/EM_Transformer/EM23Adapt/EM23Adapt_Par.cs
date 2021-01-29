namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptParProp(EM2Item par)
        {
            par.properties.Remove(EM2TAGS.COLOR);       // only relevant for UI (rethink for future UI)
            par.properties.Remove(EM2TAGS.VALUE_TYPE);  // redundant with FuncConfig (formula/condition/etc. in future stored in Common-lib)
        }
    }
}
