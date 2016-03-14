using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public class TimeseriesResult {
        public int BeginTimeSeconds {get; set;}
        public int EndTimeSeconds {get; set;}
        public List<Dictionary<string, string> > Results { get; set; }
//        public List<ResultTotal> Results {get; set;}
    }

    public class Metadata
    {
        public long BeginTimeMillis {get; set;}
        public long EndTimeMillis {get; set;}
        public DateTime EndTime {get; set;}
        public DateTime BeginTime {get; set;}
        public long CompareWith {get; set;}
    }

    public class ResultTotal
    {
        public int Count {get; set;}
    }

    public class TimeseriesTotal {
        public List<ResultTotal> Results {get; set;}
    }

    public class ResultSet
    {
        public List<TimeseriesResult> TimeSeries {get; set;}
        public TimeseriesTotal Total {get; set;}
    }


    public class NRQLTimeseriesResult
    {
        public ResultSet Previous {get; set;}
        public ResultSet Current {get; set;}
        public Metadata Metadata {get; set;}
    }
}
