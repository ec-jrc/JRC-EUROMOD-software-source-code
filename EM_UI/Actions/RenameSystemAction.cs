using DevExpress.XtraTreeList.Columns;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.GlobalAdministration;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using EM_UI.Validate;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Actions
{
    internal class RenameSystemAction : BaseAction
    {
        TreeListColumn _senderColumn = null;
        CountryConfigFacade _countryConfigFacade = null;
        DataConfigFacade _dataConfigFacade = null;
        bool _actionIsCanceled = false;

        internal RenameSystemAction(TreeListColumn senderNode, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            _senderColumn = senderNode;
            _dataConfigFacade = dataConfigFacade;
            _countryConfigFacade = countryConfigFacade;
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
            CountryConfig.SystemRow systemRow = (_senderColumn.Tag as SystemTreeListTag).GetSystemRow();

            string newSystemName = systemRow.Name, oldSystemName = systemRow.Name, newSystemYear = systemRow.Year;
            if (!SystemValidation.GetSystemNameAndYear(_dataConfigFacade != null, ref newSystemName, ref newSystemYear, _senderColumn.TreeList))
            {
                _actionIsCanceled = true;
                return;
            }

            // main action: rename system
            systemRow.Name = newSystemName; systemRow.Year = newSystemYear;
            // rename references in database-system-connection (if 'country' isn't an add-on)
            if (_dataConfigFacade != null) _dataConfigFacade.RenameSystemInDBSystemConfigRows(systemRow.ID, newSystemName);
            // rename references in conditional formatting
            _countryConfigFacade.RenameSystemInConditionalFormats(oldSystemName, newSystemName);
            
            if (_dataConfigFacade != null)
            {
                // rename standard-output-files (last parameter (request): ask user if she actually wants this to happen)
                CopySystemAction.RenameInputOutputFiles(_countryConfigFacade, systemRow, oldSystemName, true);
                // change name in global exchange rate file
                ChangeNameInExchangeRatesConfig(systemRow, oldSystemName);
                // warn about not-adaptation of AddOn_Applic-policy
                OptionalWarningsManager.Show(OptionalWarningsManager._renameSystemWarning);
            }            
        }

        private void ChangeNameInExchangeRatesConfig(CountryConfig.SystemRow systemRow, string oldSystemName)
        {
            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false); if (excf == null) return;
            bool anyChange = false;
            foreach (ExchangeRatesConfig.ExchangeRatesRow exchangeRate in 
                from er in excf.GetExchangeRates() where er.Country.ToLower() == systemRow.CountryRow.ShortName.ToLower() &&
                                                         ExchangeRate.ValidForToList(er.ValidFor).Contains(oldSystemName.ToLower()) select er)
            {
                if (!anyChange && UserInfoHandler.GetInfo("Do you want to update the system name in the global exchange rate table?" + Environment.NewLine + Environment.NewLine +
                    "Note that, if no exchange rate is found for a system name, the exchange rate is assumed to be 1.", MessageBoxButtons.YesNo) == DialogResult.No) return;
                exchangeRate.ValidFor = ExchangeRate.RemoveFromValidFor(exchangeRate.ValidFor, oldSystemName);
                exchangeRate.ValidFor = ExchangeRate.AddToValidFor(exchangeRate.ValidFor, systemRow.Name); anyChange = true;
            }
            if (anyChange) excf.WriteXML();
        }
    }
}
