using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.ImportVariables
{
    internal class ImportVariablesManager
    {
        ImportVariablesForm _importVariablesForm = null;
        DataGridView _dgvVariables = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        const short _typeNode = 1;
        const short _levelNode = 2;
        const short _acronymNode = 3;

        void PerformImportVariables()
        {
            foreach (DataGridViewRow dgvRow in _dgvVariables.Rows)
            {
                if (EM_Helpers.SaveConvertToBoolean(dgvRow.Cells[_importVariablesForm.colPerformVariables.Name].Value) != true)
                    continue; //checkbox to perform change not checked

                //adapt variables
                VarConfig.VariableRow internalVariable = null;
                VarConfig.VariableRow externalVariable = null;

                switch (dgvRow.Cells[_importVariablesForm.colAction.Name].Value.ToString())
                {
                    case ImportVariablesForm._actionAdd:
                        externalVariable = dgvRow.Tag as VarConfig.VariableRow;
                        internalVariable = _varConfigFacade.AddVariable(externalVariable.Name, externalVariable.Monetary, externalVariable.HHLevel, externalVariable.AutoLabel);
                        foreach (VarConfig.CountryLabelRow externalLabel in externalVariable.GetCountryLabelRows())
                            _varConfigFacade.SetCountrySpecificDescription(internalVariable, externalLabel.Country, externalLabel.Label);
                        _varConfigFacade.Commit();
                        break;

                    case ImportVariablesForm._actionDelete:
                        internalVariable = dgvRow.Tag as VarConfig.VariableRow;
                        _varConfigFacade.DeleteVariable(internalVariable);
                        _varConfigFacade.Commit();
                        break;

                    case ImportVariablesForm._actionChange:
                        internalVariable = (dgvRow.Tag as Dictionary<VarConfig.VariableRow, VarConfig.VariableRow>).Keys.ElementAt(0);
                        externalVariable = (dgvRow.Tag as Dictionary<VarConfig.VariableRow, VarConfig.VariableRow>).Values.ElementAt(0);

                        string info = dgvRow.Cells[_importVariablesForm.colInfo.Name].Value.ToString();
                        if (info == ImportVariablesForm._infoChangeCountryLabel)
                        {
                            foreach (VarConfig.CountryLabelRow externalLabel in externalVariable.GetCountryLabelRows())
                                _varConfigFacade.SetCountrySpecificDescription(internalVariable, externalLabel.Country, externalLabel.Label);
                        }
                        else
                            internalVariable.Monetary = externalVariable.Monetary;
                        _varConfigFacade.Commit();
                        break;
                }
            }
        }

        void PerformImportAcronyms()
        {
            foreach (TreeListNode typeNode in _treeAcronyms.Nodes)
            {
                PerformImportNode(typeNode, _typeNode);
                foreach (TreeListNode levelNode in typeNode.Nodes)
                {
                    PerformImportNode(levelNode, _levelNode);
                    foreach (TreeListNode acronymNode in levelNode.Nodes)
                        PerformImportNode(acronymNode, _acronymNode);
                }
            }
        }

        void PerformImportNode(TreeListNode node, short nodeType)
        {
            string action = node.GetValue(_importVariablesForm.colActionAcronyms).ToString();
            
            bool perform = false;
            if (action != string.Empty)
                perform = EM_Helpers.SaveConvertToBoolean(node.GetValue(_importVariablesForm.colPerformAcronyms));
            if (!perform)
                return;

            switch (nodeType)
            {
                case _typeNode:
                    switch (action)
                    {
                        case ImportVariablesForm._actionAdd:
                            PerformAddType(node.Tag as VarConfig.AcronymTypeRow);
                            break;

                        case ImportVariablesForm._actionDelete:
                            (node.Tag as VarConfig.AcronymTypeRow).Delete();
                            _varConfigFacade.Commit();
                            break;

                        case ImportVariablesForm._actionChange:
                            PerformChangeType((node.Tag as Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow>).Keys.ElementAt(0),
                                              (node.Tag as Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow>).Values.ElementAt(0));
                            break;
                    }
                    break;

                case _levelNode:
                    switch (action)
                    {
                        case ImportVariablesForm._actionAdd:
                            VarConfig.AcronymTypeRow internalType = (node.ParentNode.Tag as Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow>).Keys.ElementAt(0);
                            VarConfig.AcronymLevelRow externalLevel = node.Tag as VarConfig.AcronymLevelRow;
                            PerformAddLevel(internalType, externalLevel, GetAddAferLevel(internalType, externalLevel));
                            break;

                        case ImportVariablesForm._actionDelete:
                            (node.Tag as VarConfig.AcronymLevelRow).Delete();
                            _varConfigFacade.Commit();
                            break;

                        case ImportVariablesForm._actionChange:
                            PerformChangeLevel((node.Tag as Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow>).Keys.ElementAt(0),
                                               (node.Tag as Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow>).Values.ElementAt(0));
                            break;
                    }
                    break;

                case _acronymNode:
                    switch (action)
                    {
                        case ImportVariablesForm._actionAdd:
                            PerformAddAcronym((node.ParentNode.Tag as Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow>).Keys.ElementAt(0),
                                               node.Tag as VarConfig.AcronymRow);
                            break;

                        case ImportVariablesForm._actionDelete:
                            (node.Tag as VarConfig.AcronymRow).Delete();
                            _varConfigFacade.Commit();
                            break;

                        case ImportVariablesForm._actionChange:
                            PerformChangeAcronym((node.Tag as Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>).Keys.ElementAt(0),
                                                 (node.Tag as Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>).Values.ElementAt(0),
                                                  node.GetValue(_importVariablesForm.colInfoAcronyms).ToString());
                            break;
                    }
                    break;
            }
        }

        void PerformAddType(VarConfig.AcronymTypeRow externalType)
        {
            VarConfig.AcronymTypeRow internalType = _varConfigFacade.AddAcronymTypeRow(externalType.LongName, externalType.ShortName);
            VarConfig.AcronymLevelRow addAfterRow = null;
            foreach (VarConfig.AcronymLevelRow externalLevel in _importVariablesForm._externalVarConfigFacade.GetAcronymLevelsSortedByIndex(externalType))
                addAfterRow = PerformAddLevel(internalType, externalLevel, addAfterRow);
            _varConfigFacade.Commit();
        }

        VarConfig.AcronymLevelRow GetAddAferLevel(VarConfig.AcronymTypeRow internalType, VarConfig.AcronymLevelRow externalLevel)
        {//find the level after which to insert the new level
            //first collect all levels before the new level in the external type ...
            List<VarConfig.AcronymLevelRow> preRows = new List<VarConfig.AcronymLevelRow>();
            foreach (VarConfig.AcronymLevelRow externalSiblingLevel in externalLevel.AcronymTypeRow.GetAcronymLevelRows())
            {
                if (externalSiblingLevel.Index < externalLevel.Index)
                    preRows.Add(externalSiblingLevel);
            }
            preRows = (from preRow in preRows select preRow).OrderBy(preRow => preRow.Index).ToList(); //... and order them by index

            //then try to find the (equivalent of the) new level's direct predecessor in the internal type,
            //if found - this is the level where to add the new one after, if not found - try with the next to the direct predecessor, and so on
            VarConfig.AcronymLevelRow addAfterLevel = null;
            for (int index = preRows.Count - 1; index >= 0; --index)
            {
                foreach (VarConfig.AcronymLevelRow internalSiblingLevel in internalType.GetAcronymLevelRows())
                {
                    if (internalSiblingLevel.Name.ToLower() == preRows.ElementAt(index).Name.ToLower())
                    {
                        addAfterLevel = internalSiblingLevel;
                        break;
                    }
                }
                if (addAfterLevel != null)
                    break;
            }

            return addAfterLevel;
        }


        VarConfig.AcronymLevelRow PerformAddLevel(VarConfig.AcronymTypeRow internalType, VarConfig.AcronymLevelRow externalLevel, VarConfig.AcronymLevelRow addAfterRow)
        {
            VarConfig.AcronymLevelRow internalLevel = _varConfigFacade.AddAcronymLevelRow(internalType, addAfterRow, externalLevel.Name); //todo: null is not correct
            foreach (VarConfig.AcronymRow externalAcronym in externalLevel.GetAcronymRows())
                PerformAddAcronym(internalLevel, externalAcronym);
            _varConfigFacade.Commit();
            return internalLevel;
        }

        void PerformAddAcronym(VarConfig.AcronymLevelRow internalLevel, VarConfig.AcronymRow externalAcronym)
        {
            VarConfig.AcronymRow internalAcronym = _varConfigFacade.AddAcronymRow(internalLevel, externalAcronym.Name, externalAcronym.Description);
            foreach (VarConfig.CategoryRow externalCategory in externalAcronym.GetCategoryRows())
                _varConfigFacade.AddCategoryRow(internalAcronym, externalCategory.Value, externalCategory.Description);
            _varConfigFacade.Commit();
        }

        void PerformChangeType(VarConfig.AcronymTypeRow internalType, VarConfig.AcronymTypeRow externalType)
        {
            internalType.LongName = externalType.LongName;
            _varConfigFacade.Commit();
        }

        void PerformChangeLevel(VarConfig.AcronymLevelRow internalLevel, VarConfig.AcronymLevelRow externalLevel)
        {
            //this type of change does not exist (currently)
        }

        void PerformChangeAcronym(VarConfig.AcronymRow internalAcronym, VarConfig.AcronymRow externalAcronym, string info)
        {
            if (info.Contains(ImportVariablesForm._infoNewDescription))
                internalAcronym.Description = externalAcronym.Description;

            if (info.Contains(ImportVariablesForm._infoChangeCategory))
            {
                _varConfigFacade.DeleteCategoryRows(internalAcronym);

                foreach (VarConfig.CategoryRow externalCategory in externalAcronym.GetCategoryRows())
                    _varConfigFacade.AddCategoryRow(internalAcronym, externalCategory.Value, externalCategory.Description);
            }
            _varConfigFacade.Commit();
        }

        internal bool PerformImport(ImportVariablesForm importVariablesForm)
        {
            try
            {
                _importVariablesForm = importVariablesForm;
                _dgvVariables = _importVariablesForm.dgvVariables;
                _treeAcronyms = _importVariablesForm.treeAcronyms;
                _varConfigFacade = _importVariablesForm._internalVarConfigFacade;

                PerformImportVariables();
                PerformImportAcronyms();
                return true;
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
                return false;
            }
        }
    }
}
