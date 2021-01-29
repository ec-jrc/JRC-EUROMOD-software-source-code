using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.ImportExport;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Run
{
    internal partial class RunMainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        string _countryShortName = "no country loaded";
        GalleryItemGroup _gigCountries = null;
        bool _isBestMatchOnlyChecked = false;
        bool _isRegularExpressionChecked = false;
        int _contextMenuColumn = -1;

        const string infoPrivate = " (Private)";
        const string infoBestMatch = " (Best Match)";

        internal bool _doNotPoolSystemsDatasets = false;

        Dictionary<string, List<AddOnSystemInfo>> _addOnInfo = null;

        internal const string _showSelectedHHOptions = "showSelectedHHOptions";
        internal const string _hideHiddenSystems = "hideHiddenSystems";
        internal const string _doNotStopOnNonCriticalErrors = "doNotStopOnNonCriticalErrors";
        internal const string _addDateToOuputFilename = "addDateToOuputFilename";
        internal const string _logRuntimeInDetail = "logRuntimeInDetail";
        internal const string _closeAfterRun = "closeAfterRun";
        internal const string _labelRunFormInfoText = "RunFormInfoText";
        internal const string _colAddOnPrefix = "colAddOn";
        internal const string _colPolicySwitchPrefix = "colPolicySwitch";
        internal const string _policySwitchDefaultValueIndicator = " (default)";
        internal const string _runPublicOnly = "runPublicOnly";
        internal const string _warnAboutUselessGroups = "warnAboutUselessGroups";

        private bool _runEM2 = false;

        void btnRun_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                string outputPath = EMPath.AddSlash(txtOutputPath.Text);
                if (!Directory.Exists(outputPath))
                {
                    Tools.UserInfoHandler.ShowError("Please select a valid output path.");
                    return;
                }

                string emVersion = EM_AppContext.Instance.GetProjectName();
                if (emVersion.Trim() == string.Empty)
                {
                    UserInfoHandler.ShowError($"{DefGeneral.BRAND_TITLE} version is not defined. Please define it via the menu 'Configuration'.");
                    return;
                }

                string nHH;
                if (!GetNumberOfHHToRun(out nHH) &&
                    UserInfoHandler.GetInfo("'Run first N Households only' is set to " + nHH + ", which is not a valid number of households to run." + Environment.NewLine + Environment.NewLine +
                                            "Ignore and run all households?", MessageBoxButtons.YesNo) == DialogResult.No) return;

                ActiveControl = ribbon; //just to force data-grid-view to loose focus, otherwise cell-values may not be up-to-date

                Cursor = Cursors.WaitCursor; //use hour glass as generating add-ons may take some time

                //get the run-manager from the application instead of creating with new, because the application needs to know 
                //whether there are active run-managers (to warn in case of closing)
                //moreover the run-manager object must not be destroyed with the termination of the function or the run-dialog
                RunManager runManager = EM_AppContext.Instance.RegisterRunManager();

                bool success = runManager.KickOffRuns(outputPath, emVersion, this, _runEM2);

                Cursor = Cursors.Default;

                if (success && EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_closeAfterRun))
                    Hide();
                else
                    EM_AppContext.Instance.DeRegisterRunManager(runManager);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal DataConfig.DBSystemConfigRow GetSelectedDBSystemCombination(DataGridViewRow systemRow)
        {
            try
            {
                DataGridViewComboBoxCell cmbDatasets = systemRow.Cells[colDataset.Name] as DataGridViewComboBoxCell; //get the combo-box belonging to the system's row
                string selectedDataset = systemRow.Cells[colDataset.Name].Value.ToString();
                DataConfig.DBSystemConfigRow dbSystemConfigRow = null;
                int longestMatch = 0;
                foreach (DataConfig.DBSystemConfigRow dbSCRow in systemRow.Tag as List<DataConfig.DBSystemConfigRow>)
                {
                    if (selectedDataset.Length >= dbSCRow.DataBaseRow.Name.Length &&
                        selectedDataset.Substring(0, dbSCRow.DataBaseRow.Name.Length) == dbSCRow.DataBaseRow.Name) //assess substring because combobox may contain '(Best Match)'
                    {
                        if (dbSCRow.DataBaseRow.Name.Length < longestMatch) continue;
                        dbSystemConfigRow = dbSCRow;
                        longestMatch = dbSCRow.DataBaseRow.Name.Length; // make sure not taking "IT_2010_a3" for "IT_2010_a3_v5"
                    }
                }
                return dbSystemConfigRow;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return null;
            }
        }

        void InitForm()
        {   //load flags of all countries into the gallary (do this just once, i.e. when the form is first opened)
            try
            {
                _gigCountries = new GalleryItemGroup();
                foreach (Country country in CountryAdministrator.GetCountries())
                {
                    GalleryItem galleryItem = new GalleryItem();
                    galleryItem.Image = country._flag.GetThumbnailImage(16, 16, null, IntPtr.Zero); //make flags a little smaller to save space
                    galleryItem.Caption = country._shortName;
                    galleryItem.Hint = country._shortName;
                    _gigCountries.Items.Add(galleryItem);
                }
                gbiCountries.Gallery.Groups.Clear();
                gbiCountries.Gallery.Groups.Add(_gigCountries);
                gbiCountries.Gallery.ColumnCount = _gigCountries.Items.Count; //all countries should be visible, but no space wasted
                gbiCountries.Gallery.UseMaxImageSize = true; //size of gallery item is defined by largest image
                chkHideHiddenSystems.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_hideHiddenSystems);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void StoreSelection(out Dictionary<string, List<string>> selectionInfo, out Dictionary<string, string> extensionAllInfo)
        {
            selectionInfo = new Dictionary<string, List<string>>();
            extensionAllInfo = new Dictionary<string, string>();

            foreach (DataGridViewRow systemRow in dgvRun.Rows) //gather for each displayed system what's currently selected
            {
                string systemName = systemRow.Cells[colSystem.Name].Value.ToString(); //system will be the key in the selection-dictionary
                List<string> infos = new List<string>(); //what's selected will be put into this list (to be the value of the dictonary)

                string key = systemRow.Cells[colCountry.Index].Value.ToString() + systemName;

                foreach (DataGridViewColumn column in dgvRun.Columns)
                {
                    if (systemRow.Cells[column.Name].Value == null)
                        continue;
                    //assess which systems are selected for run (i.e. the system itself and/or any add-on-systems)
                    if (systemRow.Cells[column.Name].Value.GetType() == typeof(System.Boolean) && EM_Helpers.SaveConvertToBoolean(systemRow.Cells[column.Name].Value))
                        infos.Add(column.HeaderText); //if yes, add the heading of the column to the info (to be recognised in restore)
                    //store if 'all' is selected for any extension (as this is not stored in the user-settings as for on/off)
                    else if (systemRow.Cells[column.Name].Value.ToString() == DefPar.Value.EXTENSION_ALL)
                        extensionAllInfo.AddOrReplace(key, column.HeaderText);
                }

                //add the name of the selected dataset to the info
                infos.Add(colDataset.Name + systemRow.Cells[colDataset.Name].Value.ToString());

                //if specified add first/last household to the info
                if (systemRow.Cells[colFirstHH.Name].Value != null && systemRow.Cells[colFirstHH.Name].Value.ToString() != string.Empty)
                    infos.Add(colFirstHH.Name + systemRow.Cells[colFirstHH.Name].Value.ToString());
                if (systemRow.Cells[colLastHH.Name].Value != null && systemRow.Cells[colLastHH.Name].Value.ToString() != string.Empty)
                    infos.Add(colLastHH.Name + systemRow.Cells[colLastHH.Name].Value.ToString());

                if (!selectionInfo.ContainsKey(key)) //equal system-names may happen due to Save Country As (the ID is then equal too)
                    selectionInfo.Add(key, infos);   //therefore use country and system as key (still checking for equals to be on the save side)
            }
        }

        void RestoreSelection(Dictionary<string, List<string>> selectionInfo)
        {
            foreach (DataGridViewRow systemRow in dgvRun.Rows) //run over the displayed systems to restore any previous selection
            {
                string key = systemRow.Cells[colCountry.Index].Value.ToString() + systemRow.Cells[colSystem.Name].Value.ToString();
                if (!selectionInfo.ContainsKey(key))
                    continue; //unknown system, i.e. was not displayed before the update

                List<string> infos = selectionInfo[key]; //get the info about the previous selection-settings from the dictonary

                foreach (DataGridViewColumn column in dgvRun.Columns) //if the system itself or any add-on-system was selected for run, the info contains its column heading
                    if (infos.Contains(column.HeaderText))
                        systemRow.Cells[column.Name].Value = true;

                //restore if another dataset than the default (best match) was selected
                DataGridViewComboBoxCell cmbDatasets = systemRow.Cells[colDataset.Name] as DataGridViewComboBoxCell;
                for (int index = 0; index < cmbDatasets.Items.Count; ++index)
                    if (infos.Contains(colDataset.Name + cmbDatasets.Items[index]))
                        systemRow.Cells[colDataset.Name].Value = cmbDatasets.Items[index];

                foreach (string info in infos)
                {
                    //restore if first/last household was specified
                    if (info.StartsWith(colFirstHH.Name))
                        systemRow.Cells[colFirstHH.Name].Value = info.Substring(colFirstHH.Name.Length);
                    if (info.StartsWith(colLastHH.Name))
                        systemRow.Cells[colLastHH.Name].Value = info.Substring(colLastHH.Name.Length);
                }
            }
        }

        private void RestoreExtensionAllInfo(Dictionary<string, string> extensionRunInfo)
        { // extensionRunInfo.key = country+system, extensionRunInfo.value = caption of any extension-column for which 'all' is selected
            foreach (DataGridViewRow systemRow in dgvRun.Rows)
            {
                string key = systemRow.Cells[colCountry.Index].Value.ToString() + systemRow.Cells[colSystem.Name].Value.ToString();
                if (!extensionRunInfo.ContainsKey(key)) continue;
                foreach (DataGridViewColumn column in dgvRun.Columns)
                    if (extensionRunInfo[key] == column.HeaderText)
                        systemRow.Cells[column.Name].Value = DefPar.Value.EXTENSION_ALL;
            }
        }


        void UpdateForm(string countryShortName, List<KeyValuePair<GalleryItem, bool>> galleryItems = null)
        {   //update the form upon user actions within the form and each time the form is reactivated (i.e. Run-button pressed)
            try
            {
                if (_gigCountries == null)
                    return;

                if (galleryItems != null) //checking of gallery item by click needs do be done here, see gbiCountries_Gallery_ItemClick
                {
                    foreach (KeyValuePair<GalleryItem, bool> galleryItem in galleryItems)
                        galleryItem.Key.Checked = galleryItem.Value;
                }

                Cursor = Cursors.WaitCursor;

                if (dgvRun.Rows.Count > 0) //unless doing this the last manual selection gets lost if a country is added/removed
                    dgvRun.CurrentCell = dgvRun.Rows[0].Cells[colCountry.Name];

                //store the current selection ...
                StoreSelection(out Dictionary<string, List<string>> selectionInfo, out Dictionary<string, string> extensionRunInfo); //... systems selected for run and datasets selected
                StorePolicySwitchSettings(); //... on/off-settings of policy switches if displayed

                //update add-on-options (before filling in systems, as this needs to assess whether a displayed add-on is available for the system)
                UpdateAddOnInfo();

                //fill systems' grid
                dgvRun.Rows.Clear();
                foreach (GalleryItem galleryItem in _gigCountries.Items)
                {
                    if (_countryShortName != countryShortName) //only change the currently checked countries if the form is first loaded or if it was first loaded in an empty main-window (i.e. displayed all countries) and now a country is loaded
                        galleryItem.Checked = (galleryItem.Caption.ToLower() == countryShortName.ToLower());
                    if (galleryItem.Checked == true)
                        AddToSystemList(galleryItem.Caption); //add the systems of all selected countries, taking filters and add-on-options into account
                }

                //take view-options into account
                chkShowSelectedHH.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_showSelectedHHOptions);
                colFirstHH.Visible = EM_Helpers.SaveConvertToBoolean(chkShowSelectedHH.EditValue);
                colLastHH.Visible = EM_Helpers.SaveConvertToBoolean(chkShowSelectedHH.EditValue);

                //update advanced settings
                chkAddDateToOuputFilename.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_addDateToOuputFilename);
                chkCloseAfterRun.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_closeAfterRun);
                chkDoNotStopOnNonCriticalErrors.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_doNotStopOnNonCriticalErrors);
                chkLogRuntimeInDetail.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_logRuntimeInDetail);
                chkWarnAboutUselessGroups.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_warnAboutUselessGroups);
                //chkRunPublicOnly.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_runPublicOnly);
                customParallelRunsItem.EditValue = (EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRunsAuto ? "D" : "C") + EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRuns;

                txtOutputPath.Text = txtOutputPath.Text == null || txtOutputPath.Text == string.Empty ? EM_AppContext.FolderOutput : txtOutputPath.Text; //do not reset output-folder, if the user selected another one

                //indicate in ribbon-tab if any filter is active, as this is not obvious if another tab is selected than View / Filter / Add-Ons
                bool activeFilter = (txtFilterDatasets.EditValue != null && txtFilterDatasets.EditValue.ToString() != string.Empty) ||
                                    (txtFilterSystems.EditValue != null && txtFilterSystems.EditValue.ToString() != string.Empty) ||
                                    _isBestMatchOnlyChecked;
                rpView.Image = activeFilter ? global::EM_UI.Properties.Resources.filter1 : null;
                rpView.Category.Color = activeFilter ? System.Drawing.Color.Tomato : System.Drawing.Color.Transparent;

                //restore the selection and settings stored above
                RestoreSelection(selectionInfo);
                DisplayExtensionSettings();
                RestoreExtensionAllInfo(extensionRunInfo);

                if (dgvRun.Rows.Count > 0) //if a cell with a check-box is selected the check-box is not updated for some reason, therefore move selection to country column (i.e. a no-check-box-column)
                    dgvRun.CurrentCell = dgvRun.Rows[0].Cells[colCountry.Name];
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void UpdateAddOnInfo()
        {
            RepositoryItemCheckedComboBoxEdit cmbAddOnsEdit = cmbAddOns.Edit as RepositoryItemCheckedComboBoxEdit;

            //remember current selection of add-ons, to reinstall after update
            List<string> checkedAddOns = new List<string>();
            for (int index = 0; index < cmbAddOnsEdit.Items.Count; ++index)
                if (cmbAddOnsEdit.Items[index].CheckState == CheckState.Checked)
                    checkedAddOns.Add(cmbAddOnsEdit.Items[index].Value.ToString().ToLower());

            //remove all currently listed add-ons ...
            cmbAddOnsEdit.Items.Clear(); //... from combo-box in view-ribbon ...
            for (int index = dgvRun.Columns.Count - 1; index >= 0; --index) //... and all respective columns from run-(system-)list
                if (dgvRun.Columns[index].Name.StartsWith(_colAddOnPrefix))
                    dgvRun.Columns.RemoveAt(index);

            //initialise info about add-ons (which add-on-system they provide, and which country-systems they support) as required
            if (_addOnInfo == null)
                _addOnInfo = new Dictionary<string, List<AddOnSystemInfo>>();

            //loop over all (now) available add-ons
            foreach (Country addOn in CountryAdministrator.GetAddOns())
            {
                bool isSelected = checkedAddOns.Contains(addOn._shortName.ToLower());
                cmbAddOnsEdit.Items.Add(addOn._shortName, isSelected);

                //if the add-on is selected, display a column (with check-boxes) for this add-on in the run-(system-)list
                if (isSelected)
                {
                    if (!_addOnInfo.ContainsKey(addOn._shortName.ToLower()))
                        _addOnInfo.Add(addOn._shortName.ToLower(), AddOnInfoHelper.GetAddOnSystemInfo(addOn._shortName)); //assess and store the systems provided by the add-on and which country-systems they support

                    dgvRun.Columns.Add(new DataGridViewCheckBoxColumn()
                    {
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                        HeaderText = addOn._shortName,
                        Name = _colAddOnPrefix + Guid.NewGuid().ToString(),
                        Tag = _addOnInfo[addOn._shortName.ToLower()]
                    });
                }
            }
        }

        void AddToSystemList(string countryShortName)
        {
            Dictionary<int, List<string>> rowDatasets = new Dictionary<int, List<string>>(); //used at the very end of this function for sorting datasets in the combos
            DataConfigFacade dataConfigFacade = CountryAdministrator.GetDataConfigFacade(countryShortName); //always demand data configuration from country-admin to get an update version
            foreach (string systemName in dataConfigFacade.GetSystemsNamesDistinctAndOrdered(countryShortName)) //get all systems of the country
            {
                //check whether system fulfills system-filter criteria (if any)
                if (txtFilterSystems.EditValue != null && txtFilterSystems.EditValue.ToString() != string.Empty &&
                        !EM_Helpers.DoesValueMatchPattern(txtFilterSystems.EditValue.ToString(), systemName, false, false, _isRegularExpressionChecked))
                    continue;

                if (EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(_hideHiddenSystems) &&
                    TreeListManagement.ViewKeeper.IsHiddenSystem(countryShortName, systemName)) continue;

                int indexSystemRow = -1;
                string bestMatchName = string.Empty;

                System.Drawing.Color defaultCheckBoxBackColor = colRun.CellTemplate.Style.BackColor; //to be used below

                List<DataConfig.DBSystemConfigRow> dbSystemConfigRows = dataConfigFacade.GetDBSystemConfigRowsBySystem(systemName); //get the datasets, that can be used for running the system ...

                foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in dbSystemConfigRows) //display the system in the grid, if there are any datasets which fulfill data-filter criteria (if any)
                {
                    string dataBaseName = dbSystemConfigRow.DataBaseRow.Name;

                    //check whether dataset fulfills data-filter criteria (if any)
                    if (txtFilterDatasets.EditValue != null && txtFilterDatasets.EditValue.ToString() != string.Empty &&
                        !EM_Helpers.DoesValueMatchPattern(txtFilterDatasets.EditValue.ToString(), dataBaseName, false, false, _isRegularExpressionChecked))
                        continue;

                    //if option 'Best Match Only' is set: only add dataset to combo if it is a best match, and do not list system if there is no best match at all (indexSystemRow stays -1)
                    if (_isBestMatchOnlyChecked && dbSystemConfigRow.BestMatch != DefPar.Value.YES)
                        continue;

                    if (indexSystemRow == -1 || _doNotPoolSystemsDatasets) //system not yet displayed or each system-dataset-combination is displayed separately
                    {
                        //set the cell style for newly to create checkboxes in the add-on-columns to background = gray or white, depending on availability of add-on
                        foreach (DataGridViewColumn column in dgvRun.Columns)
                        {
                            if (column.Name.StartsWith(_colAddOnPrefix))
                            {
                                bool addOnAvailable = AddOnInfoHelper.IsSystemSupported(systemName, column.Tag as List<AddOnSystemInfo>);
                                column.CellTemplate.Style.BackColor = addOnAvailable ? defaultCheckBoxBackColor : System.Drawing.Color.LightGray;
                            }
                        }

                        //add a row for the system
                        object[] rowValues = new object[dgvRun.Columns.Count];
                        rowValues[0] = false; //run-checkbox
                        rowValues[1] = countryShortName;
                        rowValues[2] = systemName;
                        rowValues[3] = string.Empty; //combo-box with datasets
                        rowValues[4] = string.Empty; //first-hh
                        rowValues[5] = string.Empty; //last-hh
                        for (int indexColumn = 6; indexColumn < dgvRun.Columns.Count; ++indexColumn)
                            rowValues[indexColumn] = false; //separate-run-checkbox and add-on-checkboxes
                        indexSystemRow = dgvRun.Rows.Add(rowValues);

                        for (int indexColumn = 6; indexColumn < dgvRun.Columns.Count; ++indexColumn)
                        {
                            //set check-boxes for add-ons to read-only if add-on is not available for the system
                            //this cannot be done together with coloring (see above) as coloring needs to be prepared before creating the system's row, while setting read-only can only be done once the row exists
                            DataGridViewColumn column = dgvRun.Columns[indexColumn];
                            if (column.Name.StartsWith(_colAddOnPrefix))
                            {
                                bool addOnAvailable = AddOnInfoHelper.IsSystemSupported(systemName, column.Tag as List<AddOnSystemInfo>);
                                (dgvRun.Rows[indexSystemRow].Cells[column.Name] as DataGridViewCheckBoxCell).ReadOnly = !addOnAvailable;
                            }
                        }
                    }

                    if (dbSystemConfigRow.BestMatch == DefPar.Value.YES)
                    {
                        dataBaseName += infoBestMatch;
                        bestMatchName = dataBaseName; //remember to be selected below
                    }
                    if (dbSystemConfigRow.DataBaseRow.Private == DefPar.Value.YES)
                        dataBaseName += infoPrivate;

                    if (!rowDatasets.ContainsKey(indexSystemRow)) rowDatasets.Add(indexSystemRow, new List<string>());
                    rowDatasets[indexSystemRow].Add(dataBaseName);

                    if (_doNotPoolSystemsDatasets) //if system-dataset-combinations are displayed separately the dataset can be set here, otherwise the best match must be found (to be set below)
                    {
                        DataGridViewComboBoxCell cmbDatasets = dgvRun.Rows[indexSystemRow].Cells[colDataset.Name] as DataGridViewComboBoxCell;
                        if (!cmbDatasets.Items.Contains(dataBaseName)) cmbDatasets.Items.Add(dataBaseName);
                        dgvRun.Rows[indexSystemRow].Cells[colDataset.Name].Value = dataBaseName;
                        dgvRun.Rows[indexSystemRow].Tag = dbSystemConfigRows; //needs to be set for each system-dataset-combination (therefore within the loop)
                    }
                }

                if (indexSystemRow == -1 || //no dataset to run this system, therefore system is not displayed
                    _doNotPoolSystemsDatasets) //if system-dataset-combinations are displayed separately the database was already set in the loop (tag was set above too)
                    continue;

                dgvRun.Rows[indexSystemRow].Tag = dbSystemConfigRows;
            }
            PutDataInCombos(rowDatasets); //puts the datasets collected in 'rowDatasets' ordered (see below for order-criteria) into the combo-boxes (to avoid jeopardising the running version, this is implemented in a possibly undisturbing way)
        }

        void PutDataInCombos(Dictionary<int, List<string>> rowDatasets)
        {
            if (_doNotPoolSystemsDatasets) return;
            foreach (var rd in rowDatasets)
            {
                int iRow = rd.Key; List<string> dataNames = rd.Value;

                string bestMatch = string.Empty;
                for (int i = 0; i < dataNames.Count; ++i) if (dataNames[i].EndsWith(infoBestMatch)) { bestMatch = dataNames[i]; dataNames.RemoveAt(i); break; }
                List<string> sortedRegular = SeparateData(ref dataNames, false); // collects regular datasets, e.g. AT_2015_a1, SCT_2016, ... and removes them from dataNames-list
                List<string> sortedHHOT = SeparateData(ref dataNames, true); // collects HHOT datasets (e.g. AT_2015_hhot) and removes them from dataNames-list

                DataGridViewComboBoxCell cmbDatasets = dgvRun.Rows[iRow].Cells[colDataset.Name] as DataGridViewComboBoxCell;
                if (bestMatch != string.Empty) cmbDatasets.Items.Add(bestMatch); // first add best-matching dataset
                foreach (string s in sortedRegular) cmbDatasets.Items.Add(s); // then regular datasets
                foreach (string s in dataNames) cmbDatasets.Items.Add(s); // then any "unspecified" dataset (e.g. training_data)
                foreach (string s in sortedHHOT) cmbDatasets.Items.Add(s); // then HHOT

                if (cmbDatasets.Items.Count > 0) dgvRun.Rows[iRow].Cells[colDataset.Name].Value = cmbDatasets.Items[0].ToString(); // select first dataset (i.e. usually best-matching)
            }
        }

        List<string> SeparateData(ref List<string> dataNames, bool hhot)
        {
            List<string> sortedData = new List<string>();
            List<int> iRemove = new List<int>();

            foreach (string dataName in dataNames)
            {

                if (hhot) { if (!dataName.ToLower().Contains("hhot")) continue; }
                else
                {
                    string dataNameWoP = dataName.EndsWith(infoPrivate) ? dataNameWoP = dataName.Substring(0, dataName.Length - infoPrivate.Length) : dataName;
                    if (!EM_Helpers.DoesValueMatchPattern("??_????_??", dataNameWoP) &&
                        !EM_Helpers.DoesValueMatchPattern("???_????_??", dataNameWoP)) continue;
                }
                iRemove.Add(dataNames.IndexOf(dataName));
                sortedData.Add(dataName);
            }
            for (int r = iRemove.Count - 1; r >= 0; --r) dataNames.RemoveAt(iRemove[r]);
            return sortedData;
        }

        DataGridViewColumn GetGridColumnByName(string columnName)
        {
            foreach (DataGridViewColumn column in dgvRun.Columns)
                if (column.Name.ToLower() == columnName.ToLower())
                    return column;
            return null;
        }

        void RunMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StorePolicySwitchSettings(); //... on/off-settings of policy switches if displayed
            e.Cancel = true; //don't close just hide
            Hide();
        }

        void gbiCountries_Gallery_ItemClick(object sender, GalleryItemClickEventArgs e)
        {
            //e.Item.Checked = !e.Item.Checked;
            //the above code only works if the gallery item is visible (i.e. not hidden because there are too many countries)
            //therefore do the checking in UpdateForm, which is pedestrian but works

            //put the current check states in a list and change the one that caused the call-back of this function
            List<KeyValuePair<GalleryItem, bool>> itemCheckStates = new List<KeyValuePair<GalleryItem, bool>>();
            foreach (GalleryItem galleryItem in _gigCountries.Items)
            {
                bool setChecked = (e.Item.Caption != galleryItem.Caption) ? galleryItem.Checked : !e.Item.Checked;
                itemCheckStates.Add(new KeyValuePair<GalleryItem, bool>(galleryItem, setChecked));
            }
            UpdateForm(_countryShortName, itemCheckStates);
            DrawExtensionCheckboxes();

            if (e.Item.Checked)
                CheckForSystemsWithoutDatasets(e.Item.Caption);
        }

        void UpdateSettings(bool isChecked, string settingText)
        {//store user's choice of view in the local settings
            try
            {
                if (isChecked && !EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(settingText))
                    EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().AddToRunFormSettings(settingText);
                if (!isChecked && EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(settingText))
                    EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().RemoveFromRunFormSettings(settingText);
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void checkEditSelectedHH_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            bool show = EM_Helpers.SaveConvertToBoolean(e.NewValue);
            colFirstHH.Visible = show;
            colLastHH.Visible = show;
            UpdateSettings(show, _showSelectedHHOptions);
        }

        void textEditFilterDatasets_Leave(object sender, EventArgs e)
        {
            UpdateForm(_countryShortName);
        }

        void textEditFilterSystems_Leave(object sender, EventArgs e)
        {
            UpdateForm(_countryShortName);
        }

        void checkEditBestMatchOnly_CheckedChanged(object sender, EventArgs e)
        {   //if UpdateForm would be called in chkBestMatchOnly_EditValueChanged the new value could be assessed directly in UpdateForm (via chkBestMatchOnly.EditValue)
            //however, this callback only fires if the check box looses focus and not on click, therefore use variable _isBestMatchOnlyChecked
            CheckEdit checkEdit = sender as CheckEdit;
            if (checkEdit == null)
                return;
            //for a boolean the value on changing is the opposite of the current value (the outcommented part does for whatever reason not work with filters (dataset, system))
            _isBestMatchOnlyChecked = !_isBestMatchOnlyChecked; //EM_Helpers.SaveConvertToBoolean(checkEdit.OldEditValue);
            UpdateForm(_countryShortName);
        }
        void checkEditHideHiddenSystems_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            UpdateSettings(EM_Helpers.SaveConvertToBoolean(e.NewValue), _hideHiddenSystems);
            UpdateForm(_countryShortName);
        }

        void checkEditWarnAboutUselessGroups_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            UpdateSettings(!(sender as CheckEdit).Checked, _warnAboutUselessGroups);
        }

        void checkEditRegularExpression_CheckedChanged(object sender, EventArgs e)
        {   //if UpdateForm would be called in chkBestMatchOnly_EditValueChanged the new value could be assessed directly in UpdateForm (via chkBestMatchOnly.EditValue)
            //however, this callback only fires if the check box looses focus and not on click, therefore use variable _isRegularExpressionChecked
            CheckEdit checkEdit = sender as CheckEdit;
            if (checkEdit == null)
                return;
            //for a boolean the value on changing is the opposite of the current value (the outcommented part does for whatever reason not work with filters (dataset, system))
            _isRegularExpressionChecked = !_isRegularExpressionChecked; //EM_Helpers.SaveConvertToBoolean(checkEdit.OldEditValue);
            UpdateForm(_countryShortName);
        }

        void checkEditDoNotPool_CheckedChanged(object sender, EventArgs e)
        {   //see checkEditBestMatchOnly_CheckedChanged
            CheckEdit checkEdit = sender as CheckEdit;
            if (checkEdit == null)
                return;
            _doNotPoolSystemsDatasets = !_doNotPoolSystemsDatasets;
            UpdateForm(_countryShortName);
        }

        void checkEditRunEM2_EditValueChanged(object sender, EventArgs e)
        {
            _runEM2 = !_runEM2;
            AccountForEM2EM3Differences();
        }

        void checkEditDoNotStop_EditValueChanged(object sender, EventArgs e)
        {
            UpdateSettings((sender as CheckEdit).Checked, _doNotStopOnNonCriticalErrors);
        }

        void checkEditAddDate_EditValueChanged(object sender, EventArgs e)
        {
            UpdateSettings((sender as CheckEdit).Checked, _addDateToOuputFilename);
        }

        void checkEditLogRuntime_EditValueChanged(object sender, EventArgs e)
        {
            UpdateSettings((sender as CheckEdit).Checked, _logRuntimeInDetail);
        }

        void checkEditPublicOnly_EditValueChanged(object sender, EventArgs e)
        {
            //UpdateSettings((sender as CheckEdit).Checked, _runPublicOnly);
        }

        internal bool runPublicOnly()
        {
            return chkRunPublicOnly.EditValue != null && bool.TryParse(chkRunPublicOnly.EditValue.ToString(), out bool b) && b;
        }

        void checkEditCloseAfter_EditValueChanged(object sender, EventArgs e)
        {
            UpdateSettings((sender as CheckEdit).Checked, _closeAfterRun);
        }

        void btnSelectAllOrNo_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (GalleryItem galleryItem in _gigCountries.Items) //function is called by btnSelectAll (->true) as well as by btnSelectNo (->false)
                galleryItem.Checked = ((e.Item as BarButtonItem).Id == btnSelectAll.Id) ? true : false;
            UpdateForm(_countryShortName);
        }

        void btnSelectOutputPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the output path";
            folderBrowserDialog.SelectedPath = txtOutputPath.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtOutputPath.Text = folderBrowserDialog.SelectedPath;
        }

        void comboBoxAddOns_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {
            UpdateForm(_countryShortName);
        }

        internal RunMainForm()
        {
            InitializeComponent();

            InitForm(); //fill gallery with country-flags
        }

        internal void Show(string countryShortName)
        {
            AccountForEM2EM3Differences();

            if (_addOnInfo != null)
                _addOnInfo.Clear(); //enforce assessing add-on-availability, as it may have changed since the last opening of the dialog

            UpdateForm(countryShortName);
            DrawExtensionCheckboxes();

            _countryShortName = countryShortName; //only store name of country after updating, as this allows assessing whether it's the first call of run from this country
            Show();
            Activate();

            // avoid that the window gets bigger than the screen
            WindowState = FormWindowState.Maximized; int maxWidth = Width - 10; int maxHeight = Height - 10;
            WindowState = FormWindowState.Normal; Width = Math.Min(maxWidth, Width); Height = Math.Min(maxHeight, Height);

            CheckForSystemsWithoutDatasets();
        }

        private void AccountForEM2EM3Differences()
        {
            chkDoNotStopOnNonCriticalErrors.Visibility = _runEM2 ? BarItemVisibility.Always : BarItemVisibility.Never;
            chkLogRuntimeInDetail.Visibility = _runEM2 ? BarItemVisibility.Always : BarItemVisibility.Never;
            Image runButtonImage = _runEM2 ? global::EM_UI.Properties.Resources.RunOldExe : global::EM_UI.Properties.Resources.green_right_arrow;
            btnRun.Glyph = runButtonImage; btnRun.LargeGlyph = runButtonImage;
            btnRun.Caption = _runEM2 ? "Run EM2" : "Run";
        }

        void btnSelectAllSystems_ItemClick(object sender, ItemClickEventArgs e) { CheckSystemsForRun(true); }
        void btnSelectNoSystem_ItemClick(object sender, ItemClickEventArgs e) { CheckSystemsForRun(false); }
        void CheckSystemsForRun(bool check)
        {
            foreach (DataGridViewRow systemRow in dgvRun.Rows)
                systemRow.Cells[colRun.Name].Value = check;
        }

        void btnSelectAllAddOns_ItemClick(object sender, ItemClickEventArgs e) { CheckAddOnsForRun(true); }
        void btnSelectNoAddOn_ItemClick(object sender, ItemClickEventArgs e) { CheckAddOnsForRun(false); }
        void CheckAddOnsForRun(bool check)
        {
            foreach (DataGridViewRow systemRow in dgvRun.Rows)
            {
                foreach (DataGridViewColumn column in dgvRun.Columns)
                {
                    string systemName = systemRow.Cells[colSystem.Name].Value.ToString();
                    if (column.Name.StartsWith(_colAddOnPrefix) && AddOnInfoHelper.IsSystemSupported(systemName, column.Tag as List<AddOnSystemInfo>))
                        systemRow.Cells[column.Name].Value = check;
                }
            }
        }

        // gather all extensions: the global ones and those of the activated countries
        internal List<Tuple<string, string, string>> GetAllExtensions() // item1: ID, item2: long-name, item3: short-name (formally name-pattern)
        {
            List<Tuple<string, string, string>> allExtensions = new List<Tuple<string, string, string>>();
            foreach (var e in ExtensionAndGroupManager.GetGlobalExtensions())
                allExtensions.Add(new Tuple<string, string, string>(e.ID, e.Name, e.ShortName));
            foreach (GalleryItem galleryItem in _gigCountries.Items)
            {
                if (!galleryItem.Checked) continue;
                foreach (var e in ExtensionAndGroupManager.GetLocalExtensions(galleryItem.Caption))
                {
                    // it is possible that two (or more) countries have a country specific extension with the same id, if they are copies from the same origin
                    if (!(from ae in allExtensions where ae.Item1 == e.ID select ae).Any())
                        allExtensions.Add(new Tuple<string, string, string>(e.ID, e.Name, e.ShortName));
                }
            }
            return allExtensions;
        }

        void DrawExtensionCheckboxes()
        {
            try
            {
                //remove all existing checkboxes for displaying extensions, but take care to not remove the Restore Defaults and Auto Rename controls
                for (int index = rpgExtensions.ItemLinks.Count - 1; index >= 3; --index)
                    rpgExtensions.ItemLinks.RemoveAt(index);

                // run over all extensions: the global ones and those of the activated countries
                foreach (Tuple<string, string, string> e in GetAllExtensions())
                {
                    string extID = e.Item1, extLongName = e.Item2, extShortName = e.Item3;
                    //add a check box for each extension
                    BarEditItem filterCheckbox = new BarEditItem();
                    filterCheckbox.Caption = extShortName;
                    filterCheckbox.Hint = extLongName;
                    filterCheckbox.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far; //to show checkbox left of text (default is right)

                    RepositoryItemCheckEdit edit = new RepositoryItemCheckEdit();
                    edit.CheckedChanged += new EventHandler(checkEditExtension_CheckedChanged); //"listen" if user changes check
                    edit.Caption = extID + "|" + extLongName; //use this in replacement of the Tag:
                    //in checkEditExtension_CheckedChanged the sender is of type CheckEdit insted of RepositoryItemCheckEdit
                    //and the Tag is not available, however RepositoryItemCheckEdit.Caption corresponds to CheckEdit.Text
                    edit.Appearance.BackColor = System.Drawing.Color.Transparent;
                    edit.AppearanceFocused.BackColor = System.Drawing.Color.Transparent;
                    filterCheckbox.Edit = edit;
                    filterCheckbox.EditValue = (GetGridColumnByName(_colPolicySwitchPrefix + extID) != null); //check is set if column is visible

                    rpgExtensions.ItemLinks.Add(filterCheckbox);
                }
                if (rpgExtensions.ItemLinks.Count > 3)
                    rpgExtensions.ItemLinks[3].BeginGroup = true; //insert a separator after the Restore Defaults button (to separate the button from the checkboxes)
                toggleAutoRename.EditValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetPolicySwitchAutoRenameValue();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void checkEditExtension_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckEdit checkEdit = sender as CheckEdit;
                if (checkEdit == null)
                    return;

                //checkEdit.Text contains ID and name of switchable policy (ID|name)
                string[] split = checkEdit.Text.Split('|'); if (split.Count() != 2) return; string extID = split[0], extName = split[1];
                DataGridViewColumn column = GetGridColumnByName(_colPolicySwitchPrefix + extID);
                if (column != null) //column of this switchable policy is currently displayed - remove it
                    dgvRun.Columns.Remove(column);
                else //policy switch column is currently not displayed - add it
                {
                    DataGridViewButtonColumn colPolicySwitch = new DataGridViewButtonColumn();
                    colPolicySwitch.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
                    colPolicySwitch.HeaderText = extName;
                    colPolicySwitch.Name = _colPolicySwitchPrefix + extID;
                    int index = dgvRun.Columns.Add(colPolicySwitch);
                    dgvRun.Columns[index].Tag = extID;

                    //set value of switch for each system, more precisely system-dataset-combination (to on, off or blank button if set to n/a)
                    DisplayExtensionSettings(index);
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal bool IsSwitchColumnDisplayed(string switchablePolicyName)
        {
            foreach (DataGridViewColumn col in dgvRun.Columns)
            {
                if (!col.Name.StartsWith(_colPolicySwitchPrefix)) continue;
                GlobLocExtensionRow extension = ExtensionAndGroupManager.GetGlobalExtension(col.Name.Substring(_colPolicySwitchPrefix.Length));
                if (extension == null) continue;
                if (extension.ShortName.ToLower() == switchablePolicyName.ToLower()) return true;
            }
            return false;
        }

        void dgvRun_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || !dgvRun.Columns[e.ColumnIndex].Name.StartsWith(_colPolicySwitchPrefix)) return;
            object tag = dgvRun.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag; if (tag == null) return;
            string currentValue = dgvRun.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag.ToString();
            if (string.IsNullOrEmpty(currentValue) || currentValue == DefPar.Value.NA) return;

            DataConfig.DBSystemConfigRow dbSys = GetSelectedDBSystemCombination(dgvRun.Rows[e.RowIndex]);
            string extensionId = dgvRun.Columns[e.ColumnIndex].Tag.ToString();

            ContextMenuStrip extensionSwitchMenu = new ContextMenuStrip() { ShowCheckMargin = false, ShowImageMargin = false };
            extensionSwitchMenu.Items.Add(DefPar.Value.ON, null, (iSender, iArgs) => { ItemCallBack(DefPar.Value.ON); });
            extensionSwitchMenu.Items.Add(DefPar.Value.OFF, null, (iSender, iArgs) => { ItemCallBack(DefPar.Value.OFF); });
            extensionSwitchMenu.Items.Add(DefPar.Value.EXTENSION_ALL, null, (iSender, iArgs) => { ItemCallBack(DefPar.Value.EXTENSION_ALL); });
            extensionSwitchMenu.Show(new Point(MousePosition.X, MousePosition.Y));

            void ItemCallBack(string selectedValue) { ExtensionSwitchMenu_ItemClick(selectedValue, currentValue, extensionId, dbSys, dgvRun.Rows[e.RowIndex].Cells[e.ColumnIndex]); }
        }

        void ExtensionSwitchMenu_ItemClick(string selectedValue, string currentValue, string extensionId,
                                           DataConfig.DBSystemConfigRow dbSys, DataGridViewCell cell)
        {
            if (selectedValue == currentValue) return;
            try
            {
                if (selectedValue == DefPar.Value.EXTENSION_ALL) { cell.Value = selectedValue; return; }
                // store the new value of the switch in the user settings
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPolicySwitchValue(extensionId, dbSys.SystemID, dbSys.DataBaseID, selectedValue);
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
                DisplayExtensionSettings(cell.ColumnIndex, cell.RowIndex); // the function updates the buttons, taking care of what's stored in the user settings
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        void dgvRun_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == colDataset.Index) //policy switches may be dataset specific, therefore update if the dataset changes
                DisplayExtensionSettings(-1, e.RowIndex);
        }

        internal KeyValuePair<string, string> GetPolicySwitchValueAndDisplayValue(DataConfig.DBSystemConfigRow dbSystemConfigRow, string switchablePolicyID)
        {//assess which value to display for a policy switch, more precisely for a combintaion: switchable policy - system - dataset
            try
            {
                //first get the default value, as set in the dialog in the country settings ...
                string defaultValue = ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbSystemConfigRow, switchablePolicyID);
                //... then check if a user-setting exists for this policy switch
                string userSpecificValue = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetPolicySwitchValue(switchablePolicyID, dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID);
                //use the user's setting, unless the default value is set to n/a
                string value = (defaultValue != DefPar.Value.NA && userSpecificValue != string.Empty) ? userSpecificValue : defaultValue;
                string displayValue = value;
                if (displayValue == defaultValue)
                    displayValue += _policySwitchDefaultValueIndicator; //indicate if the value corresponds to the default
                if (displayValue.Contains(DefPar.Value.NA))
                    displayValue = string.Empty; //show blank button instead of button with caption n/a
                return new KeyValuePair<string, string>(value, displayValue);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            }
        }

        void StorePolicySwitchSettings() //upon UpdateForm: if any policy switch columns are displayed store the current caption of the button in the user settings
        {
            try
            {
                //                bool storePolicySwitchSettings = false;
                foreach (DataGridViewColumn column in dgvRun.Columns)
                {
                    if (column.Name.StartsWith(_colPolicySwitchPrefix) && column.Tag != null)
                    {
                        foreach (DataGridViewRow row in dgvRun.Rows)
                        {
                            DataConfig.DBSystemConfigRow dbSystemConfigRow = GetSelectedDBSystemCombination(row);
                            if (dbSystemConfigRow == null)
                                continue;
                            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPolicySwitchValue(column.Tag.ToString(), //switchable policy's ID
                                                    dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID, row.Cells[column.Name].Tag.ToString());
                            //                            storePolicySwitchSettings = true;
                        }
                    }
                }
                //                if (storePolicySwitchSettings)
                // store the switch settings regardless of whether any switches are visible
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void DisplayExtensionSettings(int colIndex = -1, int rowIndex = -1) //set the values of policy switch buttons with respect to system-dataset-combination and taking care of user-settings
        {
            try
            {
                foreach (DataGridViewColumn column in dgvRun.Columns)
                {
                    if (column.Name.StartsWith(_colPolicySwitchPrefix) && column.Tag != null && (colIndex == -1 || dgvRun.Columns.IndexOf(column) == colIndex))
                    {
                        foreach (DataGridViewRow row in dgvRun.Rows)
                        {
                            if (rowIndex != -1 && rowIndex != dgvRun.Rows.IndexOf(row)) continue;
                            //assess what the button should show, i.e. not just on/off/na but e.g. on (default) if on is the default or blank if set to n/a
                            DataConfig.DBSystemConfigRow dbSystemConfigRow = GetSelectedDBSystemCombination(row);
                            KeyValuePair<string, string> valueAndDisplayValue = GetPolicySwitchValueAndDisplayValue(dbSystemConfigRow, column.Tag.ToString());
                            row.Cells[column.Name].Value = valueAndDisplayValue.Value; //the button's caption
                            row.Cells[column.Name].Tag = valueAndDisplayValue.Key; //the real value of the switch (on/off/na)
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void btnRestorePolicySwitchDefaults_ItemClick(object sender, ItemClickEventArgs e)
        {
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().RemoveAllPolicySwitchValues();
            DisplayExtensionSettings();
        }

        void RunMainForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void CheckForSystemsWithoutDatasets(string countryShortName = "")
        {
            if (countryShortName == string.Empty)
                foreach (GalleryItem galleryItem in _gigCountries.Items)
                {
                    if (galleryItem.Checked == true)
                        countryShortName = galleryItem.Caption;
                }

            if (countryShortName == string.Empty)
                return;

            DataConfigFacade dataConfigFacade = CountryAdministrator.GetDataConfigFacade(countryShortName);
            CountryConfigFacade countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(countryShortName);

            string systemWithoutDataset = string.Empty;
            foreach (CountryConfig.SystemRow systemRow in countryConfigFacade.GetSystemRows())
                if (dataConfigFacade.GetDBSystemConfigRowsBySystem(systemRow.Name).Count == 0)
                    systemWithoutDataset += systemRow.Name + " ";

            if (systemWithoutDataset == string.Empty)
                return;

            Dialogs.OptionalWarningsManager.Show(Dialogs.OptionalWarningsManager._systemWithoutDatasetsWarning, false, string.Empty,
                                    Environment.NewLine + systemWithoutDataset + Environment.NewLine + Environment.NewLine +
                                    "Hint: You may want to assign datasets via the Configure Databases dialog.");
        }

        void customParallelRunsItem_EditValueChanged(object sender, System.EventArgs e)
        {
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRunsAuto = customParallelRunsItem.EditValue.ToString()[0] == 'D';
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRuns = int.Parse(customParallelRunsItem.EditValue.ToString().Substring(1));
            EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(true);
        }

        void toggleAutoRename_EditValueChanged(object sender, System.EventArgs e)
        {
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SetPolicySwitchAutoRenameValue((bool)toggleAutoRename.EditValue);
        }

        private void ctmMultiPolicySwitch_Opening(object sender, CancelEventArgs e)
        {
            KeyValuePair<int, int> info = GetHitInfo();
            if (info.Value > -1 && dgvRun.Columns[info.Value].Name.StartsWith(_colPolicySwitchPrefix))
            {
                _contextMenuColumn = info.Value;
            }
            else
            {
                _contextMenuColumn = -1;
                e.Cancel = true;
            }
        }

        KeyValuePair<int, int> GetHitInfo()
        {
            Point hit = dgvRun.PointToClient(MousePosition);
            DataGridView.HitTestInfo hitInfo = dgvRun.HitTest(hit.X, hit.Y);
            return new KeyValuePair<int, int>(hitInfo.RowIndex, hitInfo.ColumnIndex);
        }

        private void mniAllSystemsOn_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvRun.Rows)
            {
                //store the new value of the switch in the user settings
                string switchablePolicyID = dgvRun.Columns[_contextMenuColumn].Tag.ToString();
                DataConfig.DBSystemConfigRow dbSystemConfigRow = GetSelectedDBSystemCombination(row); //assess which concrete system-dataset combination is affected
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPolicySwitchValue(switchablePolicyID,
                                        dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID, DefPar.Value.ON);
            }
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);

            //the function updates the buttons, taking care of what's stored in the user settings
            DisplayExtensionSettings(_contextMenuColumn);
        }

        private void mniAllSystemsOff_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvRun.Rows)
            {
                //store the new value of the switch in the user settings
                string switchablePolicyID = dgvRun.Columns[_contextMenuColumn].Tag.ToString();
                DataConfig.DBSystemConfigRow dbSystemConfigRow = GetSelectedDBSystemCombination(row); //assess which concrete system-dataset combination is affected
                EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPolicySwitchValue(switchablePolicyID,
                                        dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID, DefPar.Value.OFF);
            }
            EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);

            //the function updates the buttons, taking care of what's stored in the user settings
            DisplayExtensionSettings(_contextMenuColumn);
        }

        private void mniAllSystemsDefault_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvRun.Rows)
            {
                //store the new value of the switch in the user settings
                string switchablePolicyID = dgvRun.Columns[_contextMenuColumn].Tag.ToString();
                DataConfig.DBSystemConfigRow dbSystemConfigRow = GetSelectedDBSystemCombination(row); //assess which concrete system-dataset combination is affected
                EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPolicySwitchValue(switchablePolicyID,
                    dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID, ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbSystemConfigRow, switchablePolicyID));
            }
            EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);

            //the function updates the buttons, taking care of what's stored in the user settings
            DisplayExtensionSettings(_contextMenuColumn);
        }

        private void mniAllSystemsAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvRun.Rows)
                if (row.Cells[_contextMenuColumn].Value != null && !string.IsNullOrEmpty(row.Cells[_contextMenuColumn].Value.ToString()))
                    row.Cells[_contextMenuColumn].Value = DefPar.Value.EXTENSION_ALL;
        }

        private void RunMainForm_Shown(object sender, EventArgs e)
        {
            this.dgvRun.Size = new System.Drawing.Size(this.ClientSize.Width - this.dgvRun.Left * 2, this.ClientSize.Height - (this.dgvRun.Top + this.btnSelectOutputPath.Height + 20));
            this.label1.Location = new Point(this.dgvRun.Left, this.dgvRun.Top + this.dgvRun.Height + 10 + ((this.btnSelectOutputPath.Height - this.label1.Height) / 2));
            this.txtOutputPath.Location = new Point(this.label1.Left + this.label1.Width + 10, this.dgvRun.Top + this.dgvRun.Height + 10 + ((this.btnSelectOutputPath.Height - this.txtOutputPath.Height) / 2));
            this.btnSelectOutputPath.Location = new Point(this.dgvRun.Left + this.dgvRun.Width - this.btnSelectOutputPath.Width, this.dgvRun.Top + this.dgvRun.Height + 10);
            this.txtOutputPath.Width = this.btnSelectOutputPath.Left - (this.txtOutputPath.Left + 10);
        }

        private void dgvRun_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string country = dgvRun.Rows[e.RowIndex].Cells[colCountry.Name].Value.ToString();
            string system = dgvRun.Rows[e.RowIndex].Cells[colSystem.Name].Value.ToString();
            CountryConfigFacade countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(country); //always demand data configuration from country-admin to get an update version
            if (countryConfigFacade.GetSystemRowByName(system).Private == "yes") e.CellStyle.ForeColor = Color.Red;
        }

        internal bool GetNumberOfHHToRun(out string sHH)
        {
            sHH = string.Empty;
            if (txtNumberOfHH.EditValue == null || txtNumberOfHH.EditValue.ToString() == string.Empty) return true;
            int nHH;
            if (!int.TryParse(txtNumberOfHH.EditValue.ToString(), out nHH) || nHH <= 0) return false;
            sHH = txtNumberOfHH.EditValue.ToString(); return true;
        }

        internal void UpdateOutputIfDefault(string oldSettingsValue, string newSettingsValue)
        {
            // if the output folder has the old default value, update it
            if (this.txtOutputPath.Text.Equals(oldSettingsValue)) this.txtOutputPath.Text = newSettingsValue;
        }
    }
}