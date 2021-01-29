using EM_Common;
using EM_XmlHandler;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        private void AdaptSysProp(EM2Item sys)
        {
            sys.properties.Remove(EM2TAGS.COUNTRY_ID);         // dispensable
            sys.properties.Remove(EM2TAGS.EXCHANGE_RATE_EURO); // moved to global file ExchangeRates.xml
        }

        // see "explanation wrt to toggle and switch" in Explanations.cs
        private void ReplaceSwitchAndToggle(EM2Country.SysItem sysItem) // works for policies and functions
        {
            switch (sysItem.value.ToLower())
            {
                case DefPar.Value.SWITCH: // switch-policies are as a default off and can be activated by respective extension-switches
                    sysItem.value = DefPar.Value.OFF; break;
                case DefPar.Value.TOGGLE: // toggle not needed anymore: off-switched policies/functions are only dropped after possible switch-changes
                    sysItem.value = DefPar.Value.OFF; break;
                case "": // rather unlikely (actually an error correction)
                    sysItem.value = DefPar.Value.NA; break;
            }
        }

        // it seems that there are (faulty!) parameter values set to "<![CDATA[]]>" (i.e. empty CDATA)
        private void ReplaceEmptyByNA(EM2Country.SysItem sysPar)
        {
            if (sysPar.value == XmlHelpers.CDATA()) sysPar.value = DefPar.Value.NA;
        }

        private void ReplaceWhoMustBeEligAliases(EM2Country.SysItem sysPar)
        {
            switch (XmlHelpers.RemoveCData(sysPar.value).ToLower())
            {
                case "one_member": sysPar.value = DefPar.Value.WHO_ONE; break;
                case "all_members": sysPar.value = DefPar.Value.WHO_ALL; break;
                case "taxunit": sysPar.value = DefPar.Value.WHO_ALL; break;
                case "all_adult": sysPar.value = DefPar.Value.WHO_ALL_ADULTS; break;
            }
        }
    }
}
