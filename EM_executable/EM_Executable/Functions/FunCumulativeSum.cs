using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunCumulativeSum : FunOutOfSpineBase
    {
        private class SortSpecification { internal ParVarIL varOrIl; internal bool isAscending = true; }
        
        private List<SortSpecification> sortSpecifications = null;
        private ParVarIL sumVarOrIl;
        private bool isSumWeighted = false;
        private bool isSumAbsolute = false;
        private ParVar outputVar;
        private int indexDwt = -1;
        private int indexIdPerson = -1;
        private int indexOutVar = -1;


        internal FunCumulativeSum(InfoStore infoStore) : base(infoStore) { }

        protected override void PrepareNonCommonPar()
        {
            // first prepare all the user sorting variables
            sortSpecifications = new List<SortSpecification>();

            foreach (var pg in GetParGroups(DefPar.CumulativeSum.SortingVar))
            {
                SortSpecification sortSpecification = new SortSpecification();
                sortSpecification.varOrIl = GetUniqueGroupPar<ParVarIL>(DefPar.CumulativeSum.SortingVar, pg.Value);
                if (sortSpecification.varOrIl == null)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: missing required parameter {DefPar.CumulativeSum.SortingVar}" });
                ParBool asc = GetUniqueGroupPar<ParBool>(DefPar.CumulativeSum.SortingAsc, pg.Value);
                if (asc != null) sortSpecification.isAscending = asc.GetBoolValue();
                sortSpecifications.Add(sortSpecification);
            }
            // then the summing parameters
            sumVarOrIl = GetUniquePar<ParVarIL>(DefPar.CumulativeSum.SummingVar);
            isSumWeighted = GetParBoolValueOrDefault(DefFun.CumulativeSum, DefPar.CumulativeSum.SummingWeighted);
            isSumAbsolute = GetParBoolValueOrDefault(DefFun.CumulativeSum, DefPar.CumulativeSum.SummingAbsolute);

            // finally the outputvar
            outputVar = GetUniquePar<ParVar>(DefPar.CumulativeSum.OutputVar);
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            indexDwt = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DWT);
            indexIdPerson = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.IDPERSON);
            sumVarOrIl.ReplaceVarNameByIndex();
            indexOutVar = infoStore.operandAdmin.GetIndexInPersonVarList(outputVar.name);
            foreach (SortSpecification sortSpecification in sortSpecifications)
                sortSpecification.varOrIl.ReplaceVarNameByIndex();
        }

        protected override void DoFunWork()
        {
            MultiSortTUs multiSortTUs = new MultiSortTUs(sortSpecifications, indexIdPerson);
            double totalValue = 0, cumulativeValue = 0;
            // First calculate the absolute total if required
            if (!isSumAbsolute)
                infoStore.hhAdmin.hhs.ForEach(hh => hh.GetTUs(coParTU.name).ForEach(tu => totalValue += (isSumWeighted ? sumVarOrIl.GetValue(hh, tu) * hh.GetTUValue(indexDwt, tu) : sumVarOrIl.GetValue(hh, tu))));
            // Then make a list of all TUs and sort them
            // keep a link to hh, in order to have access to GetValue
            List<TUwHH> allTUs = new List<TUwHH>();
            infoStore.hhAdmin.hhs.ForEach(hh => hh.GetTUs(coParTU.name).ForEach(tu => allTUs.Add(new TUwHH() { tu = tu, hh = hh })));
            allTUs.Sort(multiSortTUs);

            // And calculate their cumulative values
            foreach (TUwHH tuwhh in allTUs)
            {
                cumulativeValue += isSumWeighted ? sumVarOrIl.GetValue(tuwhh.hh, tuwhh.tu) * tuwhh.hh.GetTUValue(indexDwt, tuwhh.tu) : sumVarOrIl.GetValue(tuwhh.hh, tuwhh.tu);
                tuwhh.hh.SetTUValue(isSumAbsolute ? cumulativeValue : (cumulativeValue / totalValue), indexOutVar, tuwhh.tu);
            }
        }

        class TUwHH
        {
            internal List<Person> tu;
            internal HH hh;
        }

        class MultiSortTUs: IComparer<TUwHH>
        {
            List<SortSpecification> sortSpecifications;
            int indexIdPerson;
            public MultiSortTUs(List<SortSpecification> _sortSpecifications, int _indexIdPerson)
            {
                sortSpecifications = _sortSpecifications;
                indexIdPerson = _indexIdPerson;
            }

            public int Compare(TUwHH x, TUwHH y)
            {
                // compare each sorting var in order and imediately return if the values are not equal
                foreach (SortSpecification sortSpecification in sortSpecifications.OrderBy(s => int.Parse(s.varOrIl.description.par.Group)))
                {
                    double xv = sortSpecification.varOrIl.GetValue(x.hh, x.tu);
                    double yv = sortSpecification.varOrIl.GetValue(y.hh, y.tu);

                    if (xv > yv) return 1;
                    else if (xv < yv) return -1;
                }
                // else, sort on idperson
                return x.hh.GetPersonValue(indexIdPerson, x.tu[0].indexInHH).CompareTo(y.hh.GetPersonValue(indexIdPerson, y.tu[0].indexInHH));
            }
        }
    }
}
