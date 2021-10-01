using System.Text;

namespace EM_Common
{
    public static class DefGeneral
    {
        public const string UI_VERSION = "3.4.6";
        public const string UI_VERSION_FOR_ASSEMBLY = UI_VERSION + ".0";

        public static Encoding DEFAULT_ENCODING = Encoding.UTF8; // this Encoding allows displaying country specific characters, while the originally used Encoding.Default did not

        public const string POSTFIX_SIMULATED = "_s";
        public static int IMAGE_IND_FUN = 0;
        public static int IMAGE_IND_POL = 1;
        public static int IMAGE_IND_REF = 2;
        public static int IMAGE_IND_PRIV_POL = 3;
        public static int IMAGE_IND_PRIV_REF = 4;
        public static int IMAGE_IND_PRIV_FUN = 5;
        public static int IMAGE_IND_PRIV_PAR = 6;

        public static string BRAND_TITLE = "EUROMOD";
        public static string BRAND_NAME = "EUROMOD";
        public static readonly string BRAND_NAME_DEFAULT = "EUROMOD";

        public static bool IsAlternativeBrand() { return BRAND_NAME != BRAND_NAME_DEFAULT || BRAND_TITLE != BRAND_NAME_DEFAULT; }
    }
}
