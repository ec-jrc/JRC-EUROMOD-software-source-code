using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunDefOutput : FunOutOfSpineBase
    {
        internal FunDefOutput(InfoStore infoStore) : base(infoStore) { }

        string fileName;
        bool append;
        bool doReplaceUnitLoopVoidBy = false;
        bool doExchangeConversion = false;
        bool suppressVoidMessage = false;
        double replaceUnitLoopVoidBy = double.NaN;
        int nDecimals;
        double multiplyMonetaryBy = 1.0;

        ParCateg parWho = null;
        ParVar parEligVar = null;
        int indexEligVar = -1;

        Dictionary<string, ParVarIL> outList = new Dictionary<string, ParVarIL>();

        private class TUInfo // used for handling UnitInfo-parameter
        {
            internal string tuName;
            internal Dictionary<string, Func<HH, List<Person>, Person, double>> getInfos = // key: type of unit-info on individual level (e.g. IsDepChild)
                 new Dictionary<string, Func<HH, List<Person>, Person, double>>();         // value: function to obtain the info, see DoFunWork and PrepareOutPar
        }
        List<TUInfo> tuInfoGroups = new List<TUInfo>();

        private int indexIDHH;

        // functions used in TUInfo
        private double tuInfoIsChild(HH hh, List<Person> tu, Person person) { return person.isDepChild ? 1 : 0; }
        private double tuInfoIsPartner(HH hh, List<Person> tu, Person person) { return person.isPartner ? 1 : 0; }
        private double tuInfoIsOwnChild(HH hh, List<Person> tu, Person person) { return person.isOwnChild ? 1 : 0; }
        private double tuInfoIsOwnDepChild(HH hh, List<Person> tu, Person person) { return person.isOwnDepChild ? 1 : 0; }
        private double tuInfoIsDepParent(HH hh, List<Person> tu, Person person) { return person.isDepParent ? 1 : 0; }
        private double tuInfoIsDepRelative(HH hh, List<Person> tu, Person person) { return person.isDepRelative ? 1 : 0; }
        private double tuInfoIsLoneParent(HH hh, List<Person> tu, Person person) { return person.isLoneParent ? 1 : 0; }
        private double tuInfoGetHeadId(HH hh, List<Person> tu, Person person) { return person.tuHeadID; }
        private double tuInfoIsCohabiting(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsCohabiting(infoStore), hh, tu, person); }
        private double tuInfoIsWithPartner(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsWithPartner(infoStore), hh, tu, person); }
        private double tuInfoIsParent(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsParent(infoStore), hh, tu, person); }
        private double tuInfoIsParentOfDepChild(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsParentOfDepChild(infoStore), hh, tu, person); }
        private double tuInfoIsInEducation(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsInEducation(infoStore), hh, tu, person); }
        private double tuInfoIsLooseDepChild(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsLooseDepChild(infoStore), hh, tu, person); }
        private double tuInfoIsLoneParentOfDepChild(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryIsLoneParentOfDepChild(infoStore), hh, tu, person); }
        private double tuInfoNPersInUnit(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryNPersInUnit(infoStore), hh, tu, person); }
        private double tuInfoNDepChildrenInTu(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryNDepChildrenInTu(infoStore), hh, tu, person); }
        private double tuInfoNDepParentsInTu(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryNDepParentsInTu(infoStore), hh, tu, person); }
        private double tuInfoNDepRelativesInTu(HH hh, List<Person> tu, Person person) { return tuInfoQueryCall(new QueryNDepRelativesInTu(infoStore), hh, tu, person); }

        private double tuInfoQueryCall(QueryBase query, HH hh, List<Person> tu, Person person) { query.PrepareVarIndices(); return query.GetValue(hh, tu, person); }
    }
}
