using System.Linq;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static string ExtractSystemYear(string systemName) //try to find 4 subsequent digits, supposing this is the system year		
        {
            int sIndex = -1, eIndex = -1; string digits = "0123456789", sName = systemName + " ";
            for (int i = 0; i < sName.Length; ++i)
            {
                if (digits.Contains(sName[i])) { if (sIndex == -1) sIndex = i; }
                else
                {
                    if (sIndex == -1) continue;
                    if (i - sIndex == 4) { eIndex = i; break; }
                    sIndex = -1;
                }
            }
            if (sIndex == -1 | eIndex == -1) return string.Empty;
            return systemName.Substring(sIndex, 4);
        }
    }
}
