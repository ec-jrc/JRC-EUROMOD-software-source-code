using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static string ShortCountryToFullCountry(string c)
        {
            string fullCountry = "Unknown Country";
            Dictionary<string, string> allCountries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"AF", "Afghanistan"}, {"AL", "Albania"}, {"DZ", "Algeria"}, {"AS", "American Samoa"}, {"AD", "Andorra"}, {"AO", "Angola"}, {"AI", "Anguilla"}, {"AQ", "Antarctica"}, {"AG", "Antigua and Barbuda"},
                {"AR", "Argentina"}, {"AM", "Armenia"}, {"AW", "Aruba"}, {"AU", "Australia"}, {"AT", "Austria"}, {"AZ", "Azerbaijan"}, {"BS", "Bahamas"}, {"BH", "Bahrain"}, {"BD", "Bangladesh"}, {"BB", "Barbados"},
                {"BY", "Belarus"}, {"BE", "Belgium"}, {"BZ", "Belize"}, {"BJ", "Benin"}, {"BM", "Bermuda"}, {"BT", "Bhutan"}, {"BO", "Bolivia"}, {"BQ", "Bonaire"}, {"BA", "Bosnia and Herzegovina"}, {"BW", "Botswana"},
                {"BV", "Bouvet Island"}, {"BR", "Brazil"}, {"IO", "British Indian Ocean Territory"}, {"BN", "Brunei Darussalam"}, {"BG", "Bulgaria"}, {"BF", "Burkina Faso"}, {"BI", "Burundi"}, {"KH", "Cambodia"},
                {"CM", "Cameroon"}, {"CA", "Canada"}, {"CV", "Cape Verde"}, {"KY", "Cayman Islands"}, {"CF", "Central African Republic"}, {"TD", "Chad"}, {"CL", "Chile"}, {"CN", "China"}, {"CX", "Christmas Island"},
                {"CC", "Cocos (Keeling) Islands"}, {"CO", "Colombia"}, {"KM", "Comoros"}, {"CG", "Congo"}, {"CD", "Democratic Republic of the Congo"}, {"CK", "Cook Islands"}, {"CR", "Costa Rica"}, {"HR", "Croatia"},
                {"CW", "Curacao"}, {"CY", "Cyprus"}, {"CZ", "Czechia"}, {"CI", "Cote d'Ivoire"}, {"DK", "Denmark"}, {"GP", "Guadeloupe"}, {"DJ", "Djibouti"}, {"DM", "Dominica"}, {"DO", "Dominican Republic"},
                {"EC", "Ecuador"}, {"EG", "Egypt"}, {"SV", "El Salvador"}, {"GQ", "Equatorial Guinea"}, {"ER", "Eritrea"}, {"EE", "Estonia"}, {"ET", "Ethiopia"}, {"FK", "Falkland Islands (Malvinas)"}, {"TG", "Togo"},
                {"FO", "Faroe Islands"}, {"FJ", "Fiji"}, {"FI", "Finland"}, {"FR", "France"}, {"GF", "French Guiana"}, {"PF", "French Polynesia"}, {"TF", "French Southern Territories"}, {"WF", "Wallis and Futuna"},
                {"GM", "Gambia"}, {"GE", "Georgia"}, {"DE", "Germany"}, {"GH", "Ghana"}, {"GI", "Gibraltar"}, {"EL", "Greece"}, {"GL", "Greenland"}, {"GD", "Grenada"}, {"VI", "US Virgin Islands"}, {"JO", "Jordan"},
                {"GT", "Guatemala"}, {"GG", "Guernsey"}, {"GN", "Guinea"}, {"GY", "Guyana"}, {"HT", "Haiti"}, {"HM", "Heard Island and McDonald Mcdonald Islands"}, {"GU", "Guam"}, {"ZM", "Zambia"}, {"YT", "Mayotte"},
                {"VA", "Holy See (Vatican City State)"}, {"HN", "Honduras"}, {"HK", "Hong Kong"}, {"HU", "Hungary"}, {"IS", "Iceland"}, {"IN", "India"}, {"ID", "Indonesia"}, {"IR", "Iran, Islamic Republic of"},
                {"IQ", "Iraq"}, {"IE", "Ireland"},{"IM", "Isle of Man"}, {"IL", "Israel"}, {"IT", "Italy"}, {"JM", "Jamaica"}, {"JP", "Japan"}, {"JE", "Jersey"}, {"KZ", "Kazakhstan"}, {"KE", "Kenya"}, {"CU", "Cuba"},
                {"KI", "Kiribati"}, {"KP", "Korea, Democratic People's Republic of"}, {"KR", "Korea, Republic of"}, {"KW", "Kuwait"}, {"KG", "Kyrgyzstan"}, {"LA", "Lao People's Democratic Republic"}, {"LV", "Latvia"},
                {"LB", "Lebanon"}, {"LS", "Lesotho"}, {"LR", "Liberia"}, {"LY", "Libya"}, {"LI", "Liechtenstein"}, {"LT", "Lithuania"}, {"LU", "Luxembourg"}, {"MO", "Macao"}, {"YE", "Yemen"}, {"ZW", "Zimbabwe"},
                {"MK", "Macedonia, the Former Yugoslav Republic of"},  {"MG", "Madagascar"}, {"MW", "Malawi"}, {"MY", "Malaysia"}, {"MV", "Maldives"}, {"ML", "Mali"}, {"MT", "Malta"}, {"MH", "Marshall Islands"},
                {"MQ", "Martinique"}, {"MR", "Mauritania"}, {"MU", "Mauritius"}, {"MX", "Mexico"}, {"FM", "Micronesia, Federated States of"}, {"MD", "Moldova, Republic of"}, {"MC", "Monaco"}, {"GW", "Guinea-Bissau"},
                {"MN", "Mongolia"}, {"ME", "Montenegro"}, {"MS", "Montserrat"}, {"MA", "Morocco"}, {"MZ", "Mozambique"}, {"MM", "Myanmar"}, {"NA", "Namibia"}, {"NR", "Nauru"}, {"NP", "Nepal"}, {"NL", "Netherlands"},
                {"NC", "New Caledonia"}, {"NZ", "New Zealand"}, {"NI", "Nicaragua"}, {"NE", "Niger"}, {"NG", "Nigeria"}, {"NU", "Niue"}, {"NF", "Norfolk Island"}, {"MP", "Northern Mariana Islands"}, {"NO", "Norway"},
                {"OM", "Oman"}, {"PK", "Pakistan"}, {"PW", "Palau"}, {"PS", "Palestine, State of"}, {"PA", "Panama"}, {"PG", "Papua New Guinea"}, {"PY", "Paraguay"}, {"PH", "Philippines"}, {"US", "United States"},
                {"PN", "Pitcairn"}, {"PL", "Poland"}, {"PT", "Portugal"}, {"PR", "Puerto Rico"}, {"QA", "Qatar"}, {"RO", "Romania"}, {"RU", "Russian Federation"}, {"RW", "Rwanda"}, {"RE", "Reunion"}, {"GA", "Gabon"},
                {"BL", "Saint Barthelemy"}, {"SH", "Saint Helena"}, {"KN", "Saint Kitts and Nevis"}, {"LC", "Saint Lucia"}, {"MF", "Saint Martin (French part)"}, {"PM", "Saint Pierre and Miquelon"}, {"PE", "Peru"},
                {"VC", "Saint Vincent and the Grenadines"}, {"WS", "Samoa"}, {"SM", "San Marino"}, {"ST", "Sao Tome and Principe"}, {"SN", "Senegal"}, {"RS", "Serbia"}, {"SG", "Singapore"}, {"EH", "Western Sahara"},
                {"SX", "Sint Maarten (Dutch part)"}, {"SK", "Slovakia"}, {"SI", "Slovenia"}, {"SB", "Solomon Islands"}, {"SO", "Somalia"}, {"GS", "South Georgia and the South Sandwich Islands"}, {"SS", "South Sudan"},
                {"ES", "Spain"}, {"LK", "Sri Lanka"}, {"SD", "Sudan"}, {"SR", "Suriname"}, {"SJ", "Svalbard and Jan Mayen"}, {"SZ", "Swaziland"}, {"SE", "Sweden"}, {"CH", "Switzerland"}, {"SY", "Syrian Arab Republic"},
                {"TW", "Taiwan, Province of China"}, {"TJ", "Tajikistan"}, {"TZ", "United Republic of Tanzania"}, {"TH", "Thailand"}, {"TL", "Timor-Leste"}, {"TK", "Tokelau"}, {"TO", "Tonga"}, {"UK", "United Kingdom"},
                {"TT", "Trinidad and Tobago"}, {"TN", "Tunisia"}, {"TR", "Turkey"}, {"TM", "Turkmenistan"}, {"TC", "Turks and Caicos Islands"}, {"TV", "Tuvalu"}, {"UG", "Uganda"}, {"UA", "Ukraine"}, {"VU", "Vanuatu"},
                {"AE", "United Arab Emirates"}, {"UM", "United States Minor Outlying Islands"}, {"UY", "Uruguay"}, {"UZ", "Uzbekistan"}, {"VE", "Venezuela"}, {"VN", "Viet Nam"}, {"VG", "British Virgin Islands"}, 
                // conflicting & extras
                {"SCT", "Scotland"}, {"SC", "Scotland"}, {"SA", "South Africa"}, {"SL", "SimpleLand"}, {"WL", "Wales" }
                //{"SL", "Sierra Leone"}, {"SA", "Saudi Arabia"}, {"SC", "Seychelles"},
            };

            if (allCountries.ContainsKey(c.ToUpper())) fullCountry = allCountries[c.ToUpper()];

            return fullCountry;
        }

        public static string OutputNameToPretty(string filename)
        {
            // first get rid of the known extras
            filename = filename.Replace(".txt", "").Replace("_std", "");
            List<string> bits = filename.Split('_').ToList();
            if (bits.Count == 1) return ShortCountryToFullCountry(bits[0].ToUpper());
            else if (bits.Count > 1)
            {
                string nice = ShortCountryToFullCountry(bits[0].ToUpper());
                for (int i = 1; i < bits.Count; i++)
                    nice += " " + bits[i];
                return nice;
            }
            else return "";
        }

        public static Color GetColourFromString(string colourNameOrCode)
        {
            if (colourNameOrCode == null || colourNameOrCode == string.Empty) return Color.Empty;
            return colourNameOrCode[0] == '#' ? Color.FromArgb(int.Parse("FF" + colourNameOrCode.Substring(1), System.Globalization.NumberStyles.HexNumber)) : Color.FromName(colourNameOrCode);
        }

        public static string StripHTMLSpecialCharacters(string html)
        {
            return System.Web.HttpUtility.HtmlDecode(html);
        }
    }
}
