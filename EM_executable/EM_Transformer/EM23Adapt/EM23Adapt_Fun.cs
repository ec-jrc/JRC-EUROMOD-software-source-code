namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptFunProp(EM2Item fun)
        {
            fun.properties.Remove(EM2TAGS.COLOR);   // only relevant for UI (rethink for future UI)
        }
    }
}
