using DevExpress.XtraTreeList;
using EM_Common;
using EM_Common_Win;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Validate;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.Tools
{
    class UserInfoHandler
    {
        readonly static int MAX_MESSAGE_LENGTH = 10000;

        internal static bool GetPolicyName(ref string policyName, string countryShortName, TreeList countryTreeList, string currentName = "")
        {
            for (;;)
            {
                if (UserInput.Get("Policy Name:", out policyName, policyName) == DialogResult.Cancel)
                    return false;
                if (PolicyValidation.IsValidPolicyName(ref policyName, countryShortName, countryTreeList, currentName))
                    break;
            }
            return true;
        }

        internal static Dictionary<string, string> GetSystemAssignement(EM_UI_MainForm copyCountryMainForm,
                                                                        EM_UI_MainForm pasteCountryMainForm,
                                                                        List<string> pasteCountryHiddenSystems)
        {
            Dictionary<string, string> pasteSystemsNamesAndIDs = new Dictionary<string, string>();
            Dictionary<string, string> copySystemNamesAndIDs = new Dictionary<string, string>();

            foreach (CountryConfig.SystemRow systemRow in CountryAdministrator.GetCountryConfigFacade(pasteCountryMainForm.GetCountryShortName()).GetSystemRows())
            {
                if (!pasteCountryHiddenSystems.Contains(systemRow.ID))
                    pasteSystemsNamesAndIDs.Add(systemRow.Name, systemRow.ID);
            }

            foreach (CountryConfig.SystemRow systemRow in CountryAdministrator.GetCountryConfigFacade(copyCountryMainForm.GetCountryShortName()).GetSystemRows())
                copySystemNamesAndIDs.Add(systemRow.Name, systemRow.ID);

            AssignSystemsForm assignSystemsForm = new AssignSystemsForm(pasteCountryMainForm.GetCountryShortName(), pasteSystemsNamesAndIDs,
                                                                        copyCountryMainForm.GetCountryShortName(), copySystemNamesAndIDs);
            if (assignSystemsForm.ShowDialog() == DialogResult.Cancel)
                return null;
            return assignSystemsForm.GetSystemAssignment();
        }

        private static string LimitMessageSize(string msg)
        {
            // avoid too large error messages as they are anyway not visible in the screen and could end up crashing the MessageBox!
            // but try to keep the last line, as this is usually the question asked to the user...
            if (msg.Length < MAX_MESSAGE_LENGTH) return msg;
            int lastLinePos = msg.LastIndexOf(Environment.NewLine);
            string lastLine = lastLinePos > -1 ? msg.Substring(lastLinePos) : "";
            msg = msg.Substring(0, MAX_MESSAGE_LENGTH) + Environment.NewLine + "..." + lastLine;
            return msg;
        }

        //the following functions centralise user information and can be extended at a later stage, if necessary
        internal static void ShowError(string errorMessage)
        {
            MessageBox.Show(EM_AppContext.Instance.GetTopMostWindow(), LimitMessageSize(errorMessage), $"{DefGeneral.BRAND_TITLE} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void ShowException(Exception exception, string additionalInfo = "", bool showInfoAfterException = true)
        {
            if (exception is System.Data.RowNotInTableException)
                return; //this happens when elements are deleted and some treelist updating still accesses the deleted elements (not sure it is clean to catch this here! - but it seems to do no harm)
                        //e.g. deleting a DefTU function: if other functions are expanded treeList_CustomNodeCellEdit calls FillTU_Editor which tries to access the deleted DefTU-function
            if (additionalInfo == string.Empty)
                ShowError(exception.Message);
            else
            {
                if (showInfoAfterException)
                    ShowError(exception.Message + Environment.NewLine + Environment.NewLine + additionalInfo);
                else
                    ShowError(additionalInfo + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }

        internal static void ShowInfo(string info)
        {
            MessageBox.Show(EM_AppContext.Instance.GetTopMostWindow(), LimitMessageSize(info), $"{DefGeneral.BRAND_TITLE} - Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static void ShowSuccess(string info)
        {
            new SuccessBox(info).ShowDialog();
        }

        internal static DialogResult GetInfo(string request, MessageBoxButtons buttons)
        {
            return MessageBox.Show(EM_AppContext.Instance.GetTopMostWindow(), LimitMessageSize(request), $"{DefGeneral.BRAND_TITLE} - Request", buttons);
        }

        static System.IO.StreamWriter _streamWriterIgnoredExceptions = null;
        static Dictionary<string, int> _recordedIgnoredException = null;
        internal static void RecordIgnoredException(string sender, Exception exception)
        {
            bool turnOn = false; if (!turnOn) return; //to be sure this is not executed in the released version

            if (!System.Diagnostics.Debugger.IsAttached)
                return;
            if (_streamWriterIgnoredExceptions == null)
                _streamWriterIgnoredExceptions = new System.IO.StreamWriter(EM_AppContext.FolderOutput + "Ignored Exceptions.txt");
            if (_recordedIgnoredException == null)
                _recordedIgnoredException = new Dictionary<string, int>();
            string key = sender+exception.Message;
            string count = "first occurance of  ";
            if (_recordedIgnoredException.ContainsKey(key))
            {
                if (_recordedIgnoredException[key] == 1)
                    count = "other occurances of ";
                ++_recordedIgnoredException[key];
                return;
            }
            else
                _recordedIgnoredException.Add(key, 1);
            _streamWriterIgnoredExceptions.WriteLine(string.Format("{0}: {1} - {2}", count, sender, exception.Message));
            _streamWriterIgnoredExceptions.Flush();
        }
    }
}

