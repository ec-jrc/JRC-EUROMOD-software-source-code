using DevExpress.XtraTreeList.Columns;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.Actions
{
    internal class DeleteSystemAction : BaseAction
    {
        TreeListColumn _senderColumn = null;
        DataConfigFacade _dataConfigFacade = null;
        bool _actionIsCanceled = false;
        bool _showRequest = true;

        internal DeleteSystemAction(TreeListColumn senderNode, DataConfigFacade dataConfigFacade, bool showRequest = true)
        {
            _senderColumn = senderNode;
            _dataConfigFacade = dataConfigFacade;
            _showRequest = showRequest;
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

            if (_showRequest && Tools.UserInfoHandler.GetInfo("Are you sure you want to delete system '" + _senderColumn.Caption + "'?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                _actionIsCanceled = true;
                return;
            }

            //adapt exchange-rate in global table
            //note that the exchange-rate-config is not included in the undo-procedure, thus this will not be undone
            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
            if (excf != null) excf.RemoveSystems(systemRow.CountryRow.ShortName, new List<string>() { systemRow.Name });

            if (_dataConfigFacade != null) //if 'country' is an add-on it does not have a dataConfig
                _dataConfigFacade.DeleteDBSystemConfigRows(systemRow.ID); //first delete all references of the system with datasets
            CountryConfigFacade.DeleteSystemRow(systemRow); //then delete the system itself
        }
    }
}
