using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    internal class AddOnSystemInfo
    {
        internal string _addOnShortName = string.Empty;
        internal string _addOnSystemName = string.Empty;
        internal List<string> _supportedSystems = null;
        internal List<string> _notSupportedSystems = null;

        internal AddOnSystemInfo(string addOnShortName, string addOnSystemName)
        {
            _addOnShortName = addOnShortName;
            _addOnSystemName = addOnSystemName;
            _supportedSystems = new List<string>();
            _notSupportedSystems = new List<string>();
        }
    }

    internal class AddOnInfoHelper
    {
        internal static List<AddOnSystemInfo> GetAddOnSystemInfo(string addOnShortName)
        {
            List<AddOnSystemInfo> addOnSystemInfoList = new List<AddOnSystemInfo>();

            CountryConfigFacade countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(addOnShortName);
            foreach (CountryConfig.SystemRow addOnSystemRow in countryConfigFacade.GetSystemRows())
            {
                AddOnSystemInfo addOnSystemInfo = new AddOnSystemInfo(addOnShortName, addOnSystemRow.Name);
               
                //assess supported/excluded systems from func_AddOn_Applic
                List<CountryConfig.FunctionRow> functionAddOnApplic = countryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(addOnSystemRow.ID, DefFun.AddOn_Applic);
                if (functionAddOnApplic.Count <= 0)
                    continue; //no AddOn_Applic found - system is in fact not available

                foreach (CountryConfig.ParameterRow parameterRow in functionAddOnApplic.First().GetParameterRows())
                {
                    if (parameterRow.Name.ToLower().StartsWith(DefPar.AddOn_Applic.Sys.ToLower()))
                        addOnSystemInfo._supportedSystems.Add(parameterRow.Value.ToLower());
                    if (parameterRow.Name.ToLower().StartsWith(DefPar.AddOn_Applic.SysNA.ToLower()))
                        addOnSystemInfo._notSupportedSystems.Add(parameterRow.Value.ToLower());
                }

                addOnSystemInfoList.Add(addOnSystemInfo);
            }

            return addOnSystemInfoList;
        }

        internal static bool GetSupportedSystemInfo(string systemName, List<AddOnSystemInfo> totalAOSystemInfo,
                                                    out List<AddOnSystemInfo> supportedAOSystemInfo)
        {
            supportedAOSystemInfo = new List<AddOnSystemInfo>();
            foreach (AddOnSystemInfo addOnSystemInfo in totalAOSystemInfo)
                if (IsSystemSupported(systemName, addOnSystemInfo)) supportedAOSystemInfo.Add(addOnSystemInfo);
            return supportedAOSystemInfo.Count() <= 1 || UserInfoHandler.GetInfo($"Warning: add-on {supportedAOSystemInfo[0]._addOnShortName} contains more than one system running with {systemName} ({supportedAOSystemInfo[0]._addOnSystemName}, {supportedAOSystemInfo[1]._addOnSystemName})", MessageBoxButtons.OKCancel) == DialogResult.OK;
        }

        internal static bool IsSystemSupported(string systemName, List<AddOnSystemInfo> addOnSystemInfoList)
        {
            foreach (AddOnSystemInfo addOnSystemInfo in addOnSystemInfoList)
                if (IsSystemSupported(systemName, addOnSystemInfo)) return true;
            return false;
        }

        internal static bool IsSystemSupported(string systemName, AddOnSystemInfo addOnSystemInfo)
        {
            bool isSupported = false;
            foreach (string pattern in addOnSystemInfo._supportedSystems)
                if (EM_Helpers.DoesValueMatchPattern(pattern, systemName))
                {
                    isSupported = true;
                    break;
                }
            foreach (string pattern in addOnSystemInfo._notSupportedSystems)
                if (EM_Helpers.DoesValueMatchPattern(pattern, systemName))
                {
                    isSupported = false;
                    break;
                }
            return isSupported;
        }
    }
}
