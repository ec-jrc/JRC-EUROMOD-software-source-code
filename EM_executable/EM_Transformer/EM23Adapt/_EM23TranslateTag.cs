namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        internal static string TranslateTag(string e2Tag, string mainTag)
        { // note: mainTag (e.g. SYS, POL, FUN, ...) may be necessary for specification, if items share tag-names
            switch (e2Tag)
            {
                case EM2TAGS.SYSTEM_ID: return EM_XmlHandler.TAGS.SYS_ID;
                case EM2TAGS.POLICY_ID: return EM_XmlHandler.TAGS.POL_ID;
                case EM2TAGS.FUNCTION_ID: return EM_XmlHandler.TAGS.FUN_ID;
                case EM2TAGS.PARAMETER_ID: return EM_XmlHandler.TAGS.PAR_ID;
                case EM2TAGS.ACRO_ID: return EM_XmlHandler.TAGS.ACRO_ID;
            }
            return e2Tag;
        }
    }
}
