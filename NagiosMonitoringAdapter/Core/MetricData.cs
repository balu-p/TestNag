using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public class TimeSlice
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Dictionary<String, String> Values { get; set; }
    }

    public class Metric
    {
        public string Name { get; set; }
        public List<TimeSlice> TimeSlices { get; set; }
    }

    public class MetricData
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public List<string> MetricsNotFound { get; set; }
        public List<string> MetricsFound { get; set; }
        public List<Metric> Metrics { get; set; }
    }

    public class MetricDataWrapper
    {
        public MetricData MetricData { get; set;}
    }
}
