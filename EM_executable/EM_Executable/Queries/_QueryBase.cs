using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    abstract class QueryBase
    {
        protected InfoStore infoStore = null;
        internal Description description = null;

        internal QueryBase(InfoStore _infoStore) { infoStore = _infoStore; }

        /// <summary> check and prepare the query's parameters </summary>
        internal abstract void CheckAndPrepare(FunBase fun,
                                               List<string> footnoteNumbers); // usually that's one footnote number (e.g. IsDepChild#2 -> 2)

        internal abstract double GetValue(HH hh, List<Person> tu, Person person);

        // parameters that are used (or may be used in future) in several queries (and therefore use general functions for assessment)
        protected ParNumber parAgeMin = null, parAgeMax = null;
        protected ParNumber parN = null, parM = null;
        protected ParVarIL parIncome = null;
        protected ParVarIL minMaxVal = null;
        protected bool minMaxAdultsOnly = false;
        protected bool minMaxUnique = false;

        // indices of several variables that are used in several queries (and therefore prepared as a default)
        protected int indexAgeVar = -1;
        protected int indexMaritalStatus = -1;
        protected int indexPersonIdVar = -1;
        protected int indexFatherIdVar = -1;
        protected int indexMotherIdVar = -1;
        protected int indexEducationVar = -1;
        protected int indexPartnerIdVar = -1;
        protected int indexEconomicStatusVar = -1;
        protected int indexOccupationVar = -1;

        // one could put this abstract and overrides would only prepare the required variables
        // but it probably does no harm to do it for all queries
        // (in principle the same applies to preparing query-parameters (i.e. CheckAndPrepare), I guess it's a matter of taste ...)
        internal virtual void PrepareVarIndices()
        {
            // prepare variable indices that should always exist
            indexAgeVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DAG);
            indexMaritalStatus = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DMS);
            indexPersonIdVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON);
            indexFatherIdVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDFATHER);
            indexMotherIdVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDMOTHER);
            indexEducationVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DEC);
            indexPartnerIdVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPARTNER);
            indexEconomicStatusVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LES);
            indexOccupationVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LOC);
        }

        protected void PrepareAgePar(FunBase fun, List<string> footnoteNumbers) // age parameters are provided by several queries
        {                                                                       // therefore have a centralised function ...
            parAgeMin = fun.GetFootnotePar<ParNumber>(DefQuery.Par.AgeMin, footnoteNumbers);
            parAgeMax = fun.GetFootnotePar<ParNumber>(DefQuery.Par.AgeMax, footnoteNumbers);

            // to consider: I think age parameters are always optional
            // (otherwise one would need a parameter 'isOptional' and respective checks if it is false)

            // also see note in PrepareNMPar about getting numbers instead of just the parameters
        }

        protected void GetAgeLimits(out double ageMin, out double ageMax)
        {
            ageMin = parAgeMin == null ? DefinitionAdmin.NEG_INFINITE : parAgeMin.GetValue();
            ageMax = parAgeMax == null ? DefinitionAdmin.POS_INFINITE : parAgeMax.GetValue();
        }

        protected void PrepareIncomeInfoPar(FunBase fun, List<string> footnoteNumbers) // ... ditto income/info parameter
        {
            parIncome = fun.GetFootnotePar<ParVarIL>(DefQuery.Par.Income, footnoteNumbers);
            if (parIncome == null) parIncome = fun.GetFootnotePar<ParVarIL>(DefQuery.Par.Info, footnoteNumbers);
            if (parIncome == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.Income} missing" });
        }

        // currently only used by IsNtoMchild, but still make it general as future queries may make use of it
        protected void PrepareNMPar(FunBase fun, List<string> footnoteNumbers)
        {
            parN = fun.GetFootnotePar<ParNumber>(DefQuery.Par.N, footnoteNumbers);
            if (parN == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.N} missing" });

            parM = fun.GetFootnotePar<ParNumber>(DefQuery.Par.M, footnoteNumbers);
            if (parM == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.M} missing" });

            // note: getting the numbers and not just the parameters at this stage may be more efficient, but would make using 
            // constants more difficult (actually happens) and a future change to ParFormula (for more flexibility) impossible
        }

        // used by HasMinValInTU and HasMaxValInTU
        protected void PrepareMinMaxPar(FunBase fun, List<string> footnoteNumbers)
        {
            minMaxVal = fun.GetFootnotePar<ParVarIL>(DefQuery.Par.Val, footnoteNumbers);
            if (minMaxVal == null) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
            { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.Val} missing" });

            ParBool parUnique = fun.GetFootnotePar<ParBool>(DefQuery.Par.Unique, footnoteNumbers);
            if (parUnique != null) minMaxUnique = parUnique.GetBoolValue();

            ParBool parAd = fun.GetFootnotePar<ParBool>(DefQuery.Par.Adults_Only, footnoteNumbers);
            if (parAd != null) minMaxAdultsOnly = parAd.GetBoolValue();
        }
    }
}
