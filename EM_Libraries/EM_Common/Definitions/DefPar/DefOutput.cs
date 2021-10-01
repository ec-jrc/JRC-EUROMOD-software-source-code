using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefOutput
        {
            public const string File = "File";
            public const string Var = "Var";
            public const string StringVar = "StringVar";
            public const string IL = "IL";
            public const string VarGroup = "VarGroup";
            public const string ILGroup = "ILGroup";
            public const string DefIL = "DefIL";
            public const string nDecimals = "nDecimals";
            public const string Append = "Append";
            public const string UnitInfo_Id = "UnitInfo_Id";
            public const string UnitInfo_TU = "UnitInfo_TU";
            public const string Replace_Void_By = "Replace_Void_By";
            public const string Suppress_Void_Message = "Suppress_Void_Message";
            public const string MultiplyMonetaryBy = "MultiplyMonetaryBy";

            public const string UnitInfo_XXX = "UnitInfo_XXX";

            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(File, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = DefinitionAdmin.MANY,
                    description = "Name of text file to write the output to (the extension .txt can be omitted)."
                });
                fun.par.Add(Var, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VAR,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "Name of a variable to be included in the output."
                });
                fun.par.Add(StringVar, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.STRINGVAR,
                    minCount = 0,
                    maxCount = DefinitionAdmin.MANY, 
                    description = "Name of a string variable to be included in the output."
                });
                fun.par.Add(IL, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "Name of an incomelist to be included in the output.\nThe total value of the incomelist is outputted (see parameter DefIL for outputting components)."
                });
                fun.par.Add(VarGroup, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "Regular expression that describes a group of variables to be included in the output, where * stands for any character and ? stands for one arbitrary character (e.g. b* includes all variables starting with b)."
                });
                fun.par.Add(ILGroup, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "Regular expression that describes a group of incomelists to be included in the output, where * stands for any character and ? stands for one arbitrary character (e.g. ils* for all system incomelists)."
                });
                fun.par.Add(DefIL, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.IL,
                    minCount = 0, maxCount = DefinitionAdmin.MANY,
                    description = "Name of an incomelist to be included in the output.\nThe content of the incomelist is outputted, i.e. the variables contained in the incomelist (see parameter IL for outputting the total value)."
                });
                fun.par.Add(nDecimals, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 2.0,
                    description = "Number of decimals of monetary variables to show in output.\nValues with more decimals are rounded."
                });
                fun.par.Add(Append, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false,
                    description = "If set to yes: any existing content of the output file is removed.\nIf set to no: output is added to any existing content of the output file."
                });
                fun.parGroups.Add(new DefinitionAdmin.ParGroup()
                {
                    groupName = UnitInfo_XXX,
                    minCount = 0, maxCount = DefinitionAdmin.MANY
                });
                fun.parGroups.Last().par.Add(UnitInfo_TU, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TU,
                    minCount = 1, maxCount = 1,
                    description = "Assessment unit for which UnitInfo_Id parameters apply.\nNote, that outputting unit info variables usually only makes sense if TAX_UNIT is set to an individual assessment unit."
                });
                fun.parGroups.Last().par.Add(UnitInfo_Id, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 1, maxCount = DefinitionAdmin.MANY,
                    categValues = new List<string>()
                    {
                        Value.UNITINFO_HEADID,
                        Value.UNITINFO_ISPARTNER,
                        Value.UNITINFO_ISDEPENDENTCHILD, Value.UNITINFO_ISDEPCHILD,
                        Value.UNITINFO_ISOWNCHILD,
                        Value.UNITINFO_ISOWNDEPENDENTCHILD, Value.UNITINFO_ISOWNDEPCHILD,
                        Value.UNITINFO_ISDEPPARENT,
                        Value.UNITINFO_ISDEPRELATIVE,
                        Value.UNITINFO_ISLONEPARENT,
                        Value.UNITINFO_ISCOHABITING,
                        Value.UNITINFO_ISWITHPARTNER,
                        Value.UNITINFO_ISPARENT,
                        Value.UNITINFO_ISPARENTOFDEPCHILD,
                        Value.UNITINFO_ISINEDUCATION,
                        Value.UNITINFO_ISLOOSEDEPCHILD,
                        Value.UNITINFO_ISLONEPARENTOFDEPCHILD,
                        Value.UNITINFO_NPERSINUNIT,
                        Value.UNITINFO_NDEPCHILDRENINTU,
                        Value.UNITINFO_NDEPPARENTSINTU,
                        Value.UNITINFO_NDEPRELATIVESINTU
                    },
                    description = "The UnitInfo parameters allow the determination of the 'status' of single members within the assessment unit specified by UnitInfo_TU.\nPossible values of UnitInfo_Id are:\n- HeadID: the output includes PersonId of the unit's Head.\n- IsPartner: the output includes a 0/1-variable for being Partner.\n- IsDependentChild (IsDepChild): the output includes a 0/1-variable for being a dependent child.\n- IsOwnChild: the output includes a 0/1-variable for being an own child.\n- IsOwnDependentChild (IsOwnDepChild): the output includes a 0/1-variable for being an own dependent child.\n- IsDepParent: the output includes a 0/1-variable for being a dependent parent.\n- IsDepRelative: the output includes a 0/1-variable for being a dependent relative.\n- IsLoneParent: the output includes a 0/1-variable for being a lone parent."
                });
                fun.par.Add(Replace_Void_By, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1,
                    description = "Amount to be used for 'undefined' in the output.\nNote that the default 'void' value is 0.0000000000001."
                });
                fun.par.Add(Suppress_Void_Message, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false,
                    description = "If set to yes, the warning for an 'undefined' variable is suppressed."
                });
                fun.par.Add(MultiplyMonetaryBy, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.FORMULA,
                    minCount = 0, maxCount = 1,
                    description = "All monetary values are multiplied by this factor.\n(Allows e.g. for the specification of a special exchange rate)."
                });
            }
        }
    }
}
