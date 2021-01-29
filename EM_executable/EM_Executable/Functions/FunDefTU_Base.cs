using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class FunDefTU : FunOutOfSpineBase
    {
        internal FunDefTU(InfoStore infoStore) : base(infoStore) { }

        internal bool isPrepared = false; // to make sure DefTU is only prepared once (see ParTU for further explanation)

        internal bool isStatic; // new parameter, indicating whether TU needs to be recreated on each use
                                // see comments in FunUpateTU.cs
        private string tuType = DefPar.Value.TUTYPE_IND;

        // store local variables for efficiency
        int indexDag = -1;
        int indexPartnerId = -1;
        int indexFatherId = -1;
        int indexMotherId = -1;
        int indexPersonId = -1;
        ParVarIL headDefInc;
        List<string> members;
        ParCond extHeadCond;
        ParCond partnerCond;
        ParCond depChildCond;
        ParCond ownChildCond;
        ParCond ownDepChildCond;
        ParCond looseDepChildCond;
        ParCond depParentCond;
        ParCond depRelativeCond;
        ParCond loneParentCond;
        bool stopIfNoHeadFound;
        bool noChildIfHead;
        bool noChildIfPartner;
        bool assignDepChOfDependents;
        bool assignPartnerOfDependents;
        string multiplePartners;
        string tu_name;

        object localLock = new object();        // used for thread-safety

        internal override void ProvideIndexInfo()
        {
            infoStore.RegisterTU(GetUniquePar<ParBase>(DefPar.DefTu.Name).xmlValue, this);
        }

        internal override void ReplaceVarNameByIndex()
        {
            if (isPrepared) base.ReplaceVarNameByIndex(); // a not used TU will not be prepared (see ParTU)
        }
    }
}
