namespace EM_Common
{
    public class AddOn
    {
        public static string ComposeSymbolicID(string polName, string funOrder, string parOrder = null) // e.g. output_std_fr_#1.1
        {
            return ($"{polName}_#{funOrder}" + (parOrder == null ? string.Empty : $".{parOrder}")).ToLower();

            // note: EM_Executable.Control.ReplaceSymbolicIDs uses this also for composing symbolic ids for DefTU and DefIL,
            // where funOrder is in fact iltuName, e.g. TUDef_fr_#tu_hh_oecd_co.2
        }

        public static bool ResolveSymbolicID(string symbolicID, out string polName, out string funOrder, out string parOrder)
        {
            polName = funOrder = parOrder = null;

            if (!symbolicID.Contains("_#")) { polName = symbolicID; return true; }

            string[] sp1 = symbolicID.Split('#'); if (sp1.Length != 2) return false;
            polName = sp1[0].TrimEnd(new char[] { '_' });

            if (!sp1[1].Contains(".")) funOrder = sp1[1];
            else
            {
                string[] sp2 = sp1[1].Split('.'); if (sp2.Length != 2) return false;
                funOrder = sp2[0]; parOrder = sp2[1];
            }

            if (!int.TryParse(funOrder, out int fDummy) &&
                !funOrder.ToLower().StartsWith("tu") && // this is a questionable way to take care of the symbolic ids for DefTU and DefIL (see note above)
                !funOrder.ToLower().StartsWith("il")) return false;

            if (parOrder != null && !int.TryParse(parOrder, out int pDummy)) return false;

            return true;
        }

        public static bool IsParSymbolicID(string symbolicID)
        {
            if (!ResolveSymbolicID(symbolicID, out string polName, out string funOrder, out string parOrder)) return false;
            return parOrder != null;
        }
    }
}
