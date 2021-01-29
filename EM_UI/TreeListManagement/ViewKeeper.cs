using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.TreeListManagement
{
    internal static class ViewKeeper
    {
        private class CountryViewSetting
        {
            internal Dictionary<string, int> systemWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            internal List<string> hiddenSystems = new List<string>();
            internal List<string> hiddenNodes = new List<string>();
            internal Tuple<float, float> textSize = new Tuple<float, float>(8.25f, 8.25f);
        }

        private static Dictionary<string, CountryViewSetting> countryViewSettings = new Dictionary<string, CountryViewSetting>(StringComparer.OrdinalIgnoreCase);
        private static bool keepMode = true;

        internal static void StoreSettings(EM_UI_MainForm mainForm)
        {
            if (!keepMode) return;
            CountryViewSetting setting = new CountryViewSetting();
            string cc = mainForm.GetCountryShortName();
            try
            {
                foreach (TreeListColumn col in mainForm.treeList.Columns)
                {
                    setting.systemWidths.Add(col.Caption, col.Width);
                    if (!col.Visible) setting.hiddenSystems.AddUnique(col.Caption, true);
                }
                foreach (TreeListNode polNode in mainForm.treeList.Nodes)
                {
                    AddHiddenNode(ref setting.hiddenNodes, polNode);
                    foreach (TreeListNode funcNode in polNode.Nodes)
                    {
                        AddHiddenNode(ref setting.hiddenNodes, funcNode);
                        foreach (TreeListNode parNode in funcNode.Nodes) AddHiddenNode(ref setting.hiddenNodes, parNode);
                    }
                }
                if (mainForm.GetTreeListBuilder() != null)
                    setting.textSize = mainForm.GetTreeListBuilder().GetTextSize();

                if (countryViewSettings.ContainsKey(cc))
                    countryViewSettings[cc] = setting;
                else countryViewSettings.Add(cc, setting);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Failed to store view settings for " + cc + ". Settings are set back to default.", false);
                if (countryViewSettings.ContainsKey(cc)) countryViewSettings.Remove(cc);
            }
        }

        private static void AddHiddenNode(ref List<string> hiddenNodes, TreeListNode node)
        {
            if (node.Visible || node.Tag == null) return;
            BaseTreeListTag tag = node.Tag as BaseTreeListTag; if (tag == null) return;
            hiddenNodes.AddUnique(tag.GetDefaultID(), true);
        }

        private static void SetHiddenNode(List<string> hiddenNodes, TreeListNode node)
        {
            if (node.Tag == null) return;
            BaseTreeListTag tag = node.Tag as BaseTreeListTag; if (tag == null) return;
            if (hiddenNodes.Contains(tag.GetDefaultID(), true)) node.Visible = false; // this assumes that ids do not change (which should actually be the case between country-close and reopen, and if not - not much harm done)
        }

        internal static void RestoreSettings(EM_UI_MainForm mainForm, bool exceptionCall = false)
        {
            if (!keepMode) return;
            string cc = mainForm.GetCountryShortName();
            try
            {
                if (!countryViewSettings.ContainsKey(cc)) return;
                CountryViewSetting setting = countryViewSettings[cc];
                foreach (TreeListColumn col in mainForm.treeList.Columns)
                {
                    if (setting.systemWidths.ContainsKey(col.Caption)) col.Width = setting.systemWidths[col.Caption];
                    if (setting.hiddenSystems.Contains(col.Caption, true)) col.Visible = false;
                    if (setting.hiddenSystems.Count > 0) mainForm.showHiddenSystemsBox();
                }
                foreach (TreeListNode polNode in mainForm.treeList.Nodes)
                {
                    SetHiddenNode(setting.hiddenNodes, polNode);
                    foreach (TreeListNode funcNode in polNode.Nodes)
                    {
                        SetHiddenNode(setting.hiddenNodes, funcNode);
                        foreach (TreeListNode parNode in funcNode.Nodes) SetHiddenNode(setting.hiddenNodes, parNode);
                    }
                }
                if (setting.textSize != null) mainForm.GetTreeListBuilder().SetTextSize(setting.textSize);
            }
            catch (Exception exception)
            {
                if (exceptionCall) return; // to avoid an infinite loop because of some unknown problem
                UserInfoHandler.ShowException(exception, "Failed to restored view settings. Settings are set back to default.", false);
                if (countryViewSettings.ContainsKey(cc)) countryViewSettings[cc] = new CountryViewSetting();
                else countryViewSettings.Add(cc, new CountryViewSetting());
                RestoreSettings(mainForm, true);
            }
        }

        internal static bool GetKeepMode() { return keepMode; }

        internal static void SetKeepMode(bool mode)
        {
            keepMode = mode; if (!keepMode) countryViewSettings.Clear();
        }

        private const char splitCountry = '°';
        private const char splitSetting = '^';
        private const char splitList = '|';
        private const string markSystemWidths = "01";
        private const string markHiddenSystems = "02";
        private const string markHiddenNodes = "03";
        private const string markTextSize = "04";

        internal static void Init(string keptSettings)
        {
            try
            {
                countryViewSettings.Clear();
                if (keptSettings == null || keptSettings == string.Empty) return;
                string[] countries = keptSettings.Split(splitCountry); if (countries.Count() < 1) return;
                keepMode = Convert.ToBoolean(countries.First()); if (keepMode == false) return;
                foreach (string country in countries.Skip(1))
                {
                    string[] settings = country.Split(splitSetting); if (settings.Count() < 1) continue;
                    CountryViewSetting setting = new CountryViewSetting();
                    foreach (string s in settings.Skip(1))
                    {
                        string[] items = s.Split(splitList); if (items.Count() < 1) continue;
                        switch (items.First())
                        {
                            case markSystemWidths:
                                int i = 1;
                                while (i + 1 < items.Count()) { setting.systemWidths.Add(items[i], int.Parse(items[i + 1])); i += 2; }
                                break;
                            case markHiddenSystems:
                                foreach (string item in items.Skip(1)) setting.hiddenSystems.AddUnique(item, true);
                                break;
                            case markHiddenNodes:
                                foreach (string item in items.Skip(1)) setting.hiddenNodes.AddUnique(item, true);
                                break;
                            case markTextSize:
                                if (items.Count() >= 3) setting.textSize = new Tuple<float, float>((float)EM_Helpers.SaveConvertToDouble(items[1]), (float)EM_Helpers.SaveConvertToDouble(items[2]));
                                break;
                        }
                    }
                    countryViewSettings.Add(settings.First(), setting);
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Failed to apply stored view settings. Settings are set back to default.", false);
                countryViewSettings.Clear();
            }
        }

        // generate user-setting-string as:
        // "keepMode°country1-settings° ... °countryX-settings", with country-settings as:
        // "country-short-name^01|system-widths^02|hidden systems^03|hidden nodes^04|text-size", with
        // system-widths: "Policy|policy-column-with|Grp/No|group-column-width|system1-name|system1-width| ... |systemX-name|systemX-width|Comment|comment-column-widht"
        // hidden systems: "system-name1| ... |system-nameX"
        // hidden nodes: "node-id1| ... |node-idX", where node-id is what one gets by calling BaseTreeListTag.GetDefaultID
        // text-size: "size1|size2", as used by TreeListBuilder.GetTextSize/SetTextSize
        // example:
        // True°MT^01|Policy|184|Grp/No|51|MT_2007|138|MT_2008|138|MT_2009|138|MT_2010|138|MT_2011|138|MT_2012|138|MT_2013|138|MT_2014|138|MT_2015|138|MT_2016|138|Comment|205
        //        ^02|mt_2009|mt_2011|mt_2013
        //        ^03|FE08A670-11E8-44E0-B252-4F7EFBD264F4|26D04809-3A88-475E-BFFA-D08CB7A8A36D|277E4A9C-B656-4129-B59A-77D1D51B3E98|46B7E1F6-7338-4694-B4AF-F5297C86867C|a27793f4-447d-42f8-8ba9-529378560eed|1FA3B90F-0A46-4011-A86A-AA6D5F93A46F|BAC4D66B-8D78-498C-8CDC-E3D0C5EA12FF|8B894E7E-BB92-4F2D-9F89-63E60CAA0A58|3B690AD9-B908-4C05-800E-D63D9164ED6B|CA8A4CAF-C318-4E64-85C7-91A71A075738|FE15E471-BB69-4D33-938F-AB8DDF85546A|06AAF911-DA85-4A5A-B843-3322BA55F7D5|8FB5A2A5-E83F-475E-9A35-1631B6A810CA|972FD183-6619-4B29-8D7C-D61129C4C20B|B4BEC492-CEE1-4EC2-AE54-1A3439265641|C8F84404-508C-416E-AE38-A453F7D29171|2E3FBBDC-9F7B-49A0-BCD4-2350F38C8600|c12c8d9b-da21-4134-8d47-26418693657e|2A530611-FBD7-48F3-B8D6-CCC5CE54778D|AECC3969-F6E5-4DD7-939D-936EA85CFF57|3D259481-17A2-4E41-BCC6-64AF6184F7A5|A3D2F8BC-8DAB-4A38-82A0-E577AFD4B15E|d3858356-a965-454d-83d2-2cddb241ed75|f56087aa-c64d-4da0-8df2-46fe835536b2|99085CE5-F758-4CB3-AE06-7C75EB7E5B5E|D1A12E7B-3353-4026-9007-B8EC61629663|39242415-315E-41A4-93DE-4E6DC4C6B594|0C0A9FEF-9F84-492C-9B64-918A6F5F4EDD|885511E6-576E-4F80-BC2F-7D7F7E8AE2FD|22B0B701-88FE-4EED-A362-D6B591517F23|3652821C-E0DD-43A5-8187-BCF158C76AF1|4CE5471F-DD83-4FE2-8CE7-6935FA139418|50DAC69C-3094-45AE-92DE-6888EA1D6362|1F9AD442-C321-41BB-9BFA-30A21F36D649|3E48C71A-DF55-4D56-A883-2B4DB908BCC6|FF3DC2FD-B76B-40C4-9999-37926766755B|796e1fec-cfc8-427e-974f-e767909adacd|bd9f9dc3-e867-441c-ac92-b9aa7aae0b40|17197737-F404-4797-BAFE-00207DFFB5F8|64A0D6A9-5971-41E0-BA48-6AA00CE03980|FA677FB0-6010-4FAB-9EF5-10BDCE7804E9|DC706013-6D7E-49A6-9C71-DD1233A39BC6|FFBA3400-1AB7-4707-8836-E975247BAF80|9b51791e-6eb4-4553-ac86-363d9cf225b2|4356e3ad-6733-4a67-b910-3d5a7a037d8e|C60EA05C-A048-4028-808A-8B72422FEC7A|362009D7-6F5C-4CAD-9D3E-4DB31EA883BC|3BE4FA13-4155-4751-8C4C-BF32211C5BD8|966F3564-7B3F-4798-83C2-5544E6AB00DC|2B1089DA-F50F-40CC-A4BC-0D52AF987419|4C14431F-ED43-4AAD-9C3A-8F2D834D8D3D|D63D536B-8B6E-4474-BCDF-0D860F966BA1|39012505-B76D-4F47-9AB3-14DDEC811617|27DD0592-8BB7-4379-A6A1-C1CD5A6E3CDB|3C433951-7DCD-4484-8EF9-C1BFCA62CBBA|4EC01038-BC3A-4EBD-B0D7-CFB3D39DAC5B|21b3bd0b-12cd-4354-a337-65e98c7f876d|df0e2e0f-63c5-4d99-9493-ee2e9fd8dc9d|8f2f1da4-731d-4775-b2cb-65414811e29b|5E8BBC54-F153-46D3-B5A8-7CEBE253918F|186F4A14-13A8-40CA-AB9F-E18D2FEB66D6|FA6E8261-94DD-4D0D-A543-BDA09B76CB5D|D72CAD2D-0C37-4996-BF8A-97F096FB06DB|C99E6B63-5537-4A54-B29E-CCBD9AD64183|B328F4A9-D8EC-4773-8F5E-51A3B4A930DB|5CE89C79-9A0C-4E12-8442-F77604322373|63FF43DC-6276-40B5-8EAE-3BF8A35784C3|290CD48B-847D-46DB-A9CD-AFE0F470B4FB|4D458C76-6A1F-41C7-8E9E-FF457F868470|D5EF52FB-F0AD-4E58-8F96-7DCBC81B2BFC|20F07114-4EE8-406B-886D-3A06D716DEDC|918A9B26-7D57-43E3-8883-DF973CDFF27B|2087B0FE-F330-447A-B266-311B5F499714|F9EB6503-25AC-4A1F-AE9B-EE9B39B0C56B|9A9C78E9-64F7-42AA-89A4-7ADCFD6034FD|D65E63BF-6645-4CEB-A941-8D50CA15568E|7A4492E5-79CB-4FC8-A105-65707F665CB9|FA30D873-D0C9-461C-8161-D760DE2C7B85|0D12AEB9-8CE4-434A-8AD7-884BF759B8EB|2ADCEE66-E2CA-40AD-A4E3-29A1E32673EA|37BB645C-8859-4914-B4DC-18DC0202943B|81C4B090-B23B-4AB4-AD01-338B86FF5599|5DBF3F05-5DBF-4B95-8CDC-9EAA52D2D077|51E35EAF-171D-4CC2-93A0-2587A35C380C|EE0C4FFC-0704-48D5-A889-704E11EA1965|C105A97B-C1AA-44F1-BC85-6D99BEE4F96D|F88E3603-9E76-4B66-B437-8868E61E2815|8FAD1216-B914-43ED-A01B-DCF99C940BB1|087BAD9C-903A-43D5-9366-6745F22465F9|493554F1-EA87-4EAA-A741-6837301C04EB|5269D7F1-CE52-4BB8-80F3-E56666384B73|180B2551-4BF9-4A05-AF1B-34A190586667|2388CDE6-B30A-4CB7-BE97-105670740719|7B16B20D-9F17-4EB5-AFAD-6AAAEAE0A754|F882981C-4C2F-4DB1-8F78-483C3309C9AD|6F141B87-145A-413E-A837-D82ECB36327E|0769D7C1-A72A-45CD-B731-65F33253DA19|15CCB796-24C8-4C50-B1BA-A371B6FCD752|3910501F-EB10-4EF1-B3B4-D847FC8B5368|27C5E7FC-4062-4635-BC85-90197DFD8070|BF2CAC2A-8534-4F4F-A658-F983C046D698|73B17367-6E70-4DB0-A744-06B6A461BEE8|467B1846-1BE6-4466-AFB0-73F64BF89FC2|1d5af92c-205f-4dd6-942c-32cd4b236831|AC1DA840-5A42-4C90-A866-5354FD4EF22C|33B7BE14-299B-431D-B47A-6D4D3D675AE9|7BBEC088-ECE8-4965-B8F1-2440E3B12542|8846ab09-4230-4ef3-bd96-228ca9946a5a|5e432d3a-d55c-4443-bf6e-c91e8000bdb6|66563166-7e93-4f53-93dc-2cad3f7c3f46|62cb9041-ffa1-4890-84de-46660fe51e03|9605a01c-c03a-4521-b400-533054cabb3c|ebda9d64-8caf-42b6-91af-a92842c3d71f|a0f3aabb-86a0-4d73-9bfb-288118c6f8bf|fb49b1c9-1e9b-458c-95e3-1b5c6d3091e1|067c1a65-4424-4bea-b59f-ae4b0b143a22|7b930637-df07-4d1d-a51e-1784bea6c190|ff8b802b-971b-4395-b957-a8cd6a40fa32|4f76d46e-ca11-4ce2-9137-42bf6da10fa7|a34f5f1b-0dd9-40ba-8575-5534ec8bde0a|8d4e95f5-1d46-4a72-b8ad-b0ae52dd20ed|75ab0b43-ae2e-435c-b6e8-b12cc477fb27|b47a1ac9-c691-43ff-a2da-2ec046f77da1|6d0e965d-7e0f-4d6d-8fa0-8855e4be27b8|3eccc867-51c5-4210-848a-3eab5a1465fd|9247f852-3d3a-479a-8f46-56368bcb6910|dc98c25a-048e-4097-a196-3ed8514348be|04f13518-165c-4f41-beb1-f5ebb9516d68|790d8e60-2bf9-4609-98d3-8e0c606cfc17|dc25e153-9dde-4362-864d-a3fe060dab3e|2664db6d-4dfa-4f76-935a-efc498431d6b|65e8802b-773f-4f7c-ab57-0394f20e1b3b|83a1d723-5c22-4cf4-b81f-a20cf4d23b12|b1dccf8d-9ec4-4f97-bfd0-e7310fd583d4|2e00466d-82e8-4d96-9bbb-b07c25a34fc6|9237aba7-16a6-49ac-abdf-e57d686df422|bd2ed9d6-f6a0-4279-bc28-739540efb84b|f6c5af39-d796-4421-a206-7a671ce7bf8a|c2c575ce-00a0-4fca-bc4e-24bb715ec288|5c6fa811-2afb-4f29-93da-1583f8e9e944|402fffbb-8a24-43c8-b9aa-d6760ab68a04|81d8b732-fa53-413d-940d-99501ce70a01|1f280356-5536-4293-bf95-7bac080dfb14|d82b3a57-547c-411a-a57d-ff3aef46e262|cac65213-d614-464f-9811-67edcf878db7|16a7d4c9-ed94-43cb-bd19-8241c967cebd|b49d9bcd-116f-419b-9294-7e13b5c47ff2|7a7a49e0-0735-4ea9-a0b3-ff6c65eadb0b|e873bd51-566a-45b3-9426-e0d92d7d4da4|2f8286a4-13f1-4114-aded-ea1b65f323e0|301ae6fc-7373-4099-b110-09232a2a74c0|0be9846c-b133-4335-9ac5-a77dee6daf7c|b7cde7d9-0f27-410f-bb08-e98148072c89|937a1690-b5a7-4691-8101-b5f922ce220e|ace79c9d-18b1-4777-9f0e-987608cdecac|888d5d00-dace-4a22-b884-1087a972e0c3|ab2a29cb-a72c-41bb-ace0-327ad8b634ac|3d91652c-23ee-415e-ab76-bff64b26b3cc|1d2a7ac1-0b72-4f06-922e-0163b0ed3e8d|cd883d7c-0761-402d-86e9-97ed5e79c219|48a06627-2c90-4989-88df-2e5a02708997|396a72d3-0e9a-49ec-813f-935d0170e3ba|99149f40-0938-4ada-ae30-21d80ea39d28|db2870aa-23db-4966-8ce6-ab8414030297|43bfb251-b694-488f-a154-3d0e38bd81df|f5517b9f-3087-44ba-adb5-6ad3519da05a|9d391838-9c42-4203-ad76-eb2f48cb1683|6c9f5672-459e-459d-81af-7db8256113bc|6a298256-370d-4699-a9cd-3c6fe5b46268|43bbb1a9-9042-4bee-9578-977697928f85|55903d25-5a5d-44df-81f9-23fe8efc7990|139f46e0-e15f-45d5-af74-97708641d681|a07ce0a6-a253-4904-8577-6b0b40ffce58|5f615b43-2e6b-47eb-82d5-635880b6ccad|d9fad68e-22a9-4846-bf2c-6c4f2040f820|7d33d901-53a9-40a2-9fdd-86ebea6dc807|aaee426b-edda-4620-823f-99833611ad9f|fce4ad4c-c530-4930-8cd8-5f8b814b0189|168486fd-4407-430c-8225-ca14018e3495|49b2b015-3e51-4a2a-9651-4712d4179319|e5dcbdde-580b-44aa-aa97-4edf7e2da248|8f34c126-4ef9-44b3-93fe-8147a4ff5112|0701db26-4e1b-4340-ba61-5605a2890b07|9c30c297-bfc4-45dd-aa42-bdf58fb7497c|f49a62ab-a7ff-448a-a635-8e9efbf1a138|410f06ad-41a3-4bb1-b5de-3e38af459fca|b2dbb7c2-31f5-453b-b506-9d00751dd727|ab3fd3bd-d85f-4d9b-adff-5f59395b3576|7cd7e09c-8e29-4477-ad20-94a6655362e3|840eb8da-42be-481c-8449-2171e386c24c|b1eee337-0af9-4125-bdd2-1aeff6b82230|4b4e8cf0-07cc-415a-8e9d-7981c51088c5|f581e9d2-c5c3-4a72-8132-2635b46353a6|7021eabd-49d7-4fdb-9c4a-76b3c10e7257|c6defca9-af0e-4550-b441-dbf02963f206|e411813e-af01-4b64-be6c-4e2264591699|7686db96-6dd0-4198-988a-b66f4a985b23|f89dd08e-d2eb-49d9-a927-be4d4834a223|2fe03e6b-a714-439b-8c62-ef5092e34a29|6b1a4ff0-1b5d-4fc0-b82a-25cf71721bf3|17dcb43b-046f-4cdb-9b5a-bba7aea76c41|6a3c0536-f0f6-4c90-b21f-4faf9a609ae7|89b20020-37ea-40a8-a159-e7f6318b3dc9|ef671c2f-b120-4c23-89c1-879cb10cff27|c7aa0e76-956b-4fae-9e6e-85a1f505f3e4|6979c082-2ef6-4a8b-85d4-0e68702c6633|78c9a1dc-8fad-4a1c-a7dd-736a16e64243|f49b675e-ea73-4784-965a-8c19d36eff49|3EFC859C-5529-4D5C-B56A-C24538246F5B|53977608-35D4-488F-82E9-8B3C7DA040AA|8382F924-5334-40D0-97B7-4EC1BB8AC2FA|5EA59764-2BAC-4099-BCA0-D272C75E6CD5|43B69E65-49AF-41F6-9526-F8FD90014A25|38E30521-1950-489A-A611-5A8471EFA9E1|0F06980E-C11D-4DB8-9223-FF83457A25F4|D77AAF27-5C9B-4763-8965-7D3AAC447180|B38B8CB7-2183-48D6-BB34-64F910936343|876E5F3A-8468-4026-9877-FC3ED49EF1AC|6C7B01CC-D38C-40EC-AF36-1E35291FF7EB|141029E1-4092-483D-9535-BB9112B9C134|05AD3888-6F11-4312-921F-40FA7C7F3CF6|88FDDD7A-A499-4A86-8D0F-9439280AA0BE|30610BBC-1217-4CC2-98BC-DEBBFE6ED8F9
        //        ^04|8.25|7.8
        internal static string GetStoreString()
        {
            string us = keepMode.ToString();
            foreach (var s in countryViewSettings)
            {
                us += splitCountry + s.Key;
                us += splitSetting + markSystemWidths;
                foreach (var item in s.Value.systemWidths) us += splitList + item.Key + splitList + item.Value.ToString();
                us += splitSetting + markHiddenSystems;
                foreach (string item in s.Value.hiddenSystems) us += splitList + item;
                us += splitSetting + markHiddenNodes;
                foreach (string item in s.Value.hiddenNodes) us += splitList + item;
                us += splitSetting + markTextSize + splitList + EM_Helpers.ConvertToString(s.Value.textSize.Item1) + splitList + EM_Helpers.ConvertToString(s.Value.textSize.Item2);
            }
            return us;
        }

        internal static bool IsHiddenSystem(string countryShortName, string systemName)
        {
            EM_UI_MainForm openMainForm = (EM_AppContext.Instance.GetCountryMainForm(countryShortName));
            if (openMainForm == null) return countryViewSettings.ContainsKey(countryShortName) &&
                                             countryViewSettings[countryShortName].hiddenSystems.Contains(systemName, true);
            TreeListColumn sc = openMainForm.GetTreeListBuilder().GetSystemColumnByName(systemName);
            return sc == null ? false : !sc.Visible;
        }
    }
}
