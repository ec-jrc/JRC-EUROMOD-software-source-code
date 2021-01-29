using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HH
    {
        // -----------------------------------------------------------------------------------------------------------------------------
        // the households TU administration:
        // for each TU a list of person-groups belonging to a TU of this type
        // *** IMPORTANT NOTE ***: the first person is the head 
        // one item could for example be:
        //     key: tu_family_cc;
        //     value: { 0, 1, 3 }, { 2, 4 }, { 5 }
        //     i.e. persons 0,1,3 form the 1st unit, person 2,4 the 2nd and person 5 the 3rd
        // note: is only internal because HHAdmin needs to add DUMMY_INDIVIDUAL_TU otherwise could be private
        //       (DUMMY_INDIVIDUAL_TU is created by HHAdmin to keep the dummy-functions together)
        internal Dictionary<string,                      // name of TU (e.g. tu_individual_cc, tu_household_cc, tu_family_cc, ...)
                            List<List<Person>>> hhTUs =  // TU-members, where the 1st is the head
            new Dictionary<string, List<List<Person>>>();
        // -----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// returns TUs of a special type
        /// examples based on household with 4 persons: father (0), mother (1), child (2), grandfather (3)
        /// tuName = tu_individual_cc: {0} {1} {2} {3}
        /// tuName = tu_household_cc: {0,1,2,3} (father is head)
        /// tuName = tu_family_cc: {0,1,2}, {3} (father is head of 1st TU), (grandfather is head of 2nd TU)
        /// note: applies "lazy TU-building": only actually compose the TUs of the type if the type is used
        /// </summary>
        internal List<List<Person>> GetTUs(string tuName)
        {
            if (!hhTUs.ContainsKey(tuName))
            {
                var tus = infoStore.indexTUs[tuName].ComposeTUs(this, out bool isStatic);

                if (isStatic) hhTUs.Add(tuName, tus); // only Add if not a "dynamic" TU,
                                                      // i.e. needs rebuilding because definition(s) depending on simulated variables
                return tus;
            }
            return hhTUs[tuName];
        }

        // called be FunUpdateTU
        internal void UpdateTU(string name)
        {
            if (hhTUs.ContainsKey(name)) hhTUs.Remove(name);
        }

        /// <summary>
        /// returns the alternative TU based on the main TU
        /// example: mainTU is {3} (perhaps based on tu_individual_cc),
        ///    alternative TU is tu_family_cc which, for this HH, has 3 TUs: {0,1}, {2,3}, {4}
        ///    return value would be {2,3}
        /// note that one can only get bigger units, e.g. main=individual, alt=couple; main=couple, alt=household
        /// not possible: main=household, alt=individual, etc.
        /// questionable: main=couple, alt=family
        /// that means a.o. indivual is always possible for main, household is always possible for alt, everything else is difficult
        /// maybe also note that this is not used for condition-evaluating, where the condition is always evaluated for each(!) person
        /// (see ParCond.GetPersonsValues, which returns a list of values)
        /// </summary>
        internal List<Person> GetAlternativeTU(string tuName, List<Person> mainTU, Description description)
        {
            if (!infoStore.indexTUs.ContainsKey(tuName))    // this tu does not even exist!
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: unknown AlternativeTU requested '{tuName}' - Handling: AlternativeTU is ignored", runTimeErrorId = description.funID });
                return mainTU;
            }

            List<List<Person>> tus = hhTUs.ContainsKey(tuName) ? hhTUs[tuName]
                                                               : GetTUs(tuName); // alternative TU not used before
            foreach (List<Person> tu in tus)
            {
                if (tu.Count < mainTU.Count) continue;      // if this TU is smaller than mainTU, skip it

                bool missing = false, existing = false;
                foreach (Person person in mainTU)           // check that all members of mainTU exist in this tu
                    if (tu.Exists(p => p.indexInHH == person.indexInHH))
                        { existing = true; }                // one member was found
                    else
                        { missing = true; break; }          // break if one does not exist

                if (existing)                               // at least one member of mainTU was found in this tu
                {
                    if (missing) break;                     // at least one member was found but not all! it is impossible for this TU type to contain the main... (the member we just found will be missing from all other TUs)
                    else return tu;                         // all members of mainTU exist in this tu, so return it
                }
                // else no members found, try the next TU
            }

            // no matching TUs found, so issue a warning 
            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = true, message = $"{description.Get()}: Assessment unit cannot be used as alternative level. (Only assessment units containing the function's main assessment unit are allowed (e.g. maintu=family/alttu=hh is ok as family is part of hh; maintu=hh/alttu=family not ok as hh is not part of family). Handling: Alternative level is ignored for {DefVarName.IDHH}: {infoStore.GetIDHH(this)}", runTimeErrorId = description.funID });
            return mainTU;
        }
    }
}
