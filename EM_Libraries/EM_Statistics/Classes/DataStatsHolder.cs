using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public class DataStatsHolder
    {
        private readonly string keyName;
        private int keyIndex = 0;
        internal int dataNo;
        internal string level;
        internal string packageKey;
        internal List<List<double>> data = new List<List<double>>();
        private readonly Dictionary<double, int> ObservationIndex = new Dictionary<double, int>();
        private readonly Dictionary<string, int> VariableIndex = new Dictionary<string, int>();
        private readonly Dictionary<string, SavedNumber> SavedNumbers = new Dictionary<string, SavedNumber>();
        internal Dictionary<string, Func<List<double>, bool>> filters = new Dictionary<string, Func<List<double>, bool>>();

        internal class SavedNumber
        {
            internal readonly double number;
            internal readonly int sdcObsNo;
            
            internal SavedNumber(double _number, int _sdcObsNo) { number = _number; sdcObsNo = _sdcObsNo; }
        }

        public DataStatsHolder(string _keyVar, int _dataNo, string _level, string _packageKey)
        {
            keyName = _keyVar;
            dataNo = _dataNo;
            level = _level;
            packageKey = _packageKey;
        }

        internal int GetKeyIndex() { return keyIndex; }
        internal string GetKeyName() { return keyName; }

        internal void AddVar(string varName)
        {
            if (!VariableIndex.ContainsKey(varName)) VariableIndex.Add(varName, VariableIndex.Count);
        }

        internal void AddObs(List<double> list)
        {
            ObservationIndex.Add(list[keyIndex], data.Count);
            data.Add(list);
        }

        internal List<double> GetObs(double keyVal)
        {
            return data[ObservationIndex[keyVal]];
        }

        internal bool HasObs(double keyVal)
        {
            return ObservationIndex.ContainsKey(keyVal);
        }

        internal List<List<double>> GetData(Func<List<double>, bool> func = null)
        {
            return func == null ? data : data.Where(func).ToList();
        }

        internal void ConfirmKey()
        {
            keyIndex = VariableIndex[keyName];
        }

        internal int GetVarIndex(string varName)
        {
            return string.IsNullOrEmpty(varName) || !VariableIndex.ContainsKey(varName) ? -1 : VariableIndex[varName];
        }

        internal bool HasVariable(string varName)
        {
            return !string.IsNullOrEmpty(varName) && VariableIndex.ContainsKey(varName);
        }

        internal SavedNumber GetSavedNumber(string varName)
        {
            return SavedNumbers[varName];
        }

        internal bool HasSavedNumber(string varName)
        {
            return !string.IsNullOrEmpty(varName) && SavedNumbers.ContainsKey(varName);
        }

        internal void SetSavedNumber(string varName, SavedNumber savedNumber)
        {
            if (string.IsNullOrEmpty(varName)) return;
            if (SavedNumbers.ContainsKey(varName)) { SavedNumbers[varName] = savedNumber; }
            else SavedNumbers.Add(varName, savedNumber);
        }
    }
}
