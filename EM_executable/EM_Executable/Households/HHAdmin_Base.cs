using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HHAdmin
    {
        internal InfoStore infoStore = null;
        internal HHAdmin(InfoStore _infoStore) { infoStore = _infoStore; }

        // -----------------------------------------------------------------------------------------------------------------------------
        // T H E  D A T A  of the programme, i.e. all variables for all persons
        internal List<HH> hhs = new List<HH>();
        // -----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// provides a dummy HH, which can be used if there is actually no HH, but a function requires one
        /// most prominent use: RunCond assessment for non-spine-functions (see FunBase.IsRunCondMet)
        /// </summary>
        internal HH GetFirstHH() { return hhs[0]; }

        /// <summary>
        /// each HH provides two dummy individual TUs, which are built on HH creation (i.e. HHAdmin.GenerateData-function calls CreateDummy)
        /// the TUs are used for spine-functions which do not have a TAX_UNIT parameter, as ILVarOp, Store, RandSeed, etc.
        /// this works as follows:
        /// - FunInSpineBase.GetTUName() returns DUMMY_INDIVIDUAL_TU as TU-name if there is in fact no TAX_UNIT-parameter
        /// - Control.RunParallel (or RunSequential) calls HHGetTUs(tuName: DUMMY_INDIVIDUAL_TU, funDefTU: null)
        ///   (as DUMMY_INDIVIDUAL_TU always exists in HH.hhTUs, funDefTU is never used and thus can be null)
        ///   which returns an the individual dummy-TU, thus the function is carried out for each person in the HH
        /// Similarly, for RandSeed, the DUMMY_HOUSEHOLD_TU is returned, so it runs only once per HH
        /// </summary>
        private void CreateDummyTU(HH hh)
        {
            // build the DUMMY_INDIVIDUAL_TU
            List<List<Person>> iTUs = new List<List<Person>>();
            for (byte p = 0; p < hh.personVarList.Count; ++p) iTUs.Add(new List<Person>() { new Person(p) });
            hh.hhTUs.Add(DUMMY_INDIVIDUAL_TU, iTUs);
            // build the DUMMY_HOUSEHOLD_TU
            iTUs = new List<List<Person>>() { new List<Person>() };
            for (byte p = 0; p < hh.personVarList.Count; ++p) iTUs[0].Add(new Person(p));
            hh.hhTUs.Add(DUMMY_HOUSEHOLD_TU, iTUs);
        }
        internal const string DUMMY_INDIVIDUAL_TU = "dummy_individual_tu";
        internal const string DUMMY_HOUSEHOLD_TU = "dummy_household_tu";

        internal void UpdateAllTUs(HH hh)
        {
            hh.hhTUs.Clear(); // to rebuild taxunits on next access
            CreateDummyTU(hh);
        }
    }
}
