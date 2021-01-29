using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.ImportExport;
using EM_UI.Tools;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Merging
{
    internal partial class MergeChoicesForm : Form
    {
        bool _getVariablesChoices = false;
        bool _isAddOn = false;
        string _pathRemoteVersion, _nameRemoteVersion, _pathParentVersion, _nameParentVersion;

        void MergeChoicesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal MergeChoicesForm(bool getVariablesChoices, bool isAddOn)
        {
            _getVariablesChoices = getVariablesChoices;
            _isAddOn = isAddOn;
            InitializeComponent();
        }

        internal void GetInfo(out string pathRemoteVersion, out string nameRemoteVersion,
                              out string pathParentVersion, out string nameParentVersion,
                              out bool useLocalAsParent, out bool useRemoteAsParent)
        {
            pathRemoteVersion = _pathRemoteVersion;
            nameRemoteVersion = _nameRemoteVersion;
            pathParentVersion = _pathParentVersion;
            nameParentVersion = _nameParentVersion;
            useLocalAsParent = chkUseLocal.Checked;
            useRemoteAsParent = chkUseRemote.Checked;
        }

        internal bool CheckCountryLabels() { return !chkSkipCountryLabelCheck.Checked; }
        internal void ShowSkipCheckCountryLabels() { chkSkipCountryLabelCheck.Visible = true; }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (txtRemoteVersion.Text == string.Empty) { UserInfoHandler.ShowError("Please select a Remote Version."); return; }
            if (!chkUseLocal.Checked && !chkUseRemote.Checked && txtParentVersion.Text == string.Empty) { UserInfoHandler.ShowError("Please select a Parent Version or check one of the 'Use ...' boxes."); return; }

            //Since users can also paste a path, the paths need to be validated and the remote and parent variables need to be populated
  
            if (!validateInsertedFields(txtRemoteVersion.Text, true)) { return; }
            if (txtParentVersion.Text != string.Empty && !validateInsertedFields(txtParentVersion.Text, false)) { return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        Boolean validateInsertedFields(string insertedPath, bool remote)
        {
            string countryShortName = string.Empty;
            string path;
            string fileName;
            string errorMessage;

            bool oldStyle = CountryAdministrator.ConsiderOldAddOnFileStructure(_isAddOn);

            //Variables
            if (_getVariablesChoices || oldStyle)
            {
                if (Path.GetExtension(insertedPath) != ".xml")
                {
                    UserInfoHandler.ShowError("File extension should be .xml.");
                    return false;
                }
                if (_getVariablesChoices)
                {
                    FileInfo fileInfo = new FileInfo(insertedPath);
                    fileName = fileInfo.Name;
                    if (!fileName.ToLower().Equals(EMPath.EM2_FILE_VARS.ToLower()) && !fileName.ToLower().Equals(EMPath.EM2_FILE_EXRATES.ToLower()) && !fileName.ToLower().Equals(EMPath.EM2_FILE_EXTENSIONS.ToLower()) && !fileName.ToLower().Equals(EMPath.EM2_FILE_HICP.ToLower()))
                    {
                        UserInfoHandler.ShowError("Only VARCONFIG.xml, HICPCONFIG.xml, EXCHANGERATESCONFIG.xml and SWITCHABLEPOLICYCONFIG.xml can be compared.");
                        return false;
                    }
                        
                }

                if (!File.Exists(insertedPath))
                {
                    UserInfoHandler.ShowError("File cannot be found: " + insertedPath + ".");
                    return false;
                }
                else
                {
                    if (_getVariablesChoices)
                    {
                        FileInfo fileInfo = new FileInfo(insertedPath);
                        path = fileInfo.Directory.FullName;
                        fileName = fileInfo.Name;
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(insertedPath);
                        fileName = fileInfo.Name.ToLower().EndsWith(".xml") ? fileInfo.Name.Substring(0, fileInfo.Name.Length - 4) : fileInfo.Name;

                        if (!Country.DoesFolderContainValidXMLFiles(fileName, out errorMessage, fileInfo.DirectoryName, true))
                        {
                            Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + fileInfo.DirectoryName + "'.");
                            return false;
                        }
                        path = fileInfo.DirectoryName;
                    }
                }
            }
            else //AddOn (new Style) or country files
            {

                //separate the country folder from the rest of the path, i.e. split '...\exportedToSomewhere\IT' into '...\exportedToSomewhere\' and 'IT'
                if (insertedPath.EndsWith("\\") || insertedPath.EndsWith("/"))
                    insertedPath = insertedPath.Substring(0, insertedPath.Length - 1);
                int indexCountryFolder = insertedPath.LastIndexOfAny(new char[] { '\\', '/' });

                if (indexCountryFolder < 0)
                    return false; //unlikely, therefore dispense with error message

                countryShortName = insertedPath.Substring(indexCountryFolder + 1);

                //name of folder is supposed to be name of country (this is accomplished like this by export and allows to load systems of other countries)
                if (!Country.DoesFolderContainValidXMLFiles(countryShortName, out errorMessage, insertedPath, _isAddOn))
                {
                    UserInfoHandler.ShowError("Failed to load country XML-files from '" + insertedPath + "'.");
                    return false;
                }

                path = insertedPath;
                fileName = countryShortName;

            }

            if (remote)
            {
                _pathRemoteVersion = path;
                _nameRemoteVersion = fileName;
            }
            else
            {
                _pathParentVersion = path;
                _nameParentVersion = fileName;
            }


            return true;
        }


        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LinkAdvanced_Click(object sender, EventArgs e)
        {
            if (this.groupAdvancedOptions.Visible)
            {
                if (UserInfoHandler.GetInfo("Advanced settings will not be applied if you close them. Would you like to close them?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (chkSkipCountryLabelCheck.Visible)
                    {
                        chkSkipCountryLabelCheck.Checked = true;
                    }
                    this.groupAdvancedOptions.Visible = false;
                    this.linkAdvancedOptions.Text = "Display advanced options";
                    txtParentVersion.Text = string.Empty;
                    chkUseRemote.Checked = false;
                    chkUseLocal.Checked = true;
                    
                    this.Size = new Size(539, 147);
                }
            }
            else
            {

                this.groupAdvancedOptions.Visible = true;
                this.linkAdvancedOptions.Text = "Hide advanced options.";
                this.Size = new Size(539, 267);
                txtParentVersion.Enabled = false;
                btnSelectParentVersion.Enabled = false;
            }
        }

        void chkUse_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkSender = sender as CheckBox;
            CheckBox chkAdapt = chkSender == chkUseLocal ? chkUseRemote : chkUseLocal;
            if (chkSender.Checked) { chkAdapt.Checked = false; txtParentVersion.Text = string.Empty; }
            txtParentVersion.Enabled = !chkUseRemote.Checked && !chkUseLocal.Checked;
            btnSelectParentVersion.Enabled = txtParentVersion.Enabled;
        }

        bool GetImportVariablesPath(out string path, out string fileName)
        {
            path = fileName = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml files (*.xml)|*.xml";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select variables configuration file to merge ...";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return false;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
            path = fileInfo.Directory.FullName;
            fileName = fileInfo.Name;
            return true;
        }

        void btnSelectVersion_Click(object sender, EventArgs e)
        {
            string shortName_varFileName, path, textToDisplay;
            if (_getVariablesChoices)
            {
                if (!GetImportVariablesPath(out path, out shortName_varFileName)) return;
                textToDisplay = EMPath.AddSlash(path) + shortName_varFileName;
            }
            else
            {
                bool oldStyle = CountryAdministrator.ConsiderOldAddOnFileStructure(_isAddOn);
                if (!(oldStyle ? ImportExportAdministrator.GetImportAddOnPath_OldStyle(out path, out shortName_varFileName, out string fileNameNew)
                           : ImportExportAdministrator.GetImportCountryPath(_isAddOn, out path, out shortName_varFileName)))
                    return;
                textToDisplay = !oldStyle ? path : EMPath.AddSlash(path) + Country.GetCountryXMLFileName(shortName_varFileName);
            }

            if (sender == btnSelectRemoteVersion)
            {
                txtRemoteVersion.Text = textToDisplay;
                _pathRemoteVersion = path;
                _nameRemoteVersion = shortName_varFileName;
            }
            else
            {
                txtParentVersion.Text = textToDisplay;
                _pathParentVersion = path;
                _nameParentVersion = shortName_varFileName;
            }
        }
    }
}
