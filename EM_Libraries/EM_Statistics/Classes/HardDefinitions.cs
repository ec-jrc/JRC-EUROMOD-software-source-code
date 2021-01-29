using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public static class HardDefinitions
    {
        internal class FormulaParameter
        {
            internal const string OPENING_TOKEN = "[";
            internal const string CLOSING_TOKEN = "]";
            internal const string SAVED_VAR = "SAVED_VAR" + OPENING_TOKEN;
            internal const string USR_VAR = "USR_VAR" + OPENING_TOKEN;
            internal const string OP_VAR = "OP_VAR" + OPENING_TOKEN;
            internal const string OBS_VAR = "OBS_VAR" + OPENING_TOKEN;
            internal const string TEMP_VAR = "TEMP_VAR" + OPENING_TOKEN;
            internal const string BASE_COL = "BASE_COL" + OPENING_TOKEN;
            internal const string REF_COL = "REF_COL" + OPENING_TOKEN;
            internal const string NumberFormat = "0.###############";
        }

        public static class DefaultCalculationLevels
        {
            public const string INDIVIDUAL = "Individual";
            public const string HOUSEHOLD = "Household";
        }

        internal const string DecileSeparator = "~dec~";
        internal const string Reform = "~ref~";
        internal const string Base = "~base~";
        internal const string Separator = "~";
        internal const string NumberOfIndividuals = "~indno~";
        internal const string SavedTotal = "~total~";

        // Default (always required) variables
        internal const string idPerson = "idperson";
        internal const string idHH = "idhh";
        internal const string weight = "weight";
        internal const string age = "age";
        internal static Dictionary<string, string> alwaysRequiredVariables = new Dictionary<string, string>()
        {
            { idPerson, idPerson }, { idHH, idHH }, { weight, "dwt" }, { age, "dag" }
        };

        public static bool TryParseEnumType<T>(string name, out T result)
        {
            if (Enum.GetNames(typeof(T)).Contains(name)) { result = (T)Enum.Parse(typeof(T), name); return true; }
            result = default(T); return false;
        }

        public static bool InEnumList<T>(T val, T[] values) where T : struct
        {
            return values.Contains(val);
        }

        public enum TemplateType { Default, BaselineReform, Multi };

        public enum UserInputType { Null, VariableName, Numeric, Categorical_VariableName, Categorical_Numeric, PageSelection, ForEachValueDescription };

        internal enum ColumnGrouping { SystemFirst, ColumnFirst };

        internal enum CalculationType { NA, CreateArithmetic, CreateEquivalized, CreateOECDScale, CreateEquivalenceScale, CalculateGini, CalculateMedian, CalculateS8020, CreateDeciles, CreateGroupValue, CreateHHValue, CalculateArithmetic, CalculateSumWeighted, CalculateWeightedAverage, CalculatePovertyGap, CreateFlag, CalculatePopulationCount, Empty, Info };

        internal enum ParameterType { NA, VariableName, SavedNumber, UserVariable, NumericValue, BooleanValue, Action };

        public enum ExportDescriptionMode { No, InSheets, SeparateSheet }
    }
}
