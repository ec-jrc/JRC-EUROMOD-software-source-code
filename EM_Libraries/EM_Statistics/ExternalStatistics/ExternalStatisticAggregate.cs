using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Statistics.ExternalStatistics
{
    public class ExternalStatisticAggregate
    {
        internal string name;
        internal string incomeList;
        internal string description;
        internal string destination;
        internal string source;
        private string comment;
        private bool canBeDeleted;
        private Dictionary<string, ExternalStatisticAggregateValues> yearValues;

        public ExternalStatisticAggregate() {
            name = string.Empty;
            incomeList = string.Empty;
            description = string.Empty;
            destination = string.Empty;
            source = string.Empty;
            YearValues = new Dictionary<string, ExternalStatisticAggregateValues>();
            canBeDeleted = true;
            comment = string.Empty;
        }

        public ExternalStatisticAggregate(string incomeList, string name, string description, string source, string comment, string destination)
        {
            this.name = name;
            this.IncomeList = incomeList;
            this.Description = description;
            this.Source = source;
            this.Comment = comment;
            this.Destination = destination;
        }

        public string Name { get => name; set => name = value; }
        public string IncomeList { get => incomeList; set => incomeList = value; }
        public string Description { get => description; set => description = value; }
        public string Destination { get => destination; set => destination = value; }
        public string Source { get => source; set => source = value; }
        public bool CanBeDeleted { get => canBeDeleted; set => canBeDeleted = value; }
        public Dictionary<string, ExternalStatisticAggregateValues> YearValues { get => yearValues; set => yearValues = value; }
        public string Comment { get => comment; set => comment = value; }
    }

    public class ExternalStatisticAggregateValues
    {
        internal string amount = string.Empty;
        internal string beneficiares = string.Empty;
        internal string level = string.Empty;

        public ExternalStatisticAggregateValues() { }
        public string Amount { get => amount; set => amount = value; }
        public string Beneficiares { get => beneficiares; set => beneficiares = value; }
        public string Level { get => level; set => level = value; }

    }

}
