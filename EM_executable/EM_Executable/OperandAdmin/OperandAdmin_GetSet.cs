using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class OperandAdmin
    {
        internal bool Exists(string operandName)
        {
            if (indexOperands.ContainsKey(operandName)) return true;

            // if not yet existent, could still be sth like yem_loop3/ils_bsa_loop7, as these variables are only generated "on demand"
            // thus try each Store function whether it "knows" this operand 
            foreach (FunStore storeFun in indexStoreFuns.Values) if (storeFun.IsStoreOperand(operandName)) return true;
            return false;
        }

        internal DefPar.PAR_TYPE GetParType(string operandName) { return Get(operandName).parType; }

        internal bool GetIsMonetary(string operandName) { return Get(operandName).isMonetary; }

        internal bool GetIsGlobal(string operandName) { return Get(operandName).isGlobal; }

        internal bool GetProvidedBySetDefault(string operandName) { return Get(operandName).providedByDefault; }
        internal void SetProvidedBySetDefault(string operandName, bool set = true) { Get(operandName).providedByDefault = set; }

        internal void SetInitialised(string operandName) { Get(operandName).isInitialised = true; }

        internal Dictionary<string, double> GetILContent(string ilName) { return Get(ilName, true).content; }

        internal bool GetWarnIfNonMon(string ilName) { return Get(ilName, true).warnIfNonMon; }

        /// <summary>
        /// returns a variables index in the HH.personVarList
        /// called by Fun.ReplaceVarNameByIndex, to replace (complement) variable name by variable index
        /// (Fun.ReplaceVarNameByIndex is called after data-reading, because on operand-creation-time the HH.personVarList does not exist)
        /// </summary>
        internal int GetIndexInPersonVarList(string varName)
        {
            int index = Get(varName).indexInPersonVarList;
            if (index >= 0) return index;
            throw new Exception($"Operand {varName} does not have an index in HH.personVarList"); // programming error thus throw
        }

        /// <summary>
        /// returns whether a variable exists in the indexed operands
        /// called by _QueryBase.PrepareVarIndices, to check for replacement variables(e.g. ddi & ddilv)
        /// (_QueryBase.PrepareVarIndices is called after data-reading, because on operand-creation-time the HH.personVarList does not exist)
        /// </summary>
        internal bool GetVarExistsInPersonVarList(string varName)
        {
            return indexOperands.ContainsKey(varName);
        }

        /// <summary> get the indices of variables in the HH.personVarList, which were read from file </summary>
        internal List<int> GetReadVarIndices(bool monetaryOnly)
        {
            return (from op in indexOperands
                    where op.Value.readFromFile && !op.Value.providedByDefault &&
                          (!monetaryOnly || op.Value.isMonetary)
                    select op.Value.indexInPersonVarList).ToList();
        }

        /// <summary> get the indices of monetary variables in the HH.personVarList </summary>
        internal List<int> GetMonetaryVarIndices()
        {
            return (from op in indexOperands
                    where op.Value.indexInPersonVarList >= 0 && op.Value.isMonetary
                    select op.Value.indexInPersonVarList).ToList();
        }

        /// <summary> get all variables or ILs who's name matches a pattern (used by DefOutput) </summary>
        internal List<string> GetMatchingVar(string pattern, bool regExpr = false) { return GetMatchingOperands(pattern, false, regExpr); }
        internal List<string> GetMatchingIL(string pattern, bool regExpr = false) { return GetMatchingOperands(pattern, true, regExpr); }
        private List<string> GetMatchingOperands(string pattern, bool il, bool regExpr)
        {
            List<string> matching = new List<string>();
            foreach (var op in indexOperands)
                if (((il && op.Value.parType == DefPar.PAR_TYPE.IL) || (!il && op.Value.parType == DefPar.PAR_TYPE.VAR)) &&
                    EM_Helpers.DoesValueMatchPattern(pattern: pattern, value: op.Key, regExpr: regExpr))
                    matching.Add(op.Key);
            return matching;
        }

        /// <summary> get the names of all variables in the HH.personVarList (currently used for testing only) </summary>
        internal List<string> GetPersonVarNameList()
        {
            var ops = (from op in indexOperands
                       where op.Value.indexInPersonVarList >= 0
                       select op).OrderBy(o => o.Value.indexInPersonVarList);
            return (from op in ops select op.Key).ToList();
        }

        private Operand Get(string operandName, bool getILOp = false)
        {
            if (indexOperands.ContainsKey(operandName))
            {
                if (!getILOp || indexOperands[operandName].parType == DefPar.PAR_TYPE.IL) return indexOperands[operandName];
                throw new Exception($"Operand.Get failed: Operand {operandName}: parType <> {DefPar.PAR_TYPE.IL}"); // programming error, thus throw
            }
            throw new Exception($"Operand index does not contain operand {operandName}"); // ditto
        }

        /// <summary> making list "public" for testing-purposes </summary>
        internal List<string> GetOperandNames() { return indexOperands.Keys.ToList(); }

        /// <summary> called by FunUprate for uprating variables which do not have an own factor with the default factor </summary>
        internal List<string> GetVarsToUprate()
        {
            List<string> varsToUprate = new List<string>();
            foreach (string v in GetVarsToRead())
                if (GetIsMonetary(v) && !GetProvidedBySetDefault(v))
                    varsToUprate.Add(v);
            return varsToUprate;
        }

        internal string GetCreatorFun(string operandName) { return Get(operandName).creatorFun; }
    }
}
