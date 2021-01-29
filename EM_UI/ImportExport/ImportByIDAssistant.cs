using EM_Common;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ImportExport
{
    internal class ImportByIDAssistant
    {
        CountryConfigFacade _inCountryConfigFacade;
        CountryConfigFacade _outCountryConfigFacade;

        short _indexIn = 0;
        short _indexOut = 1;
        short _indexMatch = 2;

        List<List<CountryConfig.SystemRow>> _systemMatches = new List<List<CountryConfig.SystemRow>>();
        CountryConfig.SystemRow _inMatchSystem = null;
        CountryConfig.SystemRow _outMatchSystem = null;

        Dictionary<string, CountryConfig.PolicyRow> _policyMatches = new Dictionary<string, CountryConfig.PolicyRow>();
        Dictionary<string, CountryConfig.FunctionRow> _functionMatches = new Dictionary<string, CountryConfig.FunctionRow>();
        Dictionary<string, CountryConfig.ParameterRow> _parameterMatches = new Dictionary<string, CountryConfig.ParameterRow>();

        internal string _importError = string.Empty;

        internal ImportByIDAssistant(CountryConfigFacade inCountryConfigFacade, CountryConfigFacade outCountryConfigFacade)
        {
            _inCountryConfigFacade = inCountryConfigFacade;
            _outCountryConfigFacade = outCountryConfigFacade;
        }

        internal bool ImportSystems(List<CountryConfig.SystemRow> systemsToImport = null, List<string> systemsToImportNewNames = null)
        {
            try
            {
                //assess whether all systems of out-country are to be imported or a selection, and how the imported systems should be named
                AssessSystemsToImport(ref systemsToImport, ref systemsToImportNewNames);

                //assess whether the systems to import have a match (i.e. a system with equal ID) in in-country
                if (!MatchSystems(systemsToImport))
                    return false;

                //generate a system in in-country for each system to import by copying the matching system if exists or an arbitrary system if not
                GenerateSystemsForImport(systemsToImportNewNames);

                //for components which exist in both countries (i.e. have equal ids): overtake values into just generated import systems
                //for components which exist only in in-country: set to n/a in just generated import systems
                OvertakeValues();

                //for components which exist only in out-country: add and overtake values into just generated import systems
                AddNotYetExistingComponents();
            }
            catch (Exception exception)
            {
                _importError = exception.Message;
                return false;
            }

            return true;
        }

        internal List<CountryConfig.SystemRow> GetImportedSystems()
        {
            List<CountryConfig.SystemRow> importedSystems; List<CountryConfig.SystemRow> twinSystems;
            GetImportedSystemsAndPotentialTwins(out importedSystems, out twinSystems);
            return importedSystems;
        }

        internal void GetImportedSystemsAndPotentialTwins(out List<CountryConfig.SystemRow> importedSystems, out List<CountryConfig.SystemRow> twinSystems)
        {
            importedSystems = new List<CountryConfig.SystemRow>();
            twinSystems = new List<CountryConfig.SystemRow>();

            foreach (List<CountryConfig.SystemRow> systemMatch in _systemMatches)
            {
                importedSystems.Add(systemMatch.ElementAt(_indexIn));
                twinSystems.Add(systemMatch.ElementAt(_indexMatch));
            }
        }

        void AssessSystemsToImport(ref List<CountryConfig.SystemRow> systemsToImport, ref List<string> systemsToImportNewNames)
        {
            if (systemsToImport == null) //if no special selection: import all system of out-country
            {
                systemsToImport = new List<CountryConfig.SystemRow>();
                foreach (CountryConfig.SystemRow systemRow in _outCountryConfigFacade.GetSystemRows())
                    systemsToImport.Add(systemRow);
            }

            if (systemsToImportNewNames == null) //if no name was defined by the user: find a name for the imported system ...
            {
                systemsToImportNewNames = new List<string>();
                List<string> systemsToImportNewNamesLCase = new List<string>();
                foreach (CountryConfig.SystemRow systemRow in _outCountryConfigFacade.GetSystemRows())
                {
                    string newSystemName = systemRow.Name; //... if in-country does not have a system called like the system to import: use equal name
                    for (int index = 1; //... otherwise name it 1_SystemName (or 2_SystemName if 1_SystemName exists, etc.)
                        _inCountryConfigFacade.GetSystemRowByName(newSystemName) != null || //a system called x_SystemName actually exsists
                        systemsToImportNewNamesLCase.Contains(newSystemName.ToLower()); //a system called x_SystemName will be generated (avoid strange cases, where the import would generated two systems with same name - happens by comparing country1-country2 and then country2-country1)
                        ++index)
                            newSystemName = index.ToString() + "_" + systemRow.Name; 
                    systemsToImportNewNames.Add(newSystemName);
                    systemsToImportNewNamesLCase.Add(newSystemName.ToLower());
                }
            }
        }

        internal static bool DoesMatchSystemExist(List<CountryConfig.SystemRow> inSystemRows, List<CountryConfig.SystemRow> outSystemRows)
        {
            List<string> inSystemIDs = (from sys in inSystemRows select sys.ID).ToList();
            foreach (CountryConfig.SystemRow outSystemRow in outSystemRows)
                if (inSystemIDs.Contains(outSystemRow.ID))
                    return true;
            return false;
        }

        bool MatchSystems(List<CountryConfig.SystemRow> systemsToImport)
        {
            _inMatchSystem = _outMatchSystem = null;
            List<string> systemsToImportIDs = (from sys in systemsToImport select sys.ID).ToList();
            foreach (CountryConfig.SystemRow outSystem in _outCountryConfigFacade.GetSystemRows())
            {
                CountryConfig.SystemRow matchSystem = _inCountryConfigFacade.GetSystemRowByID(outSystem.ID);
                if (matchSystem != null && _inMatchSystem == null)
                {
                    _inMatchSystem = matchSystem; //note that the systems to match the countries do not need to be among the systems that are imported
                    _outMatchSystem = outSystem;
                }

                if (systemsToImportIDs.Contains(outSystem.ID))
                {
                    List<CountryConfig.SystemRow> systemMatch = new List<CountryConfig.SystemRow>();
                    systemMatch.Add(null);          //reserve space for the system (in in-country) that will be generated in GenerateSystemsForImport
                    systemMatch.Add(outSystem);     //the system to import in out-country
                    systemMatch.Add(matchSystem);   //the matching system in in-country or null if no matching system
                    _systemMatches.Add(systemMatch);
                }
            }

            if (_inMatchSystem == null)
            {
                _importError = "None of the systems to import has a matching system in the import country (i.e. with equal ID), therefore import of systems by unique ID is not possible.";
                return false;
            }

            return true;
        }

        void GenerateSystemsForImport(List<string> systemsToImportNewNames)
        {
            for (int index = 0; index < _systemMatches.Count; ++index)
            {
                List<CountryConfig.SystemRow> systemMatch = _systemMatches.ElementAt(index);
                CountryConfig.SystemRow importSystem = null;
                if (systemMatch[_indexMatch] == null) //if no matching system in in-country, clone the system which is used for matching ...
                {
                    importSystem = CountryConfigFacade.CopySystemRow(systemsToImportNewNames[index], _inMatchSystem, true); //last parameter: add at tail

                    //initially set all policies, functions and parameters to n/a
                    foreach (CountryConfig.PolicyRow policyRow in importSystem.GetPolicyRows())
                        CountryConfigFacade.SetPolicyToNA(policyRow);
                }
                else //... clone the matching system
                    importSystem = CountryConfigFacade.CopySystemRow(systemsToImportNewNames[index], systemMatch[_indexMatch]); //add after the matching system

                systemMatch.RemoveAt(_indexIn);
                systemMatch.Insert(_indexIn, importSystem);
            }
        }

        void OvertakeValues()
        {
            //OVERTAKE POLICY SWITCHES
            //loop over in-country policies
            foreach (CountryConfig.PolicyRow inMatchPolicy in _inMatchSystem.GetPolicyRows())
            {
                //try to find a matching policy in out-country (by using the two match-systems (_inMatchSystem and _outMatchSystem) assessed before)
                List<CountryConfig.PolicyRow> polQuery = (from pol in _outMatchSystem.GetPolicyRows() where pol.ID == inMatchPolicy.ID select pol).ToList();
                CountryConfig.PolicyRow outMatchPolicy = polQuery.Count == 0 ? null : polQuery.First(); //either a match was found or the policy does not exist in out-country

                //loop over the imported systems (more precise the systems prepared for import before, by copying respective in-country systems)
                List<CountryConfig.PolicyRow> inPolicies = new List<CountryConfig.PolicyRow>();
                List<CountryConfig.PolicyRow> outPolicies = new List<CountryConfig.PolicyRow>();
                for (int indexSystem = 0; indexSystem < _systemMatches.Count; ++indexSystem)
                {
                    List<CountryConfig.SystemRow> systemMatch = _systemMatches.ElementAt(indexSystem);

                    //search for twin-policy in the import-system (i.e. the policy in import-system with the same order as the policy in in-match-system)
                    polQuery = (from pol in systemMatch[_indexIn].GetPolicyRows() where pol.Order == inMatchPolicy.Order select pol).ToList();
                    CountryConfig.PolicyRow inPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0

                    //if policy does not exist in out-country, set switch to n/a
                    if (outMatchPolicy == null)
                        CountryConfigFacade.SetPolicyToNA(inPolicy);
                    else
                    {
                        //if policy does exist in out-country: search for twin-policy in the system to import (i.e. the policy in system to import with the same order as the policy in out-match-system)
                        polQuery = (from pol in systemMatch[_indexOut].GetPolicyRows() where pol.Order == outMatchPolicy.Order select pol).ToList();
                        CountryConfig.PolicyRow outPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0

                        //adapt switch in import-system to value of out-country
                        inPolicy.Switch = outPolicy.Switch;

                        //store policy found in each of the import-system as they are needed for overtaking function switches below
                        inPolicies.Add(inPolicy);
                        outPolicies.Add(outPolicy);

                        //add policy-match to the matches table, which is used for later consistency checks
                        if (!_policyMatches.ContainsKey(inMatchPolicy.ID)) //add only once, not for each imported system (checking is not system specific, thus irrelevant policy of which system is taken)
                            _policyMatches.Add(inMatchPolicy.ID, outPolicy);
                    }
                }

                if (outMatchPolicy == null)
                    continue; //if not even policy exists in out-country, functions and parameters of the policy definitely do not exist

                //OVERTAKE FUNCTION SWITCHES (for descriptions see equivalent approach in OVERTAKE POLICY SWITCHES)
                foreach (CountryConfig.FunctionRow inMatchFunction in inMatchPolicy.GetFunctionRows())
                {
                    List<CountryConfig.FunctionRow> funcQuery = (from func in outMatchPolicy.GetFunctionRows() where func.ID == inMatchFunction.ID select func).ToList();
                    CountryConfig.FunctionRow outMatchFunction = funcQuery.Count == 0 ? null : funcQuery.First();

                    List<CountryConfig.FunctionRow> inFunctions = new List<CountryConfig.FunctionRow>();
                    List<CountryConfig.FunctionRow> outFunctions = new List<CountryConfig.FunctionRow>();
                    for (int indexSystem = 0; indexSystem < _systemMatches.Count; ++indexSystem)
                    {
                        funcQuery = (from func in inPolicies[indexSystem].GetFunctionRows() where func.Order == inMatchFunction.Order select func).ToList();
                        CountryConfig.FunctionRow inFunction = funcQuery.First();

                        if (outMatchFunction == null)
                            CountryConfigFacade.SetFunctionToNA(inFunction);
                        else
                        {
                            funcQuery = (from func in outPolicies[indexSystem].GetFunctionRows() where func.Order == outMatchFunction.Order select func).ToList();
                            CountryConfig.FunctionRow outFunction = funcQuery.First();

                            inFunction.Switch = outFunction.Switch;

                            inFunctions.Add(inFunction);
                            outFunctions.Add(outFunction);

                            if (!_functionMatches.ContainsKey(inMatchFunction.ID)) //add only once, not for each imported system (checking is not system specific, thus irrelevant function of which system is taken)
                                _functionMatches.Add(inMatchFunction.ID, outFunction);
                        }
                    }

                    if (outMatchFunction == null)
                        continue;

                    //OVERTAKE PARAMETER VALUES (for descriptions see equivalent approach in OVERTAKE POLICY SWITCHES)
                    foreach (CountryConfig.ParameterRow inMatchParameter in inMatchFunction.GetParameterRows())
                    {
                        List<CountryConfig.ParameterRow> parQuery = (from par in outMatchFunction.GetParameterRows() where par.ID == inMatchParameter.ID select par).ToList();
                        CountryConfig.ParameterRow outMatchParameter = parQuery.Count == 0 ? null : parQuery.First();

                        for (int indexSystem = 0; indexSystem < _systemMatches.Count; ++indexSystem)
                        {
                            parQuery = (from par in inFunctions[indexSystem].GetParameterRows() where par.Order == inMatchParameter.Order select par).ToList();
                            CountryConfig.ParameterRow inParameter = parQuery.First();

                            if (outMatchParameter == null)
                                inParameter.Value = DefPar.Value.NA;
                            else
                            {
                                parQuery = (from par in outFunctions[indexSystem].GetParameterRows() where par.Order == outMatchParameter.Order select par).ToList();
                                CountryConfig.ParameterRow outParameter = parQuery.First();

                                inParameter.Value = outParameter.Value;

                                if (!_parameterMatches.ContainsKey(inMatchParameter.ID))
                                    _parameterMatches.Add(inMatchParameter.ID, outParameter);
                            }
                        }
                    }
                }

            }
        }

        void AddNotYetExistingComponents()
        {
            //loop over out-country policies to find policies which only exist in out-country
            List<CountryConfig.PolicyRow> outMatchPoliciesOrdered = (from pol in _outMatchSystem.GetPolicyRows() select pol).OrderBy(pol => long.Parse(pol.Order)).ToList();
            foreach (CountryConfig.PolicyRow outMatchPolicy in outMatchPoliciesOrdered)
            {
                //try to find a matching policy in in-country (by using the two match-systems (_inMatchSystem and _outMatchSystem) assessed before)
                List<CountryConfig.PolicyRow> polQuery = (from pol in _inMatchSystem.GetPolicyRows() where pol.ID == outMatchPolicy.ID select pol).ToList();
                CountryConfig.PolicyRow inMatchPolicy = polQuery.Count == 0 ? null : polQuery.First(); //either a match was found or the policy does not yet exist in in-country

                //policy does not yet exist in in-country, thus must be added
                if (inMatchPolicy == null)
                    AddNotYetExistingPolicy(outMatchPolicy);

                //policy does exist in in-country: search for functions possibly missing in in-country
                else
                {
                    List<CountryConfig.FunctionRow> outMatchFunctionsOrdered = (from func in outMatchPolicy.GetFunctionRows() select func).OrderBy(pol => long.Parse(pol.Order)).ToList();
                    foreach (CountryConfig.FunctionRow outMatchFunction in outMatchFunctionsOrdered)
                    {
                        //try to find a matching function in in-country
                        List<CountryConfig.FunctionRow> funcQuery = (from func in inMatchPolicy.GetFunctionRows() where func.ID == outMatchFunction.ID select func).ToList();
                        CountryConfig.FunctionRow inMatchFunction = funcQuery.Count == 0 ? null : funcQuery.First();

                        //function does not yet exist in in-country, thus must be added
                        if (inMatchFunction == null)
                            AddNotYetExistingFunction(outMatchFunction, inMatchPolicy);
                        
                        //function does exist in in-country: search for parameters possibly missing in in-country
                        else
                        {
                            List<CountryConfig.ParameterRow> outMatchParametersOrdered = (from par in outMatchFunction.GetParameterRows() select par).OrderBy(pol => long.Parse(pol.Order)).ToList();
                            foreach (CountryConfig.ParameterRow outMatchParameter in outMatchParametersOrdered)
                            {
                                //try to find a matching parameter in in-country
                                List<CountryConfig.ParameterRow> parQuery = (from par in inMatchFunction.GetParameterRows() where par.ID == outMatchParameter.ID select par).ToList();
                                //parameter does not yet exist in in-country, thus must be added
                                if (parQuery.Count == 0)
                                    AddNotYetExistingParameter(outMatchParameter, inMatchFunction);
                            }
                        }
                    }
                }
            }
        }

        void AddNotYetExistingPolicy(CountryConfig.PolicyRow outMatchPolicy)
        {
            if (outMatchPolicy.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name))
                return; //do not copy (double) the policy which contains the definitions of uprating-factors

            //assess where the policy should be placed:
            //first assess the pre-policy (the policy before the policy to add) in out-country ...
            bool copyBeforeNeighbour = false;
            CountryConfig.PolicyRow outMatchNeighbourPolicyRow = CountryConfigFacade.GetPrePolicyRow(outMatchPolicy);
            CountryConfig.PolicyRow inMatchNeighbourPolicyRow = null;
            List<CountryConfig.PolicyRow> polQuery = null;
            if (outMatchNeighbourPolicyRow != null) //... pre-policy exits: find the respective policy in in-country
            {
                polQuery = (from pol in _inMatchSystem.GetPolicyRows() where pol.ID == outMatchNeighbourPolicyRow.ID select pol).ToList();
                if (polQuery.Count == 0 && !SpecialCase_AfterPolUprateFactors(outMatchNeighbourPolicyRow, ref polQuery)) return; // return means the policy is ignored (at least there'd be no crash in this hopefully not existing case)
                inMatchNeighbourPolicyRow = polQuery.First();
            }
            else //... pre-policy does not exist because policy to add is the first: add before first policy in in-country (i.e. as first policy)
            {
                inMatchNeighbourPolicyRow = CountryConfigFacade.GetFirstPolicyRow(_inMatchSystem);
                copyBeforeNeighbour = true;
                
                //ignore inMatchNeighbourPolicyRow is still null as the country does not yet contain policies, because importing systems by id would then be a rather useless exercise
            }
            //note: this approach also allows adding ranges of new policies
            //example: in-country: pol2, pol3, pol6; out-country: !pol0!, !pol1!, pol2, pol3, !pol4!, !pol5!, pol6
            //(1) add !pol0! before pol2 (i.e. first policy in in-country, as there is no pre-policy in out-country)
            //(2) add !pol1! after !pol0! (i.e. after its pre-policy), note that !pol0! exists in in-country because it was added in (1)
            //(3) add !pol4! after pol3 (i.e. after its pre-policy)
            //(4) add !pol5! after !pol4! (i.e. after its pre-policy), note that !pol4! exists in in-country because it was added in (3)

            //first gather the information to add the policy to all systems considering that there are different sorts of systems (with(out) a match, (not) imported, ...)
            //add the policies only after having obtained this info for ALL systems, to avoid changing orders, etc. during the process of adding
            List<CountryConfig.PolicyRow> outCopyPolicyRows = new List<CountryConfig.PolicyRow>();
            List<CountryConfig.PolicyRow> inNeighbourPolicyRows = new List<CountryConfig.PolicyRow>();
            List<bool> switchNA = new List<bool>();
            List<bool> keepIDs = new List<bool>();
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                CountryConfig.SystemRow inSystemRow = _inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem);

                //default settings are used for "old-established" (i.e. not imported) systems, which do not have a matching system in out-country
                CountryConfig.PolicyRow outCopyPolicyRow = outMatchPolicy; //just copy the policy of the out-match-system
                bool switchNAForSys = true; //set the policies and the contained functions and parameters to n/a
                bool keepIDsForSys = false; //ids must not be kept

                //overwrite default settings for imported systems
                foreach (List<CountryConfig.SystemRow> systemMatch in _systemMatches) //use the _systemMatches table to assess whether the system was imported
                {
                    if (systemMatch.ElementAt(_indexIn).ID == inSystemRow.ID) //that means the system was imported
                    {
                        //find the policy to be copied in the respective system in out-country
                        polQuery = (from pol in systemMatch.ElementAt(_indexOut).GetPolicyRows() where pol.Order == outMatchPolicy.Order select pol).ToList();
                        outCopyPolicyRow = polQuery.First();
                        switchNAForSys = false; //do not set switches and values to n/a of course
                        //keepIDsForSys stays false (these are in fact copies and therefore new systems without a match in out-country)
                        break;
                    }
                }

                //overwrite default settings for "old-established" systems, which do have a matching system in out-country
                CountryConfig.SystemRow outTwinSystem = _outCountryConfigFacade.GetSystemRowByID(inSystemRow.ID);
                if (outTwinSystem != null)
                {
                    //find the policy to be copied in the matching system in out-country
                    polQuery = (from pol in outTwinSystem.GetPolicyRows() where pol.Order == outMatchPolicy.Order select pol).ToList();
                    outCopyPolicyRow = polQuery.First();
                    keepIDsForSys = true; //keep all ids as this makes subsequent imports more accurate (and is essential for the match-system when adding ranges of policies, see above)
                }

                outCopyPolicyRows.Add(outCopyPolicyRow);

                //assess the policy which corresponds with neighbour policy in the match-system
                polQuery = (from pol in inSystemRow.GetPolicyRows() where pol.Order == inMatchNeighbourPolicyRow.Order select pol).ToList();
                inNeighbourPolicyRows.Add(polQuery.First()); //policy must exist, therefore no check for Count==0

                switchNA.Add(switchNAForSys);
                keepIDs.Add(keepIDsForSys);
            }

            //now add the policy to all systems according to the information gathered above
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                CountryConfig.PolicyRow newPolicyRow = CountryConfigFacade.CopyPolicyRow(
                    outCopyPolicyRows.ElementAt(indexSystem),
                    outCopyPolicyRows.ElementAt(indexSystem).Name,
                    inNeighbourPolicyRows.ElementAt(indexSystem),
                    copyBeforeNeighbour,
                    switchNA.ElementAt(indexSystem),
                    keepIDs.ElementAt(indexSystem));

                //add policy-match to the matches table, which is used for later consistency checks
                if (_inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem).ID == _inMatchSystem.ID)
                    _policyMatches.Add(newPolicyRow.ID, outCopyPolicyRows.ElementAt(indexSystem));
            }
        }

        private bool SpecialCase_AfterPolUprateFactors(CountryConfig.PolicyRow outMatchNeighbourPolicyRow, ref List<CountryConfig.PolicyRow> polQuery)
        {
            // if the new policy is right after the DefUpratingFactors-policy, the pre-policy obviously (??? I did not investigate this in more detail)
            // cannot be found by id, therefore search the pre-policy by name
            if (!outMatchNeighbourPolicyRow.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name)) return false; // other problem (???)
            polQuery = (from pol in _inMatchSystem.GetPolicyRows()
                        where pol.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name)
                        select pol).ToList();
            return polQuery.Count > 0;
        }

        void AddNotYetExistingFunction(CountryConfig.FunctionRow outMatchFunction, CountryConfig.PolicyRow inMatchPolicy)
        {
            //assess where the function should be placed:
            //first assess the pre-function (the function before the function to add) in out-country ...
            bool copyBeforeNeighbour = false;
            CountryConfig.FunctionRow outMatchNeighbourFunctionRow = CountryConfigFacade.GetPreFunctionRow(outMatchFunction);
            CountryConfig.FunctionRow inMatchNeighbourFunctionRow = null;
            List<CountryConfig.FunctionRow> funcQuery = null;
            List<CountryConfig.PolicyRow> polQuery = null;
            if (outMatchNeighbourFunctionRow != null) //... pre-function exits: find the respective function in in-country
            {
                funcQuery = (from func in inMatchPolicy.GetFunctionRows() where func.ID == outMatchNeighbourFunctionRow.ID select func).ToList();
                inMatchNeighbourFunctionRow = funcQuery.First(); //function must exist, therefore no check for Count==0
            }
            else //... pre-function does not exist because function to add is the first: add before first function in in-policy (i.e. as first function)
            {
                inMatchNeighbourFunctionRow = CountryConfigFacade.GetFirstFunctionRow(inMatchPolicy);
                if (inMatchNeighbourFunctionRow != null)
                    copyBeforeNeighbour = true;
                //else ... neither pre- nor post-function exists, i.e. sole function within the policy (meaning the respective policy in-country is emtpy)
            }
            //note: this approach also allows adding ranges of new functions: see AddNotYetExistingPolicy for description

            //first gather the information to add the function to all systems considering that there are different sorts of systems (with(out) a match, (not) imported, ...)
            //add the function only after having obtained this info for ALL systems, to avoid changing orders, etc. during the process of adding
            List<CountryConfig.FunctionRow> outCopyFunctionRows = new List<CountryConfig.FunctionRow>();
            List<CountryConfig.FunctionRow> inNeighbourFunctionRows = new List<CountryConfig.FunctionRow>();
            List<CountryConfig.PolicyRow> inPolicyRows = new List<CountryConfig.PolicyRow>();
            List<bool> switchNA = new List<bool>();
            List<bool> keepIDs = new List<bool>();
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                CountryConfig.SystemRow inSystemRow = _inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem);

                //default settings are used for "old-established" (i.e. not imported) systems, which do not have a matching system in out-country
                CountryConfig.FunctionRow outCopyFunctionRow = outMatchFunction; //just copy the function of the out-match-system
                bool switchNAForSys = true; //set the function and the contained parameters to n/a
                bool keepIDsForSys = false; //ids must not be kept

                //overwrite default settings for imported systems
                foreach (List<CountryConfig.SystemRow> systemMatch in _systemMatches) //use the _systemMatches table to assess whether the system was imported
                {
                    if (systemMatch.ElementAt(_indexIn).ID == inSystemRow.ID) //that means the system was imported
                    {
                        //find the function to be copied in the respective system in out-country
                        polQuery = (from pol in systemMatch.ElementAt(_indexOut).GetPolicyRows() where pol.Order == outMatchFunction.PolicyRow.Order select pol).ToList();
                        CountryConfig.PolicyRow outCopyTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                        funcQuery = (from func in outCopyTwinPolicy.GetFunctionRows() where func.Order == outMatchFunction.Order select func).ToList();
                        outCopyFunctionRow = funcQuery.First();
                        switchNAForSys = false; //do not set switches and values to n/a of course
                        //keepIDsForSys stays false (these are in fact copies and therefore new systems without a match in out-country)
                        break;
                    }
                }

                //overwrite default settings for "old-established" systems, which do have a matching system in out-country
                CountryConfig.SystemRow outTwinSystem = _outCountryConfigFacade.GetSystemRowByID(inSystemRow.ID);
                if (outTwinSystem != null)
                {
                    //find the function to be copied in the matching system in out-country
                    polQuery = (from pol in outTwinSystem.GetPolicyRows() where pol.Order == outMatchFunction.PolicyRow.Order select pol).ToList();
                    CountryConfig.PolicyRow outTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                    funcQuery = (from func in outTwinPolicy.GetFunctionRows() where func.Order == outMatchFunction.Order select func).ToList();
                    outCopyFunctionRow = funcQuery.First();
                    keepIDsForSys = true; //keep all ids as this makes subsequent imports more accurate (and is essential for the match-system when adding ranges of function)
                }

                outCopyFunctionRows.Add(outCopyFunctionRow);

                //assess the function which corresponds with neighbour function in the match-system
                //respectively, if the function is the sole function in the policy, assess the parent policy where to add the function
                polQuery = (from pol in inSystemRow.GetPolicyRows() where pol.Order == inMatchPolicy.Order select pol).ToList();
                CountryConfig.PolicyRow inTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                if (inMatchNeighbourFunctionRow != null)
                {
                    funcQuery = (from func in inTwinPolicy.GetFunctionRows() where func.Order == inMatchNeighbourFunctionRow.Order select func).ToList();
                    inNeighbourFunctionRows.Add(funcQuery.First()); //function must exist, therefore no check for Count==0
                    inPolicyRows.Add(null); //not necessary if there is a neighbour
                }
                else
                {
                    inNeighbourFunctionRows.Add(null); //no neighbour available
                    inPolicyRows.Add(inTwinPolicy);
                }

                switchNA.Add(switchNAForSys);
                keepIDs.Add(keepIDsForSys);
            }

            //now add the function to all systems according to the information gathered above
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                CountryConfig.FunctionRow newFunctionRow = null;
                if (inNeighbourFunctionRows.ElementAt(indexSystem) != null) //the usual case: function is added before or after a sibling-function
                    newFunctionRow = CountryConfigFacade.CopyFunctionRow(
                        outCopyFunctionRows.ElementAt(indexSystem),
                        inNeighbourFunctionRows.ElementAt(indexSystem),
                        copyBeforeNeighbour,
                        switchNA.ElementAt(indexSystem),
                        keepIDs.ElementAt(indexSystem));
                else //special case: function is the sole function in out-country's policy and is copied to an empty policy in in-country
                    newFunctionRow = CountryConfigFacade.CopyFunctionRowAtTailOrUseOriginalOrder(
                        outCopyFunctionRows.ElementAt(indexSystem),
                        inPolicyRows.ElementAt(indexSystem),
                        true, //copyAtTail
                        switchNA.ElementAt(indexSystem),
                        keepIDs.ElementAt(indexSystem));

                //add function-match to the matches table, which is used for later consistency checks
                if (_inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem).ID == _inMatchSystem.ID)
                    _functionMatches.Add(newFunctionRow.ID, outCopyFunctionRows.ElementAt(indexSystem));
            }
        }

        //for descriptions see equivalent approach in AddNotYetExistingPolicy
        void AddNotYetExistingParameter(CountryConfig.ParameterRow outMatchParameter, CountryConfig.FunctionRow inMatchFunction)
        {
            //assess where the parameter should be placed:
            //first assess the pre-parameter (the parameter before the parameter to add) in out-country ...
            bool copyBeforeNeighbour = false;
            CountryConfig.ParameterRow outMatchNeighbourParameterRow = CountryConfigFacade.GetPreParameterRow(outMatchParameter);
            CountryConfig.ParameterRow inMatchNeighbourParameterRow = null;
            List<CountryConfig.ParameterRow> parQuery = null;
            List<CountryConfig.FunctionRow> funcQuery = null;
            List<CountryConfig.PolicyRow> polQuery = null;
            if (outMatchNeighbourParameterRow != null) //... pre-parameter exits: find the respective parameter in in-country
            {
                parQuery = (from par in inMatchFunction.GetParameterRows() where par.ID == outMatchNeighbourParameterRow.ID select par).ToList();
                inMatchNeighbourParameterRow = parQuery.First(); //parameter must exist, therefore no check for Count==0
            }
            else //... pre-parameter does not exist because parameter to add is the first: add before first parameter in in-function (i.e. as first parameter)
            {
                inMatchNeighbourParameterRow = CountryConfigFacade.GetFirstParameterRow(inMatchFunction);
                if (inMatchNeighbourParameterRow != null)
                    copyBeforeNeighbour = true;
                //else ... neither pre- nor post-parameter exists, i.e. sole parameter within the function (meaning the respective function in-country is emtpy)
            }
            //note: this approach also allows adding ranges of new parameters: see AddNotYetExistingPolicy for description

            //first gather the information to add the parameter to all systems considering that there are different sorts of systems (with(out) a match, (not) imported, ...)
            //add the parameter only after having obtained this info for ALL systems, to avoid changing orders, etc. during the process of adding
            List<CountryConfig.ParameterRow> outCopyParameterRows = new List<CountryConfig.ParameterRow>();
            List<CountryConfig.ParameterRow> inNeighbourParameterRows = new List<CountryConfig.ParameterRow>();
            List<CountryConfig.FunctionRow> inFunctionRows = new List<CountryConfig.FunctionRow>();
            List<bool> switchNA = new List<bool>();
            List<bool> keepIDs = new List<bool>();
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                CountryConfig.SystemRow inSystemRow = _inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem);

                //default settings are used for "old-established" (i.e. not imported) systems, which do not have a matching system in out-country
                CountryConfig.ParameterRow outCopyParameterRow = outMatchParameter; //just copy the parameter of the out-match-system
                bool switchNAForSys = true; //set the parameter and the contained parameters to n/a
                bool keepIDsForSys = false; //ids must not be kept

                //overwrite default settings for imported systems
                foreach (List<CountryConfig.SystemRow> systemMatch in _systemMatches) //use the _systemMatches table to assess whether the system was imported
                {
                    if (systemMatch.ElementAt(_indexIn).ID == inSystemRow.ID) //that means the system was imported
                    {
                        //find the parameter to be copied in the respective system in out-country

                        polQuery = (from pol in systemMatch.ElementAt(_indexOut).GetPolicyRows() where pol.Order == outMatchParameter.FunctionRow.PolicyRow.Order select pol).ToList();
                        CountryConfig.PolicyRow outCopyTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                        funcQuery = (from func in outCopyTwinPolicy.GetFunctionRows() where func.Order == outMatchParameter.FunctionRow.Order select func).ToList();
                        CountryConfig.FunctionRow outCopyTwinFunction = funcQuery.First(); //function must exist, therefore no check for Count==0
                        parQuery = (from par in outCopyTwinFunction.GetParameterRows() where par.Order == outMatchParameter.Order select par).ToList();
                        outCopyParameterRow = parQuery.First();
                        switchNAForSys = false; //do not set switches and values to n/a of course
                        //keepIDsForSys stays false (these are in fact copies and therefore new systems without a match in out-country)
                        break;
                    }
                }

                //overwrite default settings for "old-established" systems, which do have a matching system in out-country
                CountryConfig.SystemRow outTwinSystem = _outCountryConfigFacade.GetSystemRowByID(inSystemRow.ID);
                if (outTwinSystem != null)
                {
                    //find the parameter to be copied in the matching system in out-country
                    polQuery = (from pol in outTwinSystem.GetPolicyRows() where pol.Order == outMatchParameter.FunctionRow.PolicyRow.Order select pol).ToList();
                    CountryConfig.PolicyRow outTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                    funcQuery = (from func in outTwinPolicy.GetFunctionRows() where func.Order == outMatchParameter.FunctionRow.Order select func).ToList();
                    CountryConfig.FunctionRow outTwinFunction = funcQuery.First(); //function must exist, therefore no check for Count==0
                    parQuery = (from par in outTwinFunction.GetParameterRows() where par.Order == outMatchParameter.Order select par).ToList();
                    outCopyParameterRow = parQuery.First();
                    keepIDsForSys = true; //keep all ids as this makes subsequent imports more accurate (and is essential for the match-system when adding ranges of parameter)
                }

                outCopyParameterRows.Add(outCopyParameterRow);

                //assess the parameter which corresponds with neighbour parameter in the match-system
                //respectively, if the parameter is the sole parameter in the function, assess the parent function where to add the parameter
                polQuery = (from pol in inSystemRow.GetPolicyRows() where pol.Order == inMatchFunction.PolicyRow.Order select pol).ToList();
                CountryConfig.PolicyRow inTwinPolicy = polQuery.First(); //policy must exist, therefore no check for Count==0
                funcQuery = (from func in inTwinPolicy.GetFunctionRows() where func.Order == inMatchFunction.Order select func).ToList();
                CountryConfig.FunctionRow inTwinFunction = funcQuery.First(); //function must exist, therefore no check for Count==0

                if (inMatchNeighbourParameterRow != null)
                {
                    parQuery = (from par in inTwinFunction.GetParameterRows() where par.Order == inMatchNeighbourParameterRow.Order select par).ToList();
                    inNeighbourParameterRows.Add(parQuery.First()); //parameter must exist, therefore no check for Count==0
                    inFunctionRows.Add(null); //not necessary if there is a neighbour
                }
                else
                {
                    inNeighbourParameterRows.Add(null); //no neighbour available
                    inFunctionRows.Add(inTwinFunction);
                }

                switchNA.Add(switchNAForSys);
                keepIDs.Add(keepIDsForSys);
            }

            //now add the parameter to all systems according to the information gathered above
            for (int indexSystem = 0; indexSystem < _inCountryConfigFacade.GetSystemRows().Count; ++indexSystem)
            {
                bool copyAtTail = inNeighbourParameterRows.ElementAt(indexSystem) == null; //special case: parameter is the sole parameter in out-country's function and is copied to an empty function in in-country
                CountryConfig.ParameterRow newParameterRow = CountryConfigFacade.CopyParameterRow(
                        outCopyParameterRows.ElementAt(indexSystem),
                        copyAtTail ? inFunctionRows.ElementAt(indexSystem) : inNeighbourParameterRows.ElementAt(indexSystem).FunctionRow,
                        switchNA.ElementAt(indexSystem),
                        keepIDs.ElementAt(indexSystem),
                        inNeighbourParameterRows.ElementAt(indexSystem),
                        copyBeforeNeighbour, copyAtTail);

                //add parameter-match to the matches table, which is used for later consistency checks
                if (_inCountryConfigFacade.GetSystemRows().ElementAt(indexSystem).ID == _inMatchSystem.ID)
                    _parameterMatches.Add(newParameterRow.ID, outCopyParameterRows.ElementAt(indexSystem));
            }
        }
       
        //take care about remaining differences (most importantly in policy/function/parameter-order and parameter-name and -group) and inform user
        internal ImportByIDDiscrepancies AssessRemainingDiscrepancies()
        {
            ImportByIDDiscrepancies discrepancies = new ImportByIDDiscrepancies();
            discrepancies.SetConcernedSystems(GetImportedSystems());

            //CHECK POLICIES
            List<CountryConfig.PolicyRow> inMatchPoliciesOrdered = (from pol in _inMatchSystem.GetPolicyRows() select pol).OrderBy(pol => long.Parse(pol.Order)).ToList();
            SortedDictionary<int, string> policyOrderListing = new SortedDictionary<int, string>();
            bool differentPolicyOrder = false;
            foreach (CountryConfig.PolicyRow inPolicy in inMatchPoliciesOrdered)
            {
                if (!_policyMatches.Keys.Contains(inPolicy.ID))
                {   //policy does not exist in out-country, still call HandleOrder to get the correct policy-ordering (as visible in the spine)
                    ImportByIDDiscrepancies.HandleOrder(string.Empty, "-1", ref policyOrderListing, ref differentPolicyOrder);
                    continue;
                }

                CountryConfig.PolicyRow outPolicy = _policyMatches[inPolicy.ID];

                Discrepancy policyDiscrepancy = new Discrepancy();
                ImportByIDDiscrepancies.HandleName(ref policyDiscrepancy, inPolicy.Name, outPolicy.Name);
                ImportByIDDiscrepancies.HandlePrivate(ref policyDiscrepancy, inPolicy.Private, outPolicy.Private);
                ImportByIDDiscrepancies.HandleComment(ref policyDiscrepancy, inPolicy.Comment, outPolicy.Comment, false);
                ImportByIDDiscrepancies.HandleComment(ref policyDiscrepancy, inPolicy.PrivateComment, outPolicy.PrivateComment, true);
                string policyName = inPolicy.Name;
                if (inPolicy.ReferencePolID != null && inPolicy.ReferencePolID != string.Empty) //get the "real" policy to assess name
                {
                    CountryConfig.PolicyRow referencePolicy = _inCountryConfigFacade.GetPolicyRowByID(inPolicy.ReferencePolID);
                    if (referencePolicy != null)
                        policyName = "reference - " + referencePolicy.Name;
                }
                ImportByIDDiscrepancies.HandleOrder(policyName, outPolicy.Order, ref policyOrderListing, ref differentPolicyOrder);

                discrepancies.AddDiscrepancy(inPolicy.ID, policyDiscrepancy);

                //CHECK FUNCTIONS
                List<CountryConfig.FunctionRow> inMatchFunctionsOrdered = (from func in inPolicy.GetFunctionRows() select func).OrderBy(func => long.Parse(func.Order)).ToList();
                SortedDictionary<int, string> functionOrderListing = new SortedDictionary<int, string>();
                bool differentFunctionOrder = false;
                foreach (CountryConfig.FunctionRow inFunction in inMatchFunctionsOrdered)
                {
                    if (!_functionMatches.Keys.Contains(inFunction.ID))
                    {   //function does not exist in out-country, still call HandleOrder to get the correct function-ordering (as visible in the spine)
                        ImportByIDDiscrepancies.HandleOrder(string.Empty, "-1", ref functionOrderListing, ref differentFunctionOrder);
                        continue;
                    }

                    CountryConfig.FunctionRow outFunction = _functionMatches[inFunction.ID];

                    Discrepancy functionDiscrepancy = new Discrepancy();
                    ImportByIDDiscrepancies.HandlePrivate(ref functionDiscrepancy, inFunction.Private, outFunction.Private);
                    ImportByIDDiscrepancies.HandleComment(ref functionDiscrepancy, inFunction.Comment, outFunction.Comment, false);
                    ImportByIDDiscrepancies.HandleComment(ref functionDiscrepancy, inFunction.PrivateComment, outFunction.PrivateComment, true);
                    ImportByIDDiscrepancies.HandleOrder(inFunction.Name, outFunction.Order, ref functionOrderListing, ref differentFunctionOrder);

                    discrepancies.AddDiscrepancy(inFunction.ID, functionDiscrepancy);

                    //CHECK PARAMETERS
                    List<CountryConfig.ParameterRow> inMatchParametersOrdered = (from par in inFunction.GetParameterRows() select par).OrderBy(par => long.Parse(par.Order)).ToList();
                    SortedDictionary<int, string> parameterOrderListing = new SortedDictionary<int, string>();
                    bool differentParameterOrder = false;
                    foreach (CountryConfig.ParameterRow inParameter in inMatchParametersOrdered)
                    {
                        if (!_parameterMatches.Keys.Contains(inParameter.ID))
                        {   //parameter does not exist in out-country, still call HandleOrder to get the correct parameter-ordering (as visible in the spine)
                            ImportByIDDiscrepancies.HandleOrder(string.Empty, "-1", ref parameterOrderListing, ref differentParameterOrder);
                            continue;
                        }

                        CountryConfig.ParameterRow outParameter = _parameterMatches[inParameter.ID];

                        Discrepancy parameterDiscrepancy = new Discrepancy();
                        ImportByIDDiscrepancies.HandleName(ref parameterDiscrepancy, inParameter.Name, outParameter.Name);
                        ImportByIDDiscrepancies.HandlePrivate(ref parameterDiscrepancy, inParameter.Private, outParameter.Private);
                        ImportByIDDiscrepancies.HandleComment(ref parameterDiscrepancy, inParameter.Comment, outParameter.Comment, false);
                        ImportByIDDiscrepancies.HandleComment(ref parameterDiscrepancy, inParameter.PrivateComment, outParameter.PrivateComment, true);
                        ImportByIDDiscrepancies.HandleGroup(ref parameterDiscrepancy, inParameter.Group, outParameter.Group);
                        ImportByIDDiscrepancies.HandleOrder(inParameter.Name, outParameter.Order, ref parameterOrderListing, ref differentParameterOrder);

                        discrepancies.AddDiscrepancy(inParameter.ID, parameterDiscrepancy);
                    }
                    if (differentParameterOrder) //differences concerning order of parameters are reported with the (parent) function
                        discrepancies.AddOrderDiscrepancy(inFunction.ID, parameterOrderListing, "parameter");
                }

                if (differentFunctionOrder) //differences concerning order of functions are reported with the (parent) policy
                    discrepancies.AddOrderDiscrepancy(inPolicy.ID, functionOrderListing, "function");
            }

            if (differentPolicyOrder) //differences concerning order of policies are reported with the first policy
                discrepancies.AddOrderDiscrepancy(inMatchPoliciesOrdered.First().ID, policyOrderListing, "policy");

            return (discrepancies.Count() == 0) ? null : discrepancies;
        }
    }
}


