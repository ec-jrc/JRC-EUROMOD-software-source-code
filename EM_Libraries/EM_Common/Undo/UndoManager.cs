using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace EM_Common
{
    public class ADOUndoManager
    {
        // !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! 
        // !!!  Important note: DataTabels require a primary key for the UndoManager to work properly  !!!
        // !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! !!! 

        List<DataSet> _ministeredDataSets;
        List<List<DataSet>> _logs;
        int _currentStep = 0;

        const int MAX_UNDO_RECORDS = 100;

        DataSet GetConcernedDataSet(DataSet log)
        {
            foreach (DataSet dataSet in _ministeredDataSets)
                if (dataSet.DataSetName == log.DataSetName)
                    return dataSet;
            return null;
        }

        List<DataRow> CollectRowsOfState(DataSet log, DataRowState state)
        {
            List<DataRow> listToMerge = new List<DataRow>();

            if (log == null) return listToMerge; 

            foreach (DataTable logTable in log.Tables)
            {
                foreach (DataRow logRow in logTable.Rows)
                {
                    if (logRow.RowState == state)
                    {
                        listToMerge.Add(logRow);
                    }
                }
            }
            return listToMerge;
        }

        public ADOUndoManager()
        {
            _ministeredDataSets = new List<DataSet>();
            _logs = new List<List<DataSet>>();
        }

        public void AddDataSet(DataSet dataSet)
        {
            _ministeredDataSets.Add(dataSet);
        }

        public bool HasChanges()
        {
            return _currentStep > 0;
        }

        public bool CanRedo()
        {
            return _currentStep < _logs.Count;
        }

        public void Reset()
        {
            _logs.Clear();
            _currentStep = 0;
        }

        public void Commit()
        {
            List<DataSet> logStep = null;
            foreach (DataSet dataSet in _ministeredDataSets)
            {
                if (dataSet.HasChanges())
                {
                    if (logStep == null)
                    {
                        logStep = new List<DataSet>();
                    }
                    logStep.Add(dataSet.GetChanges());
                    dataSet.AcceptChanges();
                }
            }

            if (logStep != null)
            {
                //first remove undone steps on new commit
                int undoneSteps = _logs.Count - _currentStep;
                if (undoneSteps > 0)
                    _logs.RemoveRange(_currentStep, undoneSteps);

                _logs.Add(logStep);
                _currentStep++;

                // Limit the number of logSteps to avoid infinite growth of the list
                if (_currentStep > MAX_UNDO_RECORDS)
                {
                    _logs.RemoveAt(0);
                    _currentStep--;
                }
            }
        }

        public void Undo()
        {
            if (_currentStep <= 0)
                return;
            
            _currentStep--;
           
            List<DataSet> logStep = _logs[_currentStep];

            foreach (DataSet log in logStep)
            {
                DataSet undoDataSet = GetConcernedDataSet(log);

                if (log != null)
                {
                    DataSet addedLog = log.GetChanges(DataRowState.Added);
                    if (addedLog != null)
                    {
                        //the addedLog contains the added rows and, if there are referrential constraints, the parent rows, even if those are unchanged
                        //those parentrows must be filtered before the merge, otherwise those rows are duplicated!
                        List<DataRow> listToMerge = CollectRowsOfState(addedLog, DataRowState.Added);

                        undoDataSet.Merge(listToMerge.ToArray<DataRow>());
                        foreach (DataTable table in undoDataSet.Tables)
                        {
                            List<DataRow> lstToDelRows = new List<DataRow>();
                            foreach (DataRow row in table.Rows)
                                if (row.RowState != DataRowState.Unchanged)
                                {
                                    lstToDelRows.Add(row);
                                }

                            foreach (DataRow row in lstToDelRows)
                            {
                                row.Delete();
                            }
                        }
                        undoDataSet.AcceptChanges();
                    }

                    DataSet delLog = log.GetChanges(DataRowState.Deleted);
                    if (delLog != null)
                    {
                        foreach (DataTable table in delLog.Tables)
                        {
                            foreach (DataRow row in table.Rows)
                            {
                                row.RejectChanges();
                                undoDataSet.Tables[row.Table.TableName].LoadDataRow(row.ItemArray, LoadOption.OverwriteChanges);
                            }
                        }
                        undoDataSet.AcceptChanges();
                    }

                    DataSet modifLog = log.GetChanges(DataRowState.Modified);
                    if (modifLog != null)
                    {
                        // The addedLog contains the modified rows and, if there are referrential constraints, the parent rows, even if those are unchanged
                        // Those parentrows must be filtered before the merge, otherwise those rows are duplicated!
                        List<DataRow> listToMerge = CollectRowsOfState(modifLog, DataRowState.Modified);
                        undoDataSet.Merge(listToMerge.ToArray<DataRow>());
                        undoDataSet.RejectChanges();
                    }
                }
            }
        }

        public void Redo()
        {
            if (_currentStep >= _logs.Count) return;

            foreach (DataSet log in _logs[_currentStep])
            {
                DataSet undoDataSet = GetConcernedDataSet(log);

                // The log contains the modified rows and, if there are referrential constraints, the parent rows, even if those are unchanged
                // Those parentrows must be filtered before the merge, otherwise those rows are duplicated!
                Merge(undoDataSet, CollectRowsOfState(log.GetChanges(DataRowState.Added),DataRowState.Added));
                Merge(undoDataSet, CollectRowsOfState(log.GetChanges(DataRowState.Deleted), DataRowState.Deleted));
                Merge(undoDataSet, CollectRowsOfState(log.GetChanges(DataRowState.Modified), DataRowState.Modified));
            }
                       
            _currentStep++;
        }

        private static void Merge(DataSet undoDataSet, List<DataRow> rows)
        {
            if (rows != null)
            {
                undoDataSet.Merge(rows.ToArray<DataRow>());
                undoDataSet.AcceptChanges();
            }
        }
    }
}
