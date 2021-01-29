using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal partial class FunDefTU : FunOutOfSpineBase
    {
        // This is the function that composes the actual TUs
        internal List<List<Person>> ComposeTUs(HH hh, out bool _isStatic)
        {
            // first check if we need to initiate the local objects
            PrepareLocals();

            List<List<Person>> tus = new List<List<Person>>();
            _isStatic = isStatic; // if static, TUs can be stored and do not need to be recreated on next use

            // if type is INDIVIDUAL, then create a TU for each person
            if (tuType == DefPar.Value.TUTYPE_IND)
            {
                for (byte i = 0; i < hh.GetPersonCount(); ++i)
                {
                    double headPersonID = hh.GetPersonValue(indexPersonId, i);
                    List<Person> temp_tu = new List<Person>() { new Person(i) { tuHeadID = headPersonID, isHead = true } };
                    PopulateTUMembersInfo(temp_tu, hh, 0, headPersonID);    // isLoneParent is not filled, but is impossible in Individual households anyway...
                    tus.Add(temp_tu);
                }
                return tus;
            }
            else // else type is HOUSEHOLD or SUBGROUP
            {
                // compose a dictionary of all members, to hold calculated incomes & ages
                Dictionary<byte, Person> headCandidates = new Dictionary<byte, Person>();
                for (byte i = 0; i < hh.personVarList.Count; i++) 
                    headCandidates.Add(i, new Person(i));

                if (tuType == DefPar.Value.TUTYPE_SUBGROUP) // if type is SUBGROUP, create TUs until you run out of HH members
                {
                    while (headCandidates.Count > 0)
                    {
                        byte head = GetHead(hh, headCandidates);    // first get the head from the unassigned HH members
                        double headPersonID = hh.GetPersonValue(indexPersonId, head);
                        List<Person> tu = new List<Person>();

                        // create a TU with all unassigned members and this head - this will be used for calculating Person attributes (e.g. isDepChild etc)
                        List<Person> temp_tu = MakeTempTU(headCandidates, hh, head);

                        // populate the Person attributes, except for isLoneParent (see below)
                        PopulateTUMembersInfo(temp_tu, hh, headCandidates.ElementAt(0).Key, headPersonID);

                        // add the head to the new TU
                        tu.Add(temp_tu[0]);     // head should always be first
                        temp_tu.RemoveAt(0);    // remove it from the temp_tu

                        // add the rest of the members
                        foreach (string member_type in members)
                        {
                            List<Person> ps = new List<Person>();
                            switch (member_type)
                            {
                                case DefPar.DefTu.MEMBER_TYPE.DEPCHILD:
                                    ps = temp_tu.Where(p => p.isDepChild).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.DEPPARENT:
                                    ps = temp_tu.Where(p => p.isDepParent).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.DEPRELATIVE:
                                    ps = temp_tu.Where(p => p.isDepRelative).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.LOOSEDEPCHILD:
                                    ps = temp_tu.Where(p => p.isLooseDepChild).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.OWNCHILD:
                                    ps = temp_tu.Where(p => p.isOwnChild).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.OWNDEPCHILD:
                                    ps = temp_tu.Where(p => p.isOwnDepChild).ToList(); break;
                                case DefPar.DefTu.MEMBER_TYPE.PARTNER:
                                    ps = temp_tu.Where(p => p.isPartner).ToList(); break;
                                default:
                                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                                    { isWarning = false, runTimeErrorId = description.Get(), message = $"{description.Get()}: invalid member type found '{member_type}'" });
                                break;
                            }
                            foreach (Person p in ps)
                            { 
                                tu.Add(p);
                                temp_tu.Remove(p);
                            }
                        }

                        // then, if required, add the DepOfDep 
                        if (assignPartnerOfDependents || assignDepChOfDependents)
                        {
                            // in the old executable we only check remaining members in order
                            // in EM3, should we allow members to be added in mixed order too?
                            byte i = 0;
                            // for each member of the TU
                            while (i < tu.Count)
                            {
                                // if this person is a dependent, check if members can be attached
                                if (tu[i].isDepChild || tu[i].isDepParent || tu[i].isDepRelative || tu[i].isLooseDepChild || tu[i].isOwnDepChild || tu[i].isDepMember)
                                {
                                    double personId = hh.GetPersonValue(indexPersonId, tu[i].indexInHH);
                                    int j = 0;
                                    // check the unassigned members
                                    while (j < temp_tu.Count)
                                    {
                                        bool add = false;
                                        if (assignPartnerOfDependents && (personId == hh.GetPersonValue(indexPartnerId, temp_tu[j].indexInHH))) add = true;
                                        else if (assignDepChOfDependents && temp_tu[j].isDepChild &&
                                                    (personId == hh.GetPersonValue(indexFatherId, temp_tu[j].indexInHH)
                                                    || personId == hh.GetPersonValue(indexMotherId, temp_tu[j].indexInHH))) add = true;
                                        if (add)
                                        {
                                            temp_tu[j].isDepMember = true;  // flag this as a dependent member
                                            tu.Add(temp_tu[j]);             // add it to the TU
                                            temp_tu.RemoveAt(j);            // and remove it from the unassigned
                                        }
                                        else j++;    // advance to the next unassigned member
                                    }
                                }
                                ++i;    // advance to the next person in the TU
                            }
                        }

                        // we need to run loneParentCond only on people already added to the TU!
                        Dictionary<byte, bool> allLoneParent = loneParentCond.GetTUValues(hh, tu, true);
                        for (byte i = 0; i < tu.Count; i++) tu[i].isLoneParent = allLoneParent[tu[i].indexInHH];

                        // the TU is complete, so add it to the list of TUs
                        tus.Add(tu);

                        // finally remove anyone that was added in this TU from the headCandidates of the next round
                        foreach (Person p in tu) headCandidates.Remove(p.indexInHH);
                    }
                }
                else
                {
                    byte head = GetHead(hh, headCandidates);
                    double headPersonID = hh.GetPersonValue(indexPersonId, head);
                    List<Person> tu = new List<Person>();
                    tu.Add(new Person(head) { isHead = true, tuHeadID = headPersonID });   // head should always be first
                    for (byte i = 0; i < hh.GetPersonCount(); ++i)  // then add the rest
                        if (i != head) tu.Add(new Person(i));
                    PopulateTUMembersInfo(tu, hh, tu[0].indexInHH, headPersonID);
                    // we need to run loneParentCond only on people already added to the TU!
                    Dictionary<byte, bool> allLoneParent = loneParentCond.GetTUValues(hh, tu, true);
                    for (byte i = 0; i < tu.Count; i++) tu[i].isLoneParent = allLoneParent[tu[i].indexInHH];
                    tus.Add(tu);
                }
            }
            return tus;
        }

        private void PopulateTUMembersInfo(List<Person> temp_tu, HH hh, double firstPersonIndexInHH, double headPersonID, bool produceWarnings = true)
        {
            Dictionary<byte, bool> allPartner = partnerCond.GetTUValues(hh, temp_tu, true);
            bool gotSinglePartner = false;  // this is used to allow only a single partner (unless "MultiplePartners" is "Allow") 
                                            // first, calculate the query values for all members. This is not at all efficient, but it is what the old executable does. Making it more efficient would cut on available information.
            for (byte i = 0; i < temp_tu.Count; i++)
            {
                if (allPartner[temp_tu[i].indexInHH])    // the head (which is always first) can never be partner
                {
                    if (gotSinglePartner)   // only one partner is allowed and it was already found!
                    {
                        // if not "ignore" (i.e. if missing or "warn" - "allow" should never bring you to this path)
                        // parameter produceWarnings is also used here, to make sure you don't get the same warning twice, when looking for the head and when building the TU
                        if (produceWarnings && multiplePartners != DefPar.Value.MULTIPLE_PARTNERS_IGNORE)  
                            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                            { isWarning = true, message = $"{description.Get()}: more than one possible partner found for assessment unit '{tu_name}', idperson: '{hh.GetPersonValue(indexPersonId, temp_tu[i].indexInHH)}'" });
                    }
                    else
                    {
                        temp_tu[i].isPartner = true;
                        gotSinglePartner = multiplePartners != DefPar.Value.MULTIPLE_PARTNERS_ALLOW;  // only set this to true if indeed only one partner is allowed
                        if (temp_tu[0].partnerIndexInHH == byte.MaxValue) temp_tu[0].partnerIndexInHH = temp_tu[i].indexInHH;   // but for the purposes of partner:yem, always keep the first partner found
                    }
                }
                else
                    temp_tu[i].isPartner = false;
            }
            // we need to get each condition in turn and pass the values to the members
            //(ideally we would be able to have a single loop and get all conditions of first person, then second etc. but this is not possible with GetTUValues)
            Dictionary<byte, bool> allOwnChild = ownChildCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isOwnChild = allOwnChild[temp_tu[i].indexInHH];

            Dictionary<byte, bool> allDepChild = depChildCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isDepChild = (!noChildIfHead || !temp_tu[i].isHead) && (!noChildIfPartner || !temp_tu[i].isPartner) && allDepChild[temp_tu[i].indexInHH];

            Dictionary<byte, bool> allOwnDepChild = ownDepChildCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isOwnDepChild = allOwnDepChild[temp_tu[i].indexInHH];

            Dictionary<byte, bool> allDepParent = depParentCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isDepParent = allDepParent[temp_tu[i].indexInHH];

            Dictionary<byte, bool> allLooseDepChild = looseDepChildCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isLooseDepChild = allLooseDepChild[temp_tu[i].indexInHH];

            Dictionary<byte, bool> allDepRelative = depRelativeCond.GetTUValues(hh, temp_tu, true);
            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].isDepRelative = allDepRelative[temp_tu[i].indexInHH];

            // note: we need to run loneParentCond only on people already added to the TU! so it cannot be done here

            for (byte i = 0; i < temp_tu.Count; i++) temp_tu[i].tuHeadID = headPersonID;
        }

        private List<Person> MakeTempTU(Dictionary<byte, Person> tuMembers, HH hh, byte head)
        {
            // create a TU with all possible members and this head, 
            List<Person> temp_tu = new List<Person>();
            tuMembers[head].isHead = true;   // set head 
            temp_tu.Add(tuMembers[head]);    // head should always be first
            foreach (byte i in tuMembers.Keys)      // then add the rest
                if (i != head)
                    temp_tu.Add(tuMembers[i]);
            return temp_tu;
        }
    }
}
