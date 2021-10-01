using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunDropKeepUnit : FunOutOfSpineBase
    {
        private bool keep = true;
        private ParCond parCond = null;
        private string who = DefPar.Value.WHO_ONE;
        private string tuName = string.Empty;

        internal FunDropKeepUnit(InfoStore infoStore, bool _keep) : base(infoStore) { keep = _keep; }

        protected override void PrepareNonCommonPar()
        {
            parCond = GetUniquePar<ParCond>(keep ? DefPar.KeepUnit.Keep_Cond : DefPar.DropUnit.Drop_Cond);
            ParCateg parWho = GetUniquePar<ParCateg>(keep ? DefPar.KeepUnit.Keep_Cond_Who : DefPar.DropUnit.Drop_Cond_Who);
            if (parWho != null) who = parWho.GetCateg();

            ParTU parTU = GetUniquePar<ParTU>(DefPar.Common.TAX_UNIT);
            if (parTU != null) tuName = parTU.name;
        }

        protected override void DoFunWork()
        {
            List<HH> hhToRemove = new List<HH>();
            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                List<byte> dropKeep = new List<byte>();
                foreach (List<Person> tu in hh.GetTUs(tuName))
                    if (FunInSpineBase.IsCondMetByTU(hh, tu, parCond, who)) foreach (Person p in tu) dropKeep.Add(p.indexInHH);

                bool dropWholeHH, noChange;
                if (keep) Keep(hh, dropKeep, out dropWholeHH, out noChange); else Drop(hh, dropKeep, out dropWholeHH, out noChange);

                if (noChange) continue;
                if (dropWholeHH) { hhToRemove.Add(hh); continue; }

                for (int i = hh.GetPersonCount() - 1; i >= 0; --i)
                {
                    bool dk = dropKeep.Contains(Convert.ToByte(i));
                    if (keep ? !dk : dk)
                    {
                        hh.personVarList.RemoveAt(i);
                        hh.personStringVarList.RemoveAt(i);
                    }
                }
                infoStore.hhAdmin.UpdateAllTUs(hh); // if the composition of the HH was changed, the taxunits are no longer valid
            }
            foreach (HH hh in hhToRemove) infoStore.hhAdmin.hhs.Remove(hh);
        }

        private void Keep(HH hh, List<byte> keep, out bool dropWholeHH, out bool noChange)
        {
            noChange = keep.Count == hh.GetPersonCount();
            dropWholeHH = keep.Count == 0;
            if (noChange || dropWholeHH) return;           
        }

        private void Drop(HH hh, List<byte> drop, out bool dropWholeHH, out bool noChange)
        {
            noChange = drop.Count == 0;
            dropWholeHH = drop.Count == hh.GetPersonCount();
            if (noChange || dropWholeHH) return;
        }
    }
}
