using EM_XmlHandler;
using System.Collections.Generic;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        /// <summary> performs all EM2->EM3 adaptations for the variables file </summary>
        /// <in_out param name="content"> content of file as read by EM2Variables.Read </param>
        internal void AdaptVar(EM2Variables.Content content)
        {
            Dictionary<string, string> varCountryLabels = ShortLabels(content); // see below

            foreach (var v in content.variables)
            {
                // AutoLabel not necessary (was just stored for performance reasons in Excel-UI)
                v.Remove(EM2TAGS.AUTO_LABEL);

                // add the compressed labels to the variable, if there are labels (see below)
                if (varCountryLabels.ContainsKey(v[EM2TAGS.ID]))
                    v.Add(EM_XmlHandler.TAGS.LABELS, XmlHelpers.CDATA(varCountryLabels[v[EM2TAGS.ID]]));
            }
        }

        // country lables make 95% of the size of the file !!! therefore this attempts to reduce this a bit
        //
        // EM2 STRUCTURE: each variable has n structures of the form below for n countries that describe the variable:
        // <CountryLabel> <ID>...</ID><VariableID>...</VariableID><Country>..</Country><Label>...</Label></CountryLabel>
        //
        // EM3 STRUCTURE (per variable): <Labels><![CDATA[c1°c1-lable|c2°c2-label|...]]></Labels> (note: CDATA only once required)
        // e.g. <Labels><![CDATA[bg°Unemployment benefit (обезщетениe за безработица)|cy°Unemployment benefits from Social Insurance Fund|dk°Unemployment benefits and similar benefits (Arbejdsløshedsdagpenge og andre A- kasseydelser)|ee°unemployment insurance benefit (töötukindlustushüvitis)|el°unemployment insurance benefit (επίδομα ανεργίας)|es°Unemployment insurance (Prestación por desempleo)|fi°Earnings-related unemployment allowance|fr°Contributory unemployment benefit (ARE)|ie°jobseekers benefit (mainly)|lt°unemployment insurance benefit (nedarbo draudimo išmoka)|pt°benefit : unemployment : contributory|si°unemployment wage compensation (denarno nadomestilo za primer brezposelnosti)|sk°Contributory unemployment benefit|uk°JSA contributory|HR°unemployment benefit]]></Labels>
        //
        // note: this is a bit against our principle to keep things separate and avoid complicated string-constructs,
        // but perhaps it is justified by the reduction in size of the variables file ...
        // KOSTAS - T O D O (low priority): rethink this structure
        private Dictionary<string, string> ShortLabels(EM2Variables.Content content)
        {
            Dictionary<string, string> varCountryLabels = new Dictionary<string, string>();
            foreach (var lab in content.countryLabels)
            {
                if (lab[EM2TAGS.LABEL] == XmlHelpers.CDATA() ||
                    lab[EM2TAGS.LABEL] == XmlHelpers.CDATA("-")) continue; // empty
                string varID = lab[EM2TAGS.VAR_ID];
                string label = XmlHelpers.RemoveCData(lab[EM2TAGS.LABEL]);
                if (!varCountryLabels.ContainsKey(varID)) varCountryLabels.Add(varID, string.Empty);
                else varCountryLabels[varID] += "|";
                varCountryLabels[varID] += $"{lab[EM2TAGS.COUNTRY]}°{label}";
            }
            return varCountryLabels;
        }
    }
}
