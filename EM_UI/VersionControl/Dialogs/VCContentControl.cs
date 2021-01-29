using EM_Common;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCContentControl : UserControl
    {
        internal const string NOT_ASSESSED = "Not Assessed";
        const string NOT_AVAILABLE = "Not Available";

        VCAdministrator _vcAdministrator = null;
        VCAPI _vcAPI = null;

        internal VCContentControl()
        {
            InitializeComponent();
        }

        internal void Init(VCAdministrator vcAdministrator, bool statusVisible, bool versionsVisible, bool commentsVisible)
        {
            _vcAdministrator = vcAdministrator;
            _vcAPI = _vcAdministrator._vcAPI;
            colStatus.Visible = statusVisible;
            colRemoteVersions.Visible = versionsVisible;
            colComment.Visible = commentsVisible;
            Fill();
        }

        internal bool checkComments()
        {
            bool allOK = true;
            foreach (DataGridViewRow row in GetIncluded())
                if (row.Cells[colComment.Index].Value == null || row.Cells[colComment.Index].Value.ToString().Trim() == string.Empty)
                    allOK = false;

            return allOK;
        }

        internal bool AssessStati(bool checkForNoSelection = false, bool forceCheckAll = false)
        {
            List<DataGridViewRow> rowsToAssess = new List<DataGridViewRow>();
            List<VersionControlUnitInfo> unitsToAssess = new List<VersionControlUnitInfo>();
            foreach (DataGridViewRow row in GetIncluded(checkForNoSelection, forceCheckAll))
                if (row.Cells[colStatus.Index].Value.ToString() == NOT_ASSESSED)
                    { rowsToAssess.Add(row); unitsToAssess.Add(row.Tag as VersionControlUnitInfo); }
            
            List<string> unitStati; if (!_vcAdministrator.GetUnitsStati(unitsToAssess, out unitStati, forceCheckAll)) return false;     // we only "forceCheckAll" in the bundle upload, and we also need to "checkBundleExistence" there... 
            for (int i = 0; i < rowsToAssess.Count; ++i)
            {
                rowsToAssess[i].Cells[colStatus.Index].Value = unitStati[i];
                if (unitStati[i] == VCAdministrator.VC_STATUS_UPTODATE || unitStati[i] == NOT_ASSESSED)
                {
                    rowsToAssess[i].ReadOnly = true;
                    rowsToAssess[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
                
            }
            return true;
        }

        void Fill()
        {
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.WaitCursor;
            colRemoteVersions.Items.Add(NOT_ASSESSED); //that's for not getting an error-message while filling the grid because of showing a value in the combo-box-text, which is not in the combo-box-list

            List<VersionControlUnitInfo> units; if (!_vcAPI.GetLocalUnits(out units) || units == null) return;
            foreach (VersionControlUnitInfo unitInfo in units)
            {
                //add the unit
                int index = grid.Rows.Add(false, //check-box include
                    unitInfo.Name, UnitTypeToString(unitInfo.UnitType),
                    NOT_ASSESSED,  //versions (initially hidden)
                    NOT_ASSESSED); //status (initially hidden)
                grid.Rows[index].Tag = unitInfo;
            }
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.Default;
        }

        static string AddVersionMarkers(string displayText, string id, string uiId, string mostRecentId)
        {
            return string.Format("{0}{1}{2}", id==uiId ? VCAdministrator.UI_VERSION : string.Empty, 
                                              id== mostRecentId ? VCAdministrator.MOST_RECENT_VERSION : string.Empty, displayText);
        }

        static string RemoveVersionMarkers(string displayText)
        {
            if (displayText.StartsWith(VCAdministrator.UI_VERSION)) displayText = displayText.Substring(VCAdministrator.UI_VERSION.Length);
            if (displayText.StartsWith(VCAdministrator.MOST_RECENT_VERSION)) displayText = displayText.Substring(VCAdministrator.MOST_RECENT_VERSION.Length);
            return displayText;
        }

        List<DataGridViewRow> GetIncluded(bool checkForNoSelection = false, bool forceCheckAll = false)
        {
            List<DataGridViewRow> includedRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in grid.Rows)
                if (forceCheckAll || EM_Helpers.SaveConvertToBoolean(row.Cells[colInclude.Index].Value))
                    includedRows.Add(row);
            if (checkForNoSelection && includedRows.Count == 0) UserInfoHandler.ShowInfo("Please select at least one unit, by ticking the box in the column 'Include'.");
            return includedRows;
        }

        internal bool AnyUnitSelected() { return GetIncluded().Count > 0; }

        internal static string UnitTypeToString(VCAPI.VC_FOLDER_TYPE unitType)
        {
            switch (unitType)
            {
                case VCAPI.VC_FOLDER_TYPE.COUNTRY: return "Country";
                case VCAPI.VC_FOLDER_TYPE.ADDON: return "Add-On";
                case VCAPI.VC_FOLDER_TYPE.CONFIG: return "Config";
                case VCAPI.VC_FOLDER_TYPE.INPUT: return "Input";
                case VCAPI.VC_FOLDER_TYPE.LOG: return "Log";
                default: return "Unknown";
            }
        }

        internal static VCAPI.VC_FOLDER_TYPE UnitTypeFromString(string unitType)
        {
            switch (unitType)
            {
                case "Country": return VCAPI.VC_FOLDER_TYPE.COUNTRY;
                case "Add-On": return VCAPI.VC_FOLDER_TYPE.ADDON;
                case "Config": return VCAPI.VC_FOLDER_TYPE.CONFIG;
                case "Input": return VCAPI.VC_FOLDER_TYPE.INPUT;
                case "Log": return VCAPI.VC_FOLDER_TYPE.LOG;
                default: return VCAPI.VC_FOLDER_TYPE.UNDEFINED;
            }
        }

        internal void checkInclude(bool check) {
            foreach (DataGridViewRow row in grid.Rows)
                if (row.Cells[colStatus.Index].Value.ToString() != VCAdministrator.VC_STATUS_UPTODATE && row.Cells[colStatus.Index].Value.ToString() != NOT_ASSESSED)
                    row.Cells[colInclude.Index].Value = check;
        }

        internal bool GetUploadInfo(out List<VersionControlUnitInfo> unitInfos, out List<string> stati)
        { return GetUpDownloadInfo(out unitInfos, out stati, false, true); }

        bool GetUpDownloadInfo(out List<VersionControlUnitInfo> unitInfos, out List<string> stati, 
                               bool includeVersionInfo = true, bool includeStatusInfo = true) // bool includeCommentInfo = true, out List<string> comments,
        {
            Cursor bkUpMainFormCursor = EM_AppContext.Instance.GetActiveCountryMainForm().Cursor, bkUpCursor = Cursor;
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursor = Cursors.WaitCursor;

            unitInfos = new List<VersionControlUnitInfo>();
            stati = new List<string>();

            foreach (DataGridViewRow row in GetIncluded()) //run over all selected units
            {
                string selectedVersion = row.Cells[colRemoteVersions.Index].Value.ToString();

                if (includeVersionInfo && selectedVersion == NOT_AVAILABLE) continue; //ignore selected unit if version needed, but no version available
                VersionControlUnitInfo unitInfo = row.Tag as VersionControlUnitInfo;
                unitInfos.Add(unitInfo);

                if (!includeStatusInfo) stati.Add(null); //for symmetry
                else
                {
                    string unitStatus = row.Cells[colStatus.Index].Value.ToString();
                    if (unitStatus == NOT_ASSESSED) stati.Add(_vcAdministrator.GetUnitStatus(unitInfo));
                    else stati.Add(unitStatus);
                }
            }
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = bkUpMainFormCursor; Cursor = bkUpCursor;
            if (unitInfos.Count == 0) return false;
            return true;
        }
    }
}
