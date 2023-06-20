using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Statistics.ExternalStatistics
{
    public class ExternalStatisticDistributional
    {
        private string name;
        private string tableName;
        private string description;
        private string source;
        private string comment;
        private Dictionary<string, string> yearValues;

        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public string Source { get => source; set => source = value; }
        public string Comment { get => comment; set => comment = value; }
        public Dictionary<string, string> YearValues { get => yearValues; set => yearValues = value; }
        public string TableName { get => tableName; set => tableName = value; }

        public ExternalStatisticDistributional()
        {
            Name = string.Empty;
            Description = string.Empty;
            Source = string.Empty;
            YearValues = new Dictionary<string, string>();
            Comment = string.Empty;
        }

        public ExternalStatisticDistributional(string tableName, string name, string description, string source, string comment)
        {
            this.TableName = tableName;
            this.Name = name;
            this.Description = description;
            this.Source = source;
            this.Comment = comment;
            this.YearValues = new Dictionary<string, string>();
        }
    }
}
