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
        private readonly List<Variable> VariableIndex = new List<Variable>();
        private readonly List<SavedNumber> SavedNumbers = new List<SavedNumber>();
        internal Dictionary<string, Func<List<double>, bool>> filters = new Dictionary<string, Func<List<double>, bool>>();

        internal class Variable
        {
            internal readonly string name;
            internal readonly LocalMap localMap = null;
            internal readonly int index;
            internal readonly bool monetary = true;

            internal Variable(string _name, LocalMap _localMap, int _index, bool _monetary) { name = _name; localMap = _localMap; index = _index; monetary = _monetary; }
        }

        internal class SavedNumber
        {
            internal readonly string name;
            internal readonly LocalMap localMap = null;
            internal readonly double number;
            internal readonly int sdcObsNo;

            internal SavedNumber(string _name, LocalMap _localMap, double _number, int _sdcObsNo) { name = _name; localMap = _localMap; number = _number; sdcObsNo = _sdcObsNo; }
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

        private Variable GetVariable(string varName, LocalMap localMapRequestor)
        {
            if (string.IsNullOrEmpty(varName)) return null;
            return (from v in VariableIndex
                    where v.name.ToLower() == varName.ToLower() && LocalMap.AllowsFor(v.localMap, localMapRequestor)
                    select v).FirstOrDefault();
        }

        internal bool HasVariable(string varName, LocalMap localMapRequestor)
        {
            return GetVariable(varName, localMapRequestor) != null;
        }

        internal int GetVarIndex(string varName, LocalMap localMapRequestor)
        {
            Variable v = GetVariable(varName, localMapRequestor); return v == null ? -1 : v.index;
        }

        internal void ConfirmKey()
        {
            keyIndex = GetVarIndex(keyName, null);
        }

        internal void AddVar(string varName, bool monetary, LocalMap localMap)
        {
            if (!HasVariable(varName, localMap)) VariableIndex.Add(new Variable(varName, localMap, VariableIndex.Count, monetary));
        }

        internal bool IsVarMonetary(string varName, LocalMap localMapRequestor)
        {
            Variable v = GetVariable(varName, localMapRequestor); return v == null ? true : v.monetary;
        }

        internal SavedNumber GetSavedNumber(string varName, LocalMap localMapRequestor)
        {
            if (string.IsNullOrEmpty(varName)) return null;
            return (from sn in SavedNumbers
                    where sn.name.ToLower() == varName.ToLower() && LocalMap.AllowsFor(sn.localMap, localMapRequestor)
                    select sn).FirstOrDefault();
        }

        internal bool HasSavedNumber(string varName, LocalMap localMapRequestor)
        {
            return GetSavedNumber(varName, localMapRequestor) != null;
        }

        internal void SetSavedNumber(SavedNumber savedNumber)
        {
            SavedNumber sn = GetSavedNumber(savedNumber.name, savedNumber.localMap);
            if (sn != null) SavedNumbers.Remove(sn);
            SavedNumbers.Add(savedNumber);
        }
    }
}
