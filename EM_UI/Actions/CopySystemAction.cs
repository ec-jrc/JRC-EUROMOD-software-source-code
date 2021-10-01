using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.GlobalAdministration;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using EM_UI.Validate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Actions
{
    internal class CopySystemAction : BaseAction
    {
        TreeListColumn _senderColumn = null;
        CountryConfigFacade _countryConfigFacade = null;
        DataConfigFacade _dataConfigFacade = null;
        bool _isAddOn = false;
        bool _actionIsCanceled = false;

        //add very first system
        internal CopySystemAction(CountryConfigFacade countryConfigFacade, bool isAddOn)
        {
            _countryConfigFacade = countryConfigFacade;
            _isAddOn = isAddOn;
        }

        //add system by copying an existing system
        internal CopySystemAction(TreeListColumn senderColumn, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            _senderColumn = senderColumn;
            _countryConfigFacade = countryConfigFacade;
            _dataConfigFacade = dataConfigFacade;
            _isAddOn = _dataConfigFacade == null;
        }
        
        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            CountryConfig.SystemRow systemRow = (_senderColumn == null) ? null : (_senderColumn.Tag as SystemTreeListTag).GetSystemRow();
            TreeList countryTreeList = (_senderColumn != null) ? _senderColumn.TreeList : null;
            string systemName = systemRow == null ? string.Empty : systemRow.Name;
            string systemYear = systemRow == null ? string.Empty : systemRow.Year;

            if (!SystemValidation.GetSystemNameAndYear(!_isAddOn, ref systemName, ref systemYear, countryTreeList))
            {
                _actionIsCanceled = true;
                return;
            }

            CountryConfig.SystemRow newSystemRow;
            if (_senderColumn == null) //add very first system
                newSystemRow = _countryConfigFacade.AddFirstSystemRow(systemName);
            else //add system by copying an existing system
            {
                CountryConfig.SystemRow toCopySystemRow = (_senderColumn.Tag as SystemTreeListTag).GetSystemRow();

                //copy system itself
                newSystemRow = CountryConfigFacade.CopySystemRow(systemName, toCopySystemRow);

                //copy all references of the system with datasets (i.e. new system can be used with same datasets as its template)
                if (!_isAddOn)
                    _dataConfigFacade.CopyDBSystemConfigRows(toCopySystemRow, newSystemRow);

                //copy system formats
                _countryConfigFacade.CopySystemFormatting(toCopySystemRow, newSystemRow);
                
                //rename output filename of default outputs
                RenameInputOutputFiles(_countryConfigFacade, newSystemRow, toCopySystemRow.Name);

                //adapt exchange-rate in global table
                //note that the exchange-rate-config is not included in the undo-procedure, thus this will not be undone
                AddToExchangeRatesTable(systemName, toCopySystemRow);
                
                ExtensionAndGroupManager.CopyExtensionAndGroupMemberships(_countryConfigFacade.GetCountryConfig(), newSystemRow.ID, toCopySystemRow.ID);
            }
            newSystemRow.Year = systemYear;
        }

        void AddToExchangeRatesTable(string systemName, CountryConfig.SystemRow toCopySystemRow)
        {
            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false); if (excf == null) return;
            bool found = false;
            foreach (ExchangeRatesConfig.ExchangeRatesRow exchangeRate in excf.GetExchangeRates())
            {
                if (exchangeRate.Country.ToLower() != toCopySystemRow.CountryRow.ShortName.ToLower() ||
                    !ExchangeRate.ValidForToList(exchangeRate.ValidFor).Contains(toCopySystemRow.Name.ToLower())) continue;
                exchangeRate.ValidFor = ExchangeRate.AddToValidFor(exchangeRate.ValidFor, systemName); found = true; break;
            }
            if (found) excf.WriteXML();
        }

        internal static void RenameInputOutputFiles(CountryConfigFacade countryConfigFacade, CountryConfig.SystemRow systemRow, string oldSystemName, bool request = false, bool renameOutput = true, bool renameInput = true)
        {
            //search policies Output_Std_cc and Output_Std_HH_cc
            //CountryConfig.PolicyRow policyRow = countryConfigFacade.GetPolicyRowByName(systemRow.ID, standardOutputPolicyName);
            //if (policyRow == null) continue;
            //within these policies search for function(s) DefOutput
            //List<CountryConfig.FunctionRow> defOutputFunctionRows = countryConfigFacade.GetFunctionRowsByPolicyIDAndFunctionName(policyRow.ID, DefFun.DefOutput);

            var functionRows = from f in countryConfigFacade.GetCountryConfig().Function
                                        where f.PolicyRow.SystemID == systemRow.ID && ((renameOutput &&  f.Name.ToLower() == DefFun.DefOutput.ToLower()) || (renameInput &&  f.Name.ToLower() == DefFun.DefInput.ToLower()))
                                        select f;
            foreach (CountryConfig.FunctionRow functionRow in functionRows)
            {
                //within these functions search for parameter File
                List<CountryConfig.ParameterRow> parameterRows = countryConfigFacade.GetParameterRowsByName(functionRow.ID, DefPar.DefOutput.File);

                foreach (CountryConfig.ParameterRow parameterRow in parameterRows)
                {
                    //if parameter file found replace system name (e.g. rename UK_2009_std to UK_2009_reform_std)
                    int index = parameterRow.Value.ToLower().IndexOf(oldSystemName.ToLower());
                    if (index < 0) continue;
                    string newOutputName = parameterRow.Value.Substring(0, index) + systemRow.Name + parameterRow.Value.Substring(index + oldSystemName.Length);
                    if (request)
                    {
                        if (UserInfoHandler.GetInfo("Do you want to adapt the filenames in DefInput/DefOutput functions according to the new system name?" + Environment.NewLine +
                            string.Format("(e.g. '{0}' to '{1}')", parameterRow.Value, newOutputName), MessageBoxButtons.YesNo) == DialogResult.Yes)
                            request = false; // do not ask again for any other output-policy
                        else return; // do not rename
                    }
                    parameterRow.Value = newOutputName;
                }
            }
        }
    }
}
