using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCUsers : Form
    {
        internal class UserRightInfo
        {
            internal UserInfo userInfo = null;
            internal bool hasProjectAdminRight = false;
            internal VCAPI.VC_ACCESS_RIGHTS defaultUnitRight = VCAPI.VC_ACCESS_RIGHTS.NONE;
            internal Dictionary<long, VCAPI.VC_ACCESS_RIGHTS> unitRights = null;
        }
        class ExtUserRightInfo : UserRightInfo
        {
            internal Dictionary<long, VCAPI.VC_ACCESS_RIGHTS> oldUnitRights = null;
            internal bool added = false;
        }

        VCAPI _vcAPI = null;
        List<ExtUserRightInfo> _usersToRemove = new List<ExtUserRightInfo>();

        const string VC_ACCESS_UPLOAD = "Upload";
        const string VC_ACCESS_DOWNLOAD = "Download";
        const string VC_ACCESS_NONE = "None";

        internal VCUsers(VCAPI vcAPI)
        {
            InitializeComponent();

            _vcAPI = vcAPI;

            colRefineRights.Visible = false;
            colDefaultRight.ReadOnly = false;

            List<UserInfo> userInfos; List<bool> adminRights; List<VCAPI.VC_ACCESS_RIGHTS> defaultRights; 
            if (!_vcAPI.GetProjectUserRights(_vcAPI.vc_projectInfo.ProjectId, out userInfos, out adminRights, out defaultRights))
            {
                UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                return;
            }

            colDefaultRight.Items.Add(VC_ACCESS_DOWNLOAD); colDefaultRight.Items.Add(VC_ACCESS_UPLOAD);
            for (int i = 0; i < userInfos.Count; ++i)
            {
                ExtUserRightInfo tag = new ExtUserRightInfo { userInfo = userInfos.ElementAt(i) };
                tag.defaultUnitRight = defaultRights.ElementAt(i);
                tag.hasProjectAdminRight = adminRights.ElementAt(i);
                AddUserRow(tag);
            }
        }

        void AddUserRow(ExtUserRightInfo tag)
        {
            int index = dgvUsers.Rows.Add(string.Format(VCAPI.GetPrettyUserName(tag.userInfo)), tag.hasProjectAdminRight,
                tag.defaultUnitRight == VCAPI.VC_ACCESS_RIGHTS.UPLOAD ? VC_ACCESS_UPLOAD : (tag.defaultUnitRight == VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD ? VC_ACCESS_DOWNLOAD : VC_ACCESS_NONE));
            dgvUsers.Rows[index].Tag = tag;
        }

        void VCUsers_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        internal void GetChanges(out List<UserRightInfo> usersToAdd, out List<UserInfo> usersToRemove, out List<UserRightInfo> usersToChange)
        {
            usersToAdd = new List<UserRightInfo>(); usersToRemove = new List<UserInfo>(); usersToChange = new List<UserRightInfo>();
            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                ExtUserRightInfo tag = row.Tag as ExtUserRightInfo;

                //users to add
                if (tag.added) usersToAdd.Add(new UserRightInfo { userInfo = tag.userInfo,
                                                                  defaultUnitRight = DefaultUnitRightToInt(row),
                                                                  hasProjectAdminRight = EM_Helpers.SaveConvertToBoolean(row.Cells[colAdmin.Index].Value),
                                                                  unitRights = tag.unitRights });
                else
                {
                   //users with changed rights-settings (also includes users to add)
                    bool changed = tag.hasProjectAdminRight != EM_Helpers.SaveConvertToBoolean(row.Cells[colAdmin.Index].Value) || //... compare old and new admin-right ...
                                   DefaultUnitRightToInt(row) != tag.defaultUnitRight; //... and old and new default unit-right ...
                    
                    if (changed) usersToChange.Add(new UserRightInfo { userInfo = tag.userInfo,
                                                                       defaultUnitRight = DefaultUnitRightToInt(row),
                                                                       hasProjectAdminRight = EM_Helpers.SaveConvertToBoolean(row.Cells[colAdmin.Index].Value),
                                                                       unitRights = tag.unitRights });
                }
            }

            //users to delete
            foreach (ExtUserRightInfo userRightInfo in _usersToRemove) usersToRemove.Add(userRightInfo.userInfo);
        }

        VCAPI.VC_ACCESS_RIGHTS DefaultUnitRightToInt(DataGridViewRow row)
        {
            string cellVal = row.Cells[colDefaultRight.Index].Value.ToString();
            return cellVal == VC_ACCESS_UPLOAD ? VCAPI.VC_ACCESS_RIGHTS.UPLOAD : (cellVal == VC_ACCESS_DOWNLOAD ? VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD : VCAPI.VC_ACCESS_RIGHTS.NONE);
        }

        void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == colRefineRights.Index)
            {
                ExtUserRightInfo tag = dgvUsers.Rows[e.RowIndex].Tag as ExtUserRightInfo;
                Cursor = Cursors.WaitCursor;
                VCUserUnitRights vcUserUnitRights = new VCUserUnitRights(_vcAPI, tag.userInfo, DefaultUnitRightToInt(dgvUsers.Rows[e.RowIndex]), tag.unitRights);
                Cursor = Cursors.Default;
                if (vcUserUnitRights.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                tag.unitRights = vcUserUnitRights.GetChangedRights();
            }
        }

        void btnRemoveUsers_Click(object sender, EventArgs e)
        {
            List<ExtUserRightInfo> usersToRemove = new List<ExtUserRightInfo>(); string userNames = string.Empty;
            foreach (DataGridViewRow row in dgvUsers.SelectedRows)
            {
                ExtUserRightInfo extUserInfo = row.Tag as ExtUserRightInfo;
                VCAdministrator.AddUnitToMessage(ref userNames, extUserInfo.userInfo.username);
                if (!extUserInfo.added) usersToRemove.Add(extUserInfo); //user only needs to be removed via API if it wasn't added during this session of the dialog
            }

            if (UserInfoHandler.GetInfo("Are you sure you want to remove user(s) " + userNames + " from project?", MessageBoxButtons.OKCancel)
                                         == System.Windows.Forms.DialogResult.Cancel) return;
            _usersToRemove.AddRange(usersToRemove);
            foreach (DataGridViewRow row in dgvUsers.SelectedRows) dgvUsers.Rows.Remove(row);
        }

        void btnAddUsers_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            List<UserInfo> potUserInfos = new List<UserInfo>();
            List<string> potUserNames = new List<string>();
            List<long> inProUserIds = new List<long>();
            List<UserInfo> allUsers;

            foreach (DataGridViewRow row in dgvUsers.Rows) inProUserIds.Add((row.Tag as ExtUserRightInfo).userInfo.userId);
            
            if (!_vcAPI.GetAllUsers(out allUsers))
            {
                UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                return;
            }

            foreach (UserInfo userInfo in allUsers)
                if (!inProUserIds.Contains(userInfo.userId))
                {
                    potUserInfos.Add(userInfo);
                    potUserNames.Add(string.Format(VCAPI.GetPrettyUserName(userInfo)));
                }

            Cursor = Cursors.Default;
            VCUsersAdd vcUsersAdd = new VCUsersAdd(potUserNames);
            if (vcUsersAdd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            List<int> selectedIndices = vcUsersAdd.GetSelectedIndices();
            if (selectedIndices.Count == 0) return;

            foreach (int selIndex in selectedIndices)
            {
                UserInfo addUserInfo = potUserInfos[selIndex];
                ExtUserRightInfo tag = null;
                for (int remIndex = 0; remIndex < _usersToRemove.Count; ++remIndex) //check if user was in fact deleted during this section and is now re-added
                    if (_usersToRemove[remIndex].userInfo.userId == addUserInfo.userId) { tag = _usersToRemove[remIndex]; _usersToRemove.RemoveAt(remIndex); break; }
                if (tag == null) tag = new ExtUserRightInfo { userInfo = addUserInfo, added = true,
                                                              defaultUnitRight = VCAPI.VC_ACCESS_RIGHTS.DOWNLOAD};
                AddUserRow(tag);
            }
        }
    }
}
