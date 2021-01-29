using EM_UI.ExtensionAndGroupManagement;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    // note: dialog is constructed to be used for administrating Groups and country specific and global Extensions
    internal partial class AdminExtensionsOrGroupsForm : Form
    {
        private List<ExtensionOrGroup> orig;
        internal List<ExtensionOrGroup> added = null;
        internal List<ExtensionOrGroup> changed = null;
        internal List<string> deletedIds = null;
        private bool globalExt = false;

        internal AdminExtensionsOrGroupsForm(string header, List<ExtensionOrGroup> extensionsOrGroups, bool _globalExt = false)
        {
            InitializeComponent();

            orig = extensionsOrGroups; Text = header; globalExt = _globalExt;

            foreach (ExtensionOrGroup group in extensionsOrGroups)
                dgvExtensionsOrGroups.Rows.Add(group.id, group.look, group.shortName, group.name);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int r = dgvExtensionsOrGroups.Rows.Add(null, new LookDef(), string.Empty, string.Empty);
            dgvExtensionsOrGroups.CurrentCell = dgvExtensionsOrGroups.Rows[r].Cells[colShortName.Index];
            dgvExtensionsOrGroups.BeginEdit(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (globalExt && !AcceptRestrictions()) return;

            for (int r = dgvExtensionsOrGroups.SelectedRows.Count - 1; r >= 0; --r)
            {
                DataGridViewRow row = dgvExtensionsOrGroups.SelectedRows[r];
                if (row.Cells[colID.Index].Value == null) dgvExtensionsOrGroups.Rows.Remove(row); // delete if added in this session (not yet in XML)
                else row.Visible = false; // hide if needs to be removed from XML
            }
        }

        private bool AcceptRestrictions()
        {
            if (EM_AppContext.Instance.IsAnythingOpen(false)) { UserInfoHandler.ShowInfo("Deleting extensions needs all countries to be closed."); return false; }
            return UserInfoHandler.GetInfo("Please note that the necessary changes for countries using this/these extension(s) are accomplished once the country is next opened (automatically, but with a user request).",
                MessageBoxButtons.OKCancel) == DialogResult.OK;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // first: gather the rows to delete (hidden) and the group-names, to avoid groups with same name (from the user's point of view unique identifier)
            Dictionary<string, string> hidden = new Dictionary<string, string>();
            List<string> names = new List<string>(); List<string> shortNames = new List<string>();
            foreach (DataGridViewRow row in dgvExtensionsOrGroups.Rows)
            {
                bool hasName = GetName(row, false, true, out string name);
                bool hasShortName = GetName(row, true, true, out string shortName);
                if (row.Visible)
                {
                    if (!hasName && !hasShortName) continue;
                    if (!hasShortName) { UserInfoHandler.ShowError($"Please indicate a short name for '{name}'"); return; }
                    if (!hasName) { UserInfoHandler.ShowError($"Please indicate a name for '{shortName}'"); return; }
                    if (shortNames.Contains(shortName)) { UserInfoHandler.ShowError($"Not unique short name '{shortName}' (rows {row.Index + 1} and {shortNames.IndexOf(shortName) + 1})."); return; }
                    if (names.Contains(name)) { UserInfoHandler.ShowError($"Not unique name '{name}' (rows {row.Index + 1} and {names.IndexOf(name) + 1})."); return; }
                    names.Add(name); shortNames.Add(shortName);
                }
                else if (hasName) hidden.Add(name, row.Cells[colID.Index].Value.ToString());
            }

            // then: find out what needs to be added/changed/deleted
            added = new List<ExtensionOrGroup>(); changed = new List<ExtensionOrGroup>(); deletedIds = new List<string>();
            const string READDED = "DELETED_AND_READDED";
            foreach (DataGridViewRow row in dgvExtensionsOrGroups.Rows)
            {
                string groupName; if (!GetName(row, false, false, out groupName)) continue; // ignore if empty name-column
                string groupShortName = row.Cells[colShortName.Index].Value.ToString();
                if (row.Cells[colID.Index].Value == null)
                {
                    if (!hidden.ContainsKey(groupName.ToLower())) // ADD NEW GROUP
                    {
                        added.Add(new ExtensionOrGroup() { name = groupName, shortName = groupShortName, look = GetLook(row)  });
                        continue;
                    } 
                    row.Cells[colID.Index].Value = hidden[groupName.ToLower()]; // group exists, but was deleted and re-added
                    hidden[groupName.ToLower()] = READDED;
                }

                // CHANGE GROUP
                string id = row.Cells[colID.Index].Value.ToString();
                changed.Add(new ExtensionOrGroup() { id = row.Cells[colID.Index].Value.ToString(), name = groupName, shortName = groupShortName, look = GetLook(row) });
            }

            // DELETE GROUPS
            foreach (var h in hidden) if (h.Value != READDED) deletedIds.Add(h.Value);

            DialogResult = DialogResult.OK; Close();
        }

        private bool GetName(DataGridViewRow row, bool shortName, bool toLower, out string name)
        {
            name = string.Empty; int index = shortName ? colShortName.Index : colName.Index;
            if (row.Cells[index].Value == null) return false;
            name = row.Cells[index].Value.ToString().Trim();
            if (name == string.Empty) return false;
            if (toLower) name = name.ToLower();
            return true;
        }

        private LookDef GetLook(DataGridViewRow row)
        {
            return row.Cells[colLook.Index].Value as LookDef;
        }

        private void dgvExtensionsOrGroups_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex != colLook.Index || e.RowIndex < 0) return;
            GetLook(dgvExtensionsOrGroups.Rows[e.RowIndex]).PaintGridSymbol(e);
            e.Handled = true;
        }

        private void dgvExtensionsOrGroups_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colLook.Index || e.RowIndex < 0) return;
            LookDef lookDef = LookDef.DefineLook(); if (lookDef == null) return;
            dgvExtensionsOrGroups.Rows[e.RowIndex].Cells[colLook.Index].Value = lookDef;
            dgvExtensionsOrGroups.Update();
        }

        private void dgvExtensionsOrGroups_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete) return;
            btnDelete_Click(null, null);
        }
    }
}
