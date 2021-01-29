using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class FunDefTU : FunOutOfSpineBase
    {
        private byte GetHead(HH hh, Dictionary<byte, Person> headCandidates)
        {
            List<Person> heads = new List<Person>();
            // first keep only candidates that satisfy the ExtHeadCond

            // get temp HH values for the head condition, assuming the first candidate as the head
            List<Person> temp_tu = headCandidates.Values.ToList();
            PopulateTUMembersInfo(temp_tu, hh, temp_tu[0].indexInHH, hh.GetPersonValue(indexPersonId, temp_tu[0].indexInHH), false);
            
            Dictionary<byte, bool> headElig = extHeadCond.GetTUValues(hh, temp_tu);
            foreach (byte i in headCandidates.Keys)
            {
                if (headElig[i])
                {
                    if (!headCandidates[i].headCalculation.calculated)
                    {
                        headCandidates[i].headCalculation.calculated = true;
                        headCandidates[i].headCalculation.income = headDefInc.GetValue(hh, new List<Person>() { new Person((byte)i) });
                        headCandidates[i].headCalculation.age = hh.personVarList[i][indexDag];
                    }
                    heads.Add(headCandidates[i]);
                }
            }
            if (heads.Count == 0)   // if no candidate passed, then depending on StopIfNoHeadFound, either issue an error, or repeat the above, ignoring ExtHeadCond
            {
                if (stopIfNoHeadFound)
                {
                    double idhh = hh.personVarList[0][infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDHH)];
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: Household {idhh}: extended head conidtion '{extHeadCond.xmlValue}' rules out all assessment unit members for being head" });
                }
                else
                {
                    foreach (byte i in headCandidates.Keys)
                    {
                        if (!headCandidates[i].headCalculation.calculated)
                        {
                            headCandidates[i].headCalculation.calculated = true;
                            headCandidates[i].headCalculation.income = headDefInc.GetValue(hh, new List<Person>() { new Person((byte)i) });
                            headCandidates[i].headCalculation.age = hh.personVarList[i][indexDag];
                        }
                        heads.Add(headCandidates[i]);
                    }
                }
            }
            // if more than one individuals satisfy the ExtHeadCond, check the income
            if (heads.Count > 1)
            {
                double maxInc = heads.Select(x => x.headCalculation.income).Max();   // get the max income
                for (int i = heads.Count - 1; i >= 0; i--)
                    if (heads[i].headCalculation.income < maxInc) heads.RemoveAt(i); // and remove anyone poorer
            }
            // if more than one individuals have the same income, check the age
            if (heads.Count > 1)
            {
                double maxAge = heads.Select(x => x.headCalculation.age).Max();   // get the max age
                for (int i = heads.Count - 1; i >= 0; i--)
                    if (heads[i].headCalculation.age < maxAge) heads.RemoveAt(i); // and remove anyone younger
            }
            // if no head found, return error
            if (heads.Count == 0)
            {
                double idhh = hh.personVarList[0][infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDHH)];
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: Could not determine a valid head for hh {idhh}" });
            }
            // else return the first head found
            return heads[0].indexInHH;
        }
    }
}
