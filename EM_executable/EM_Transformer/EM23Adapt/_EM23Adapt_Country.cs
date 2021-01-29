using System.Collections.Generic;
using System.Linq;

namespace EM_Transformer
{
    /// <summary>
    /// this class is responsible for all EM2->EM3 CONTENT adaptations
    /// where content is understood in contrast to STRUCTURE (see comment in EMCountry_Def)
    /// </summary>
    public partial class EM23Adapt
    {
        /// <summary>
        /// performs all EM2->EM3 content (see above) adaptations for country- and AddOn-files
        /// *!*!*!*!*!*!*  NOTE: ALL "explanation wrt ..." ARE FOUND IN explanations.cs  *!*!*!*!*!*!*
        /// </summary>
        /// <param name="ctryContent"> content of country-XML-file as read by EM2Country.Read </param>
        /// <param name="dataContent"> content of country-dataconfig-XML-file as read by EM2Data.Read </param>
        /// <param name="extensionInfo"> content of global policy-switches-XML-file (provides info for adaptation, see below) </param>
        internal void AdaptCountry(EM2Country.Content ctryContent, EM2Data.Content dataContent,
                                   List<List<MultiProp>> extensionInfo,
                                   out List<string> errors)
        {
            errors = new List<string>();

            // ***  C O U N T R Y  ***
            ctryContent.general.id = null; // remove country-id (which never had any use)

            // ***  A D A P T   O R D E R   T O   V I S U A L   O R D E R  ***
            // ***  S E T   F U N C T I O N ' S   R U N - O P T I O N   ***
            AdaptOrder(ctryContent);

            // ***  S Y S T E M S  ***
            foreach (var sys in ctryContent.systems)
                AdaptSysProp(sys.Value);                             // remove outdated properties (ExchangeRateEuro, Private, ...)

            // ***  P O L I C I E S  ***
            foreach (var pol in ctryContent.policies)
            {
                AdaptPolProp(pol.Value);                             // remove outdated properties (Type, Color, Private, ...)
                GatherRefPol(pol.Value, ctryContent);                // see "explanation wrt to new handling of reference policies"
            }
            RemoveRefPol(ctryContent);                               // see "explanation wrt to new handling of reference policies"

            // ***  F U N C T I O N S  ***
            List<string> funIds = (from f in ctryContent.functions select f.Key).ToList();
            foreach (string funId in funIds) // it is necessary to run over the ids instead of the functions themselves
            {                                // because we may add functions and thus change the collection
                EM2Item fun = ctryContent.functions[funId];
                AdaptFunProp(fun);                              // remove outdated properties (Color, Private, ...)
                AdaptChangeParam_Fun(fun, ctryContent, errors); // see "explanation wrt to new handling of ChangeParam"
            }

            // ***  P A R A M E T E R S  ***
            foreach (var par in ctryContent.parameters)
            {
                AdaptParProp(par.Value);         // remove outdated properties (Color, ValueType, Private, ...)
                EM2Item fun = ctryContent.functions[par.Value.properties[EM2TAGS.FUNCTION_ID]];
                AdaptTotals_Par(par.Value, fun); // merge Agg_Var and Agg_IL
                AdaptCZDefConfig_Par(par.Value); // solve a special case for CZ (actually a syntax error due to very old notation)
                AdaptFootnotes_Par(par.Value); // replace #_LowLim_Amount by #_LowLim, #_Income by #_Info, etc.
            }

            // ***  S Y S T E M   V A L U E S   O F   P O L I C I E S  ***
            foreach (var sysPol in ctryContent.sysPol)
            {
                ReplaceSwitchAndToggle(sysPol); // see "explanation wrt to toggle and switch"
            }

            // ***  S Y S T E M   V A L U E S   O F   F U N C T I O N S  ***
            foreach (var sysFun in ctryContent.sysFun)
            {
                ReplaceSwitchAndToggle(sysFun); // see "explanation wrt to toggle and switch"
            }

            // ***  S Y S T E M   V A L U E S   O F   P A R A M E T E R S  ***
            foreach (var sysPar in ctryContent.sysPar)
            {
                ReplaceEmptyByNA(sysPar); // it seems that there are (faulty!) parameter values set to "<![CDATA[]]>" (i.e. empty CDATA)
                ReplaceWhoMustBeEligAliases(sysPar); // replace 'one_member' by 'one', 'all_members' and 'taxunit' by 'all' and 'all_adult' by 'all_adults'
            }                             


            // ***  U P R A T I N G   I N D I C E S  ***
            foreach (var upIndex in ctryContent.upInd)
            {
                AdaptYearValues(upIndex.Value, ctryContent.upIndVal, EM_XmlHandler.TAGS.UPIND_ID); // put year-values in own elements (see comment in function)
            }

            // ***  I N D I R E C T   T A X E S  ***
            foreach (var indTax in ctryContent.indTax)
            {
                AdaptYearValues(indTax.Value, ctryContent.indTaxVal, EM_XmlHandler.TAGS.INDTAX_ID); // put year-values in own elements (see above)
            }

            if (dataContent == null) return;
            // BREAK HERE FOR ADD-ONS !!! the rest only concerns countries
            //---------------------------------------------------------------------------------------

            // ***  D A T A S E T S  ***
            //foreach (var dataSet in dataContent.dataSets) { } // nothing to do currently 

            // ***  S Y S T E M - D A T A - C O M B I N A T I O N S  ***
            foreach (var sysData in dataContent.sysData)
                AdaptSysDataProp(sysData); // remove outdated properties (UseCommonDefault, ...)

            // ***  S W I T C H - G R O U P S  ***
            for (int i = dataContent.policySwitches.Count - 1; i >= 0; --i) // see "explanation wrt new handling of policy switches"
            {
                AdaptExtensionSwitchProp(dataContent.policySwitches[i], extensionInfo, ctryContent.policies, out bool remove, ctryContent);
                if (remove) // remove if no respective switch policy found (maybe outdated switch) or if set to n/a
                    dataContent.policySwitches.RemoveAt(i);
            }
        }
    }
}
