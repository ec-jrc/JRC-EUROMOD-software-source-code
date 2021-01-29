using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    internal class Discrepancy //currently very simple structure to store a discrepancy for a policy/function/parameter (could be extended to more complexity if necessary)
    {
        internal Discrepancy(string description = "", bool significant = false) { _description = description; _significant = significant; }
        internal string _description = string.Empty;
        internal bool _significant = false; //significant discrepancies refer to differences in name/group/order etc.
                                            //not significant differences to discrepancies refer to differences in comments, etc.
        
        const string _separator = "^~°";
        internal string ToStoreFormat() { return _description + _separator + _significant.ToString(); }
        internal void FromStoreFormat(string storeFormat)
        {
            List<string> descriptionAndSignificance = storeFormat.Split(new string[] { _separator }, StringSplitOptions.None).ToList<string>();
            if (descriptionAndSignificance.Count == 2)
            {
                _description = descriptionAndSignificance.ElementAt(0);
                _significant = EM_Helpers.SaveConvertToBoolean(descriptionAndSignificance.ElementAt(1));
            }
        }
    }

    internal class ImportByIDDiscrepancies
    {
        DateTime _creationDate;
        Dictionary<string, Discrepancy> _discrepancies = new Dictionary<string, Discrepancy>(); //lists explanations for discrepancies by id of policy/function/parameter
        Dictionary<string, string> _concernedSystems = new Dictionary<string, string>();

        const string _idSeparator = "°^~";
        const string _entrySeparator = "~^°";

        internal ImportByIDDiscrepancies()
        {
            _creationDate = DateTime.Now;
        }

        internal void Clear() {_discrepancies.Clear(); }
        internal int Count() { return _discrepancies.Count; }

        internal void AddDiscrepancy(string id, Discrepancy discrepancy)
        {
            if (discrepancy._description != string.Empty)
                _discrepancies.Add(id, discrepancy);
        }
        internal void AddDiscrepancy(string id, string description, bool significant)
        {
            if (description != string.Empty)
                _discrepancies.Add(id, new Discrepancy(description, significant));
        }

        internal string GetDiscrepancy(List<string> polFuncPar_idsWithinAvailableSystems, List<string> idsOfAvailableSystems = null)
        {
            foreach (string id in polFuncPar_idsWithinAvailableSystems)
                if (_discrepancies.Keys.Contains(id))
                    return (idsOfAvailableSystems == null) ? _discrepancies[id]._description :
                           GetIdentificationInfo(idsOfAvailableSystems, true) + Environment.NewLine + _discrepancies[id]._description;
            return string.Empty;
        }

        internal bool HasDiscrepancy(List<string> polFuncPar_idsWithinAvailableSystems, out bool significant)
        {
            significant = false;
            foreach (string id in polFuncPar_idsWithinAvailableSystems)
                if (_discrepancies.Keys.Contains(id))
                {
                    significant = _discrepancies[id]._significant;
                    return true;
                }
            return false;
        }

        internal string GetIdentificationInfo(List<string> idsOfAvailableSystems, bool lineBreak = false)
        {
            string info = "Refers to system(s): ";
            foreach (string systemID in _concernedSystems.Keys)
                if (idsOfAvailableSystems.Contains(systemID))
                    info += _concernedSystems[systemID] + " "; //not all the initialy checked or imported systems may exist anymore (manually removed or because the showed no differences)
            if (lineBreak)
                info += Environment.NewLine;
            info += "(info created " + _creationDate.ToString() + ")"; 
            return info;
        }

        internal void WriteToFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Write discrepancy marker info to file ...";
                openFileDialog.Filter = "Text files (*.txt)|*.txt";
                openFileDialog.CheckPathExists = true;
                openFileDialog.CheckFileExists = false;
                openFileDialog.AddExtension = true;
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                    return;

                //store creation-date of info
                System.IO.StreamWriter txtWriter = new System.IO.StreamWriter(openFileDialog.FileName);
                txtWriter.WriteLine(_creationDate);

                //store concerned systems
                string strConcernedSystems = string.Empty;
                foreach (string systemID in _concernedSystems.Keys)
                    strConcernedSystems += systemID + _idSeparator + _concernedSystems[systemID] + _entrySeparator;
                txtWriter.WriteLine(strConcernedSystems);

                //store discrepancies
                string strDiscrepancies = string.Empty;
                foreach (string id in _discrepancies.Keys)
                    strDiscrepancies += id + _idSeparator + _discrepancies[id].ToStoreFormat() + _entrySeparator;
                txtWriter.WriteLine(strDiscrepancies);
                txtWriter.Close();

                UserInfoHandler.ShowSuccess("Information successfully stored in file " + openFileDialog.FileName);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal bool LoadFromFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Load discrepancy marker info from file ...";
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.CheckPathExists = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.AddExtension = true;
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                    return false;

                System.IO.StreamReader txtReader = new System.IO.StreamReader(openFileDialog.FileName);

                //read creation-date of info
                string strCreationDate = txtReader.ReadLine();
                _creationDate = Convert.ToDateTime(strCreationDate);

                //read concerned systems
                _concernedSystems.Clear();
                string strConcernedSystems = txtReader.ReadLine();
                foreach (string strConcernedSystem in strConcernedSystems.Split(new string[] {_entrySeparator}, StringSplitOptions.None))
                {   
                    List<string> idAndName = strConcernedSystem.Split(new string[] {_idSeparator}, StringSplitOptions.None).ToList<string>();
                    if (idAndName.Count == 2)
                        _concernedSystems.Add(idAndName.ElementAt(0), idAndName.ElementAt(1));
                }

                //read discrepancies
                _discrepancies.Clear();
                string strDiscrepancies = txtReader.ReadToEnd();
                foreach (string strDiscrepancy in strDiscrepancies.Split(new string[] { _entrySeparator }, StringSplitOptions.None))
                {
                    List<string> idAndDiscrepancy = strDiscrepancy.Split(new string[] { _idSeparator }, StringSplitOptions.None).ToList<string>();
                    if (idAndDiscrepancy.Count == 2)
                    {
                        Discrepancy discrepancy = new Discrepancy();
                        discrepancy.FromStoreFormat(idAndDiscrepancy.ElementAt(1));
                        _discrepancies.Add(idAndDiscrepancy.ElementAt(0), discrepancy);
                    }
                }

                if (_creationDate == null || _concernedSystems.Count == 0 || _discrepancies.Count == 0)
                    throw new System.ArgumentException("The selected file does not seem to contain the relevant information."); //to hand the error to the error handler

                UserInfoHandler.ShowSuccess("Information successfully loaded.");
                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowError("Failed to read info for markers from file because of the following error:"
                                           + Environment.NewLine + Environment.NewLine + exception.Message);
                return false;
            }
        }

        static internal void HandleName(ref Discrepancy discrepancy, string inValue, string outValue)
        {
            inValue = HandleNullValue(inValue);
            outValue = HandleNullValue(outValue);
            if (inValue != outValue)
            {
                discrepancy._description += "name: " + outValue + Environment.NewLine;
                discrepancy._significant = true;
            }
        }

        static internal void HandleComment(ref Discrepancy discrepancy, string inValue, string outValue, bool privateComment)
        {
            inValue = HandleNullValue(inValue);
            outValue = HandleNullValue(outValue);
            if (inValue != outValue)
                discrepancy._description += (privateComment ? "private " : "") + "comment: " +
                    ((outValue == string.Empty) ? "not set" : outValue) + Environment.NewLine;
        }

        static internal void HandlePrivate(ref Discrepancy discrepancy, string inValue, string outValue)
        {
            inValue = HandleNullValue(inValue);
            outValue = HandleNullValue(outValue);
            if (inValue == string.Empty) inValue = DefPar.Value.NO;
            if (outValue == string.Empty) outValue = DefPar.Value.NO;
            if (inValue != outValue)
            {
                discrepancy._description += "private: " + (outValue != DefPar.Value.YES ? "not " : "") + "set" + Environment.NewLine;
                discrepancy._significant = true;
            }
        }

        static internal void HandleGroup(ref Discrepancy discrepancy, string inValue, string outValue)
        {
            inValue = HandleNullValue(inValue);
            outValue = HandleNullValue(outValue);
            if (inValue != outValue)
            {
                discrepancy._description += "group: " + ((outValue == string.Empty || outValue == null) ? "not set" : outValue) + Environment.NewLine;
                discrepancy._significant = true;
            }
        }

        static internal void HandleOrder(string inName, string outOrder, ref SortedDictionary<int, string> orderListing, ref bool differentOrder)
        {   //generate a list of descriptions of in-country components (pol/func/par-name and row-number),
            //which can be displayed ordered by out-country-order (helped by the sorted dictionary), if out-country-order is different
            int outNumOrder = -1;
            if (outOrder == "-1") //component does not exist in out-country and is only added to ensure correct numbering of in-components
                outNumOrder = Math.Min(-1, orderListing.Count > 0 ? orderListing.Keys.Min() - 1 : -1); //looks rather complicated, but is due to simple facts: (a) Keys.Min/Max cannot handle empty lists (b) SortedDictionary cannot handel equal keys (c) key needs to be negative (see AddOrderDiscrepancy)
            else
            {
                outNumOrder = EM_Helpers.SaveConvertToInt(outOrder);
                if (!differentOrder && orderListing.Count > 0 && outNumOrder < orderListing.Keys.Max())
                    differentOrder = true; //if an element has a lower order than any predecessor (i.a. a predecessor from the point of view of in-country) the order in out-country is different
            }
            orderListing.Add(outNumOrder, (orderListing.Count + 1).ToString() + ". " + inName);
        }

        internal void AddOrderDiscrepancy(string inID, SortedDictionary<int, string> orderListing, string type)
        {   //compose a description out of the list generated in HandleOrder
            string description = type + " order:" + Environment.NewLine;
            for (int index = 0; index < orderListing.Count; ++index)
                if (orderListing.ElementAt(index).Key >= 0) //<0 means that component does not exist in out-country (see call of HandleOrder in ImportByIDAssistant)
                    description += "\t" + orderListing.ElementAt(index).Value + Environment.NewLine;

            if (_discrepancies.ContainsKey(inID))
            {
                _discrepancies[inID]._description += description; //add the description to an already existing descripiton
                _discrepancies[inID]._significant = true;
            }
            else
                _discrepancies.Add(inID, new Discrepancy(description, true)); //not yet a description existing: add a description
        }

        internal void SetConcernedSystems(List<CountryConfig.SystemRow> systemRows)
        {   //store id and name of concerned system to be able to inform the user to which systems the info refers (systems may be deleted, if no differneces, thus do not save system-rows)
            _concernedSystems.Clear();
            foreach (CountryConfig.SystemRow systemRow in systemRows)
                _concernedSystems.Add(systemRow.ID, systemRow.Name);
        }

        private static string HandleNullValue(string value) { return (value == null || value == "\"\"") ? string.Empty : value; }
    }
}