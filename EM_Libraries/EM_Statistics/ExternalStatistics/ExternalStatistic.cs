using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Statistics.ExternalStatistics
{
    public class ExternalStatistic
    {
        Dictionary<string, ExternalStatisticAggregate>  aggregate;
        Dictionary<string, ExternalStatisticDistributional> distributional;
        List<string> years;

        public ExternalStatistic()
        {
            Aggregate = new Dictionary<string, ExternalStatisticAggregate>();
            Distributional = new Dictionary<string, ExternalStatisticDistributional>();
            Years = new List<string>();
        }

        public List<string> Years { get => years; set => years = value; }
        public Dictionary<string, ExternalStatisticAggregate> Aggregate { get => aggregate; set => aggregate = value; }
        public Dictionary<string, ExternalStatisticDistributional> Distributional { get => distributional; set => distributional = value; }
    }
}
