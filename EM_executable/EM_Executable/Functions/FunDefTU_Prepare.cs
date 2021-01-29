using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class FunDefTU : FunOutOfSpineBase
    {

        protected override void PreprocessParameters()
        {
            isStatic = GetParBoolValueOrDefault(DefFun.DefTu, DefPar.DefTu.IsStatic);

            // get the MEMBERS parameter
            string memberString = GetParBaseValueOrDefault(DefFun.DefTu, DefPar.DefTu.Members);
            members = memberString == null ? new List<string>() : memberString.Split('&').Select(x => x.Trim().ToLower()).ToList();

            tu_name = GetUniquePar<ParBase>(DefPar.DefTu.Name).xmlValue;

            // get all the conditions and replace the "{default}"
            extHeadCond = GetUniquePar<ParCond>(DefPar.DefTu.ExtHeadCond);
            if (extHeadCond != null) extHeadCond.xmlValue = extHeadCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.EXTHEAD);

            partnerCond = GetUniquePar<ParCond>(DefPar.DefTu.PartnerCond);
            if (partnerCond != null) partnerCond.xmlValue = partnerCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.PARTNER);

            depChildCond = GetUniquePar<ParCond>(DefPar.DefTu.DepChildCond);
            if (depChildCond != null) depChildCond.xmlValue = depChildCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.DEPCHILD);

            ownChildCond = GetUniquePar<ParCond>(DefPar.DefTu.OwnChildCond);
            if (ownChildCond != null) ownChildCond.xmlValue = ownChildCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.OWNCHILD);

            ownDepChildCond = GetUniquePar<ParCond>(DefPar.DefTu.OwnDepChildCond);
            if (ownDepChildCond != null) ownDepChildCond.xmlValue = ownDepChildCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.OWNDEPCHILD);

            looseDepChildCond = GetUniquePar<ParCond>(DefPar.DefTu.LooseDepChildCond);
            if (looseDepChildCond != null) looseDepChildCond.xmlValue = looseDepChildCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.LOOSEDEPCHILD);

            depParentCond = GetUniquePar<ParCond>(DefPar.DefTu.DepParentCond);
            if (depParentCond != null) depParentCond.xmlValue = depParentCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.DEPPARENT);

            depRelativeCond = GetUniquePar<ParCond>(DefPar.DefTu.DepRelativeCond);
            if (depRelativeCond != null) depRelativeCond.xmlValue = depRelativeCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.DEPRELATIVE);

            loneParentCond = GetUniquePar<ParCond>(DefPar.DefTu.LoneParentCond);
            if (loneParentCond != null) loneParentCond.xmlValue = loneParentCond.xmlValue.Replace(DefPar.DefTu.DEFAULT_CONDITION.DEFAULT, DefPar.DefTu.DEFAULT_CONDITION.LONEPARENT);
        }

        protected override void PrepareNonCommonPar()
        {
            ParCateg parType = GetUniquePar<ParCateg>(DefPar.DefTu.Type);
            if (parType != null) tuType = parType.GetCateg();

            if (tuType != DefPar.Value.TUTYPE_IND)
            {
                // get the INCOME used to define the head
                headDefInc = GetUniquePar<ParVarIL>(DefPar.DefTu.HeadDefInc);
                if (headDefInc == null) // if no income defined, use ils_origy as default
                {
                    headDefInc = new ParVarIL(infoStore);
                    headDefInc.xmlValue = infoStore.country.sys.headDefInc == string.Empty ? DefVarName.ILSORIGY : infoStore.country.sys.headDefInc;
                    headDefInc.description = description;
                    headDefInc.CheckAndPrepare(this);
                }

                // get all the conditions and set the defaults if they did not exist before
                if (extHeadCond == null)
                {
                    extHeadCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.EXTHEAD, description = description };
                    extHeadCond.CheckAndPrepare(this);
                }
            }

            if (partnerCond == null)
            {
                partnerCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.PARTNER, description = description };
                partnerCond.CheckAndPrepare(this);
            }

            if (depChildCond == null)
            {
                depChildCond = new ParCond(infoStore) { xmlValue = "{0}", description = description };
                depChildCond.CheckAndPrepare(this);
            }

            if (ownChildCond == null)
            {
                ownChildCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.OWNCHILD, description = description };
                ownChildCond.CheckAndPrepare(this);
            }

            if (ownDepChildCond == null)
            {
                ownDepChildCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.OWNDEPCHILD, description = description };
                ownDepChildCond.CheckAndPrepare(this);
            }

            if (looseDepChildCond == null)
            {
                looseDepChildCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.LOOSEDEPCHILD, description = description };
                looseDepChildCond.CheckAndPrepare(this);
            }

            if (depParentCond == null)
            {
                depParentCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.DEPPARENT, description = description };
                depParentCond.CheckAndPrepare(this);
            }

            if (depRelativeCond == null)
            {
                depRelativeCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.DEPRELATIVE, description = description };
                depRelativeCond.CheckAndPrepare(this);
            }

            if (loneParentCond == null)
            {
                loneParentCond = new ParCond(infoStore) { xmlValue = DefPar.DefTu.DEFAULT_CONDITION.LONEPARENT, description = description };
                loneParentCond.CheckAndPrepare(this);
            }

            // get all the booleans and set the values
            ParBool noChildIfHeadPar = GetUniquePar<ParBool>(DefPar.DefTu.NoChildIfHead);
            noChildIfHead = noChildIfHeadPar != null && noChildIfHeadPar.GetBoolValue();
            ParBool noChildIfPartnerPar = GetUniquePar<ParBool>(DefPar.DefTu.NoChildIfPartner);
            noChildIfPartner = noChildIfPartnerPar != null && noChildIfPartnerPar.GetBoolValue();
            ParBool assignDepChOfDependentsPar = GetUniquePar<ParBool>(DefPar.DefTu.AssignDepChOfDependents);
            assignDepChOfDependents = assignDepChOfDependentsPar != null && assignDepChOfDependentsPar.GetBoolValue();
            ParBool assignPartnerOfDependentsPar = GetUniquePar<ParBool>(DefPar.DefTu.AssignPartnerOfDependents);
            assignPartnerOfDependents = assignPartnerOfDependentsPar != null && assignPartnerOfDependentsPar.GetBoolValue();
            ParBool stopIfNoHeadFoundPar = GetUniquePar<ParBool>(DefPar.DefTu.StopIfNoHeadFound);
            stopIfNoHeadFound = stopIfNoHeadFoundPar != null && stopIfNoHeadFoundPar.GetBoolValue();
            ParCateg multiplePartnersPar = GetUniquePar<ParCateg>(DefPar.DefTu.MultiplePartners);
            multiplePartners = multiplePartnersPar == null ? string.Empty : multiplePartnersPar.GetCateg();
        }

        private void PrepareLocals()
        {
            if (indexDag == -1)
            {
                lock (localLock)    // make sure you don't have two threads creating this at the same time
                {
                    if (indexDag == -1)
                    {
                        // prepare the head income & condition
                        if (tuType != DefPar.Value.TUTYPE_IND)
                        {
                            headDefInc.CheckAndPrepare(this);
                            headDefInc.ReplaceVarNameByIndex();
                            extHeadCond.CheckAndPrepare(this);
                            extHeadCond.ReplaceVarNameByIndex();
                        }

                        // prepare the remaining conditions
                        partnerCond.CheckAndPrepare(this);
                        partnerCond.ReplaceVarNameByIndex();
                        depChildCond.CheckAndPrepare(this);
                        depChildCond.ReplaceVarNameByIndex();
                        ownChildCond.CheckAndPrepare(this);
                        ownChildCond.ReplaceVarNameByIndex();
                        ownDepChildCond.CheckAndPrepare(this);
                        ownDepChildCond.ReplaceVarNameByIndex();
                        looseDepChildCond.CheckAndPrepare(this);
                        looseDepChildCond.ReplaceVarNameByIndex();
                        depParentCond.CheckAndPrepare(this);
                        depParentCond.ReplaceVarNameByIndex();
                        depRelativeCond.CheckAndPrepare(this);
                        depRelativeCond.ReplaceVarNameByIndex();
                        loneParentCond.CheckAndPrepare(this);
                        loneParentCond.ReplaceVarNameByIndex();

                        // save various indices so that we don't have to look for them every time
                        indexPartnerId = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPARTNER);
                        indexFatherId = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDFATHER);
                        indexMotherId = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDMOTHER);
                        indexPersonId = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON);
                        indexDag = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DAG);  // this must always be last to avoid cross-thread bugs!
                    }
                }
            }
        }
    }
}
