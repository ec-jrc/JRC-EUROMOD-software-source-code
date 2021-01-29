using System.Collections.Generic;

namespace EM_Common
{
    public static partial class DefPar
    {
        public static class DefTu
        {
            public const string Name = "Name";
            public const string IsStatic = "IsStatic"; // new parameter, indicating whether TU needs to be recreated on each use
                                                       // (e.g. child-definition depends on a variable that changes during run)
            public const string Type = "Type";
            public const string Members = "Members";
            public const string HeadDefInc = "HeadDefInc";
            public const string ExtHeadCond = "ExtHeadCond";
            public const string PartnerCond = "PartnerCond";
            public const string DepChildCond = "DepChildCond";
            public const string OwnChildCond = "OwnChildCond";
            public const string OwnDepChildCond = "OwnDepChildCond";
            public const string LooseDepChildCond = "LooseDepChildCond";
            public const string DepParentCond = "DepParentCond";
            public const string DepRelativeCond = "DepRelativeCond";
            public const string LoneParentCond = "LoneParentCond";
            public const string StopIfNoHeadFound = "StopIfNoHeadFound";
            public const string NoChildIfHead = "NoChildIfHead";
            public const string NoChildIfPartner = "NoChildIfPartner";
            public const string AssignDepChOfDependents = "AssignDepChOfDependents";
            public const string AssignPartnerOfDependents = "AssignPartnerOfDependents";
            public const string AllowMultiplePartners = "AllowMultiplePartners";
            public const string MultiplePartners = "MultiplePartners";
            public static class DEFAULT_CONDITION
            {
                public const string EXTHEAD = "(!isdepchild)";
                public const string PARTNER = "(head:idperson=idpartner)";
                public const string DEPCHILD = "(!isparent & idpartner<=0)";
                public const string OWNCHILD = "(head:idperson=idmother | head:idperson=idfather | partner:idperson=idmother | partner:idperson=idfather)";
                public const string OWNDEPCHILD = "(isownchild & isdepchild)";
                public const string LOOSEDEPCHILD = "(idmother=0 & idfather=0 & isdepchild)";
                public const string DEPPARENT = "(head:idmother=idperson | head:idfather=idperson | partner:idmother=idperson | partner:idfather=idperson)";
                public const string DEPRELATIVE = "(0)";
                public const string LONEPARENT = "(isparentofdepchild & idpartner<=0)";
                public const string DEFAULT = "default";
            }
            public class MEMBER_TYPE
            {
                public const string PARTNER = "partner";
                public const string OWNCHILD = "ownchild";
                public const string DEPCHILD = "depchild";
                public const string OWNDEPCHILD = "owndepchild";
                public const string LOOSEDEPCHILD = "loosedepchild";
                public const string DEPPARENT = "depparent";
                public const string DEPRELATIVE = "deprelative";

                public const string PARTNER_CAMEL_CASE = "Partner";
                public const string OWNCHILD_CAMEL_CASE = "OwnChild";
                public const string DEPCHILD_CAMEL_CASE = "DepChild";
                public const string OWNDEPCHILD_CAMEL_CASE = "OwnDepChild";
                public const string LOOSEDEPCHILD_CAMEL_CASE = "LooseDepChild";
                public const string DEPPARENT_CAMEL_CASE = "DepParent";
                public const string DEPRELATIVE_CAMEL_CASE = "DepRelative";
            }
        
            internal static void Add(DefinitionAdmin.Fun fun)
            {
                fun.par.Add(Name, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 1, maxCount = 1
                });
                fun.par.Add(IsStatic, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = true, // i.e. reflects the "old" behaviour
                    description = "If set to no, TU is recreated on each use, default = yes.\nSet to 'no' e.g. for a child-definition depending on a variable that changes during run."
                });
                fun.par.Add(Type, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 1, maxCount = 1,
                    categValues = new List<string>() { Value.TUTYPE_HH, Value.TUTYPE_IND, Value.TUTYPE_SUBGROUP }
                });
                fun.par.Add(Members, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.TEXT,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(HeadDefInc, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.VARorIL,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(ExtHeadCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(PartnerCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(DepChildCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(OwnChildCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(OwnDepChildCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(LooseDepChildCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(DepParentCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(DepRelativeCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(LoneParentCond, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CONDITION,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(StopIfNoHeadFound, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(NoChildIfHead, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(NoChildIfPartner, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(AssignDepChOfDependents, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(AssignPartnerOfDependents, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1
                });
                fun.par.Add(MultiplePartners, new DefinitionAdmin.Par()
                {
                    valueType = PAR_TYPE.CATEG,
                    minCount = 0, maxCount = 1,
                    categValues = new List<string>() { Value.MULTIPLE_PARTNERS_WARN, Value.MULTIPLE_PARTNERS_IGNORE, Value.MULTIPLE_PARTNERS_ALLOW },
                    description = "Allows customizing the behaviour when multiple partners are found in the data."
                });
            }
        }
    }
}
