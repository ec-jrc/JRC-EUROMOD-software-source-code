using System.Collections.Generic;
using System;

namespace EM_Common
{
    public static partial class DefPar
    {
        public enum PAR_TYPE
        {
            FORMULA, CONDITION, BOOLEAN, NUMBER, TEXT, VAR, STRINGVAR, OUTVAR, IL, TU, VARorIL, CATEG, PLACEHOLDER, QUERY
        }

        public enum PREFIX
        {
            HEAD, PARTNER, NONE
        }

        internal static List<string> GetWhoMustBeEligValues()
        {
            return new List<string>() { Value.WHO_NOBODY, Value.WHO_ONE, Value.WHO_ALL, Value.WHO_ONE_ADULT, Value.WHO_ALL_ADULTS };
        }

        public class Common
        {
            public const string TAX_UNIT = "TAX_UNIT";
            public const string Output_Var = "Output_Var";
            public const string Output_Add_Var = "Output_Add_Var";
            public const string Result_Var = "Result_Var";
            public const string Who_Must_Be_Elig = "Who_Must_Be_Elig";
            public const string Elig_Var = "Elig_Var";
            public const string Run_Cond = "Run_Cond";
            public const string LowLim = "LowLim";
            public const string UpLim = "UpLim";
            public const string Limpriority = "Limpriority";
            public const string Threshold = "Threshold";
            public const string Round_to = "Round_to";
            public const string Round_Up = "Round_Up";
            public const string Round_Down = "Round_Down";

            internal enum TU_ADD_MODE { COMPULSORY, OPTIONAL, NOT }
            internal enum OUTVAR_ADD_MODE { COMPULSORY, OPTIONAL, OPTIONAL_NOADD, NOT }

            internal static void Add(DefinitionAdmin.Fun fun,
                TU_ADD_MODE tuMode = TU_ADD_MODE.COMPULSORY,
                OUTVAR_ADD_MODE outvarMode = OUTVAR_ADD_MODE.COMPULSORY, string outvarDefault = null, bool addResultVar = true,
                bool addElig = true, bool addRunCond = true, bool addLimits = true, bool addRound = true)
            {
                if (outvarMode != OUTVAR_ADD_MODE.NOT)
                {
                    fun.par.Add(Output_Var, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.OUTVAR, isCommon = true,
                        minCount = outvarMode == OUTVAR_ADD_MODE.COMPULSORY ? 1 : 0, maxCount = 1,
                        defaultValue = outvarDefault,
                        substitutes = outvarMode == OUTVAR_ADD_MODE.OPTIONAL_NOADD ? new List<string>() : new List<string> { Output_Add_Var },
                        description = "Variable for storing the result of the function. Result of function overwrites the current value of the variable."
                    });
                    if (outvarMode != OUTVAR_ADD_MODE.OPTIONAL_NOADD)
                        fun.par.Add(Output_Add_Var, new DefinitionAdmin.Par()
                        {
                            valueType = PAR_TYPE.OUTVAR, isCommon = true,
                            minCount = 0, maxCount = 1,
                            defaultValue = outvarDefault,
                            substitutes = new List<string>() { Output_Var },
                            description = "Variable for storing the result of the function. Result of function is added to the current value of the variable."
                        });
                }
                if (tuMode != TU_ADD_MODE.NOT)
                    fun.par.Add(TAX_UNIT, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.TU, isCommon = true,
                        minCount = tuMode == TU_ADD_MODE.COMPULSORY ? 1 : 0, maxCount = 1,
                        description = "Assessment unit for function's calculations."
                    });
                if (addResultVar)
                    fun.par.Add(Result_Var, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.OUTVAR, isCommon = true,
                        minCount = 0, maxCount = 1,
                        description = "Variable for storing the result of the function. Result of function overwrites the current value of the variable."
                    });

                if (addElig)
                {
                    fun.par.Add(Who_Must_Be_Elig, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.CATEG, isCommon = true,
                        minCount = 0, maxCount = 1,
                        categValues = GetWhoMustBeEligValues(),
                        description = "Function's calculations are carried out if ..." + Environment.NewLine +
                                              "- one (one_member): ... one member of the assessment unit is 'eligible'" + Environment.NewLine +
                                              "- one_adult: ... one adult member of the assessment unit is 'eligible'" + Environment.NewLine +
                                              "- all (all_members; taxunit): ... all members of the assessment unit are 'eligible'" + Environment.NewLine +
                                              "- all_adults: ... all adult members of the assessment unit are 'eligible'" + Environment.NewLine +
                                              "- nobody: ... always" + Environment.NewLine +
                                              "'eligible' is determined by the variable indicated by parameter Elig_Var"
                    });
                    fun.par.Add(Elig_Var, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.VAR, isCommon = true,
                        minCount = 0, maxCount = 1, defaultValue = DefVarName.DEFAULT_ELIGVAR,
                        description = "Variable indicating whether a person is 'eligible' (see parameter Who_Must_Be_Elig):" + Environment.NewLine +
                                              "- zero: person is not eligible" + Environment.NewLine +
                                              "- non zero: person is eligible"
                    });
                }

                if (addRunCond) AddRunCond(fun);

                if (addLimits)
                {
                    fun.par.Add(LowLim, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.FORMULA, isCommon = true,
                        minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.NEG_INFINITE,
                        description = "Replaces result of function if result is smaller."
                    });
                    fun.par.Add(UpLim, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.FORMULA, isCommon = true,
                        minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.POS_INFINITE,
                        description = "Replaces result of function if result is higher."
                    });
                    fun.par.Add(Threshold, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.FORMULA, isCommon = true,
                        minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.NEG_INFINITE,
                        description = "Replaces result of function if result is smaller: if lower limit is not defined by zero, otherwise by lower limit."
                    });
                    fun.par.Add(Limpriority, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.CATEG, isCommon = true,
                        minCount = 0, maxCount = 1, defaultValue = Value.NA,
                        categValues = new List<string>() { Value.LIMPRI_LOWER, Value.LIMPRI_UPPER, Value.NA },
                        description = "Parameter for the further specification of an operand:" + Environment.NewLine +
                                              "Possible values:" + Environment.NewLine +
                                              "If upper limit (UpLim) is smaller than lower limit (LowLim) ..." + Environment.NewLine +
                                              "- upper: ... upper limit dominates" + Environment.NewLine +
                                              "- lower: ... lower limit dominates" + Environment.NewLine +
                                              "- not defined: ... a warning is issued."
                    });
                }

                if (addRound)
                {
                    fun.par.Add(Round_Down, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.NUMBER, isCommon = true,
                        minCount = 0, maxCount = 1,
                        substitutes = new List<string>() { Round_to, Round_Up },
                        description = "Result is rounded down to nearest whole number if set to 1, to nearest number with 1 decimal if set to 0.1, to nearest 10 if set to 10, etc."
                    });

                    fun.par.Add(Round_Up, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.NUMBER, isCommon = true,
                        minCount = 0, maxCount = 1,
                        substitutes = new List<string>() { Round_Down, Round_to },
                        description = "Result is rounded up to nearest whole number if set to 1, to nearest number with 1 decimal if set to 0.1, to nearest 10 if set to 10, etc."
                    });

                    fun.par.Add(Round_to, new DefinitionAdmin.Par()
                    {
                        valueType = PAR_TYPE.NUMBER, isCommon = true,
                        minCount = 0, maxCount = 1,
                        substitutes = new List<string>() { Round_Down, Round_Up },
                        description = "Result is rounded to nearest whole number if set to 1, to nearest number with 1 decimal if set to 0.1, to nearest 10 if set to 10, etc."
                    });
                }
            }

            internal static void AddRunCond(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Run_Cond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION, isCommon = true,
                    minCount = 0, maxCount = 1,
                    description = "Function is only carried out if the condition is fulfilled. The parameter is intended to be a conditional switch. Thus the condition must not be individual or household based, but refer to a specific processing state or other global condition."
                });
            }
        }
    }
}
