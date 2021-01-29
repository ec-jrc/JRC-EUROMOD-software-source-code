using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunAddHHMembers : FunOutOfSpineBase
    {
        internal FunAddHHMembers(InfoStore infoStore) : base(infoStore) { }

        private class AddInstruction
        {
            // either we are adding either children or partners or "other" persons
            internal string addWho = DefPar.Value.ADDHHMEMBERS_OTHER;
            // for adding children and partners the (individual) condition to be the "triggering" person, i.e. the parent getting a child or the person getting a partner
            // for adding "other persons" a household condition that must be fulfill for adding the new person
            internal ParCond cond = null;
            // only relevant for adding children, if true, the child may have father and mother, otherwise only one of them
            internal bool isPartnerParent = true;
            // formulas to set any variables, e.g. dag = 3, dec = 1, ...
            internal Dictionary<string, ParFormula> prepVarDefinitions = new Dictionary<string, ParFormula>(); // key: var-name, value: formula (used in preparation)
            internal Dictionary<int, ParFormula> varDefinitions = new Dictionary<int, ParFormula>(); // key: var-index, value: same formula as above (used once index is available)
        }

        // there can be x such instructions in the function (organised via group)
        private List<AddInstruction> addInstructions = new List<AddInstruction>();
        private VarSpec flagVar = null;

        // indices of the variables that are required for adding the new persons
        int indexIDHH, indexIDPERSON, indexIDMOTHER, indexIDFATHER, indexIDPARTNER, indexDGN, indexDWT, indexDCT;
        int nStringVars = 0;

        protected override void PrepareNonCommonPar()
        {
            ParVar fv = GetUniquePar<ParVar>(DefPar.AddHHMembers.FlagVar);
            if (fv != null) flagVar = new VarSpec() { name = fv.name };

            foreach (var g in GetParGroups(DefPar.AddHHMembers.GROUP_MAIN))
            {
                var group = g.Value; int groupNo = g.Key;
                AddInstruction addInstruction = new AddInstruction();

                // first find out whether we are adding children or partners or other persons ...
                ParCateg parWho = GetUniqueGroupPar<ParCateg>(DefPar.AddHHMembers.Add_Who, group); if (parWho == null) continue; // compulsory parameter, error already issued
                addInstruction.addWho = parWho.GetCateg();

                // ... depending on that, check the add-condition
                ParCond parParent = GetUniqueGroupPar<ParCond>(DefPar.AddHHMembers.ParentCond, group);
                ParCond parPartner = GetUniqueGroupPar<ParCond>(DefPar.AddHHMembers.PartnerCond, group);
                ParCond parOther = GetUniqueGroupPar<ParCond>(DefPar.AddHHMembers.HHCond, group);
                string missing = string.Empty; string tooMuch = string.Empty;
                switch (addInstruction.addWho)
                {
                    case DefPar.Value.ADDHHMEMBERS_CHILD:
                        if (parParent != null) addInstruction.cond = parParent; else missing = DefPar.AddHHMembers.ParentCond;
                        if (parPartner != null) tooMuch = DefPar.AddHHMembers.PartnerCond + " ";
                        if (parOther != null) tooMuch = DefPar.AddHHMembers.HHCond;
                        ParBool parIsPP = GetUniqueGroupPar<ParBool>(DefPar.AddHHMembers.IsPartnerParent, group);
                        if (parIsPP != null) addInstruction.isPartnerParent = parIsPP.GetBoolValue();
                        break;
                    case DefPar.Value.ADDHHMEMBERS_PARTNER:
                        if (parParent != null) tooMuch = DefPar.AddHHMembers.ParentCond + " ";
                        if (parPartner != null) addInstruction.cond = parPartner; else missing = DefPar.AddHHMembers.PartnerCond;
                        if (parOther != null) tooMuch = DefPar.AddHHMembers.HHCond;
                        break;
                    case DefPar.Value.ADDHHMEMBERS_OTHER:
                        if (parParent != null) tooMuch = DefPar.AddHHMembers.ParentCond + " ";
                        if (parPartner != null) tooMuch = DefPar.AddHHMembers.PartnerCond;
                        if (parOther != null) addInstruction.cond = parOther; else missing = DefPar.AddHHMembers.HHCond; 
                        break;
                    default: continue; // error is caught by general error handling
                }
                if (missing != string.Empty) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{description.Get()} (group {groupNo}): {DefPar.AddHHMembers.Add_Who}={addInstruction.addWho} requires parameter {missing}" });
                if (tooMuch != string.Empty) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()} (group {groupNo}): {DefPar.AddHHMembers.Add_Who}={addInstruction.addWho} does not require {tooMuch}" });
                
                // here we are only gathering the variables that are to be set, but do not evaluate whether they exist
                // note: this function does not "register" variables, i.e. if a variable is not used anywhere else, it cannot be set
                foreach (ParFormula parVarDefinition in GetPlaceholderGroupPar<ParFormula>(group))
                {
                    string varName = parVarDefinition.description.GetParName();
                    if (addInstruction.prepVarDefinitions.ContainsKey(varName))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{parVarDefinition.description.Get()}: double definition of variable {varName}" });
                        continue;
                    }
                    if (varName == DefVarName.IDHH || varName == DefVarName.IDPERSON || varName == DefVarName.DWT || varName == DefVarName.DCT)
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{parVarDefinition.description.Get()}: variable {varName} is system-set and cannot be changed" });
                        continue;
                    }
                    addInstruction.prepVarDefinitions.Add(varName, parVarDefinition);
                }
                addInstructions.Add(addInstruction);
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();

            // exchange variable-names by variable-indices (move them to the other dictionary) and check if the variables exist
            foreach (AddInstruction addInstruction in addInstructions)
                foreach (var prepVarDefinition in addInstruction.prepVarDefinitions)
                {
                    if (infoStore.operandAdmin.Exists(prepVarDefinition.Key) && infoStore.operandAdmin.GetParType(prepVarDefinition.Key) == DefPar.PAR_TYPE.VAR)
                        addInstruction.varDefinitions.Add(infoStore.operandAdmin.GetIndexInPersonVarList(prepVarDefinition.Key), prepVarDefinition.Value);
                    else infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{prepVarDefinition.Value.description.Get()}: variable {prepVarDefinition.Key} does not exist" });
                }

            if (flagVar != null) flagVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(flagVar.name);

            // prepare the indices of the variables that are required for adding the new persons
            indexIDHH = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDHH);
            indexIDPERSON = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON);
            indexIDMOTHER = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDMOTHER);
            indexIDFATHER = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDFATHER);
            indexIDPARTNER = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPARTNER);
            indexDGN = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DGN);
            indexDWT = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DWT);
            indexDCT = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DCT);
            nStringVars = infoStore.hhAdmin.GetFirstHH().personStringVarList.Count;
        }

        protected override void DoFunWork()
        {
            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                // we first gather all new persons to add them together at the end, to not preliminary change the HH
                List<List<double>> newPersons = new List<List<double>>();

                double maxPersonId = 0; // find out the max-idPerson
                foreach (List<double> hhMem in hh.personVarList) if (hhMem[indexIDPERSON] > maxPersonId) maxPersonId = hhMem[indexIDPERSON];

                // the dummy-HH-TU is necessary for evaluating the formuals and the HH_COND for adding "other" persons
                var dummyTU = hh.GetTUs(HHAdmin.DUMMY_HOUSEHOLD_TU)[0];

                foreach (AddInstruction addInstruction in addInstructions)
                {
                    if (addInstruction.addWho == DefPar.Value.ADDHHMEMBERS_CHILD || addInstruction.addWho == DefPar.Value.ADDHHMEMBERS_PARTNER)
                    {
                        for (byte p = 0; p < hh.GetPersonCount(); ++p) // there are as may children/partners added as there are
                        {                                              // "triggering" persons (i.e. persons fulfilling the condition)
                            if (!addInstruction.cond.GetPersonValue(hh, new Person(p))) continue;

                            // ADDING CHILDREN
                            if (addInstruction.addWho == DefPar.Value.ADDHHMEMBERS_CHILD)
                            {
                                double motherId = -1, fatherId = -1;
                                // with the id and gender of the "triggering" person we can set the first partent as either mother or father
                                bool female = hh.personVarList[p][indexDGN] == 0;
                                if (female) motherId = hh.personVarList[p][indexIDPERSON]; else fatherId = hh.personVarList[p][indexIDPERSON];

                                // if the partner of the first parent should be the other parent, we need to have the gender of the partner,
                                // as a child can only have on mother and one father (due to have only idMother and idFather at our disposal)
                                if (addInstruction.isPartnerParent)
                                {
                                    double otherParentId = hh.personVarList[p][indexIDPARTNER];
                                    if (otherParentId > 0)
                                    {
                                        bool? partnerFemale = null;
                                        foreach (List<double> hhMem in hh.personVarList) if (hhMem[indexIDPERSON] == otherParentId) partnerFemale = hhMem[indexDGN] == 0;
                                        if (partnerFemale != null && partnerFemale != female)
                                        { if (partnerFemale == true) motherId = otherParentId; else fatherId = otherParentId; }
                                    }
                                }
                                // once one or two parents are defined, the child can be added
                                newPersons.Add(MakeNewPerson(hh, ++maxPersonId, dummyTU, addInstruction.varDefinitions, motherId, fatherId));
                            }
                            // ADDING PARTNERS
                            else
                            {
                                newPersons.Add(MakeNewPerson(hh, ++maxPersonId, dummyTU, addInstruction.varDefinitions, -1, -1,
                                               hh.personVarList[p][indexIDPERSON])); // the new person is the partner of the "triggering" person ...
                                // ... but the "triggering" person is only the partner of the new person, if (s)he does not yet have a partner
                                if (hh.personVarList[p][indexIDPARTNER] <= 0) hh.personVarList[p][indexIDPARTNER] = maxPersonId;
                            }
                        }
                    }
                    // ADDING OTHER PERSONS
                    if (addInstruction.addWho == DefPar.Value.ADDHHMEMBERS_OTHER)
                    {
                        if (FunInSpineBase.IsCondMetByTU(hh, dummyTU, addInstruction.cond, DefPar.Value.WHO_ONE))
                            newPersons.Add(MakeNewPerson(hh, ++maxPersonId, dummyTU, addInstruction.varDefinitions));
                    }
                }

                foreach (List<double> newPerson in newPersons)
                {
                    if (flagVar != null) newPerson[flagVar.index] = 1;
                    hh.personVarList.Add(newPerson);

                    List<string> stringVarList = new List<string>(nStringVars); for (int i = 0; i < nStringVars; ++i) stringVarList.Add(string.Empty);
                    hh.personStringVarList.Add(stringVarList);
                }

                // taxunit definitions need to be updated (i.e. deleted and refreshed on next use)
                if (newPersons.Count > 0) infoStore.hhAdmin.UpdateAllTUs(hh);
            }
        }

        private List<double> MakeNewPerson(HH hh, double maxPersonId, List<Person> dummyTU, Dictionary<int, ParFormula> varDefinitions,
                                           double motherId = -1, double fatherId = -1, double partnerId = -1)
        {
            List<double> newPerson = new List<double>();
            for (int varIndex = 0; varIndex < hh.personVarList[0].Count; ++varIndex)
            {
                if (varIndex == indexIDHH || varIndex == indexDWT || varIndex == indexDCT) // copy idHH, country-id and weight
                    newPerson.Add(hh.personVarList[0][varIndex]);
                else if (varIndex == indexIDPERSON) // set IdPerson
                    newPerson.Add(maxPersonId);
                else if (partnerId >= 0 && varIndex == indexIDPARTNER) // if we are adding partners, set IdParnter to the id of the "triggering person" ...
                    newPerson.Add(partnerId);
                else if (motherId >= 0 && varIndex == indexIDMOTHER) // ... similar, if we are adding parents, set IdMother ...
                    newPerson.Add(motherId);
                else if (fatherId >= 0 && varIndex == indexIDFATHER) // ... and idFather
                    newPerson.Add(fatherId);
                else if (varDefinitions.ContainsKey(varIndex)) // use defintion if there is any for the variable ...
                    newPerson.Add(varDefinitions[varIndex].GetValue(hh, dummyTU));
                else newPerson.Add(0); // ... otherwise set zero
            }
            return newPerson;
        }
    }
}
