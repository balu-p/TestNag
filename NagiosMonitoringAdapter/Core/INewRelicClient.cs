using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public interface INewRelicClient
    {
        List<Metric> GetApdex(string appid, int windowMinutes);
        NRQLTimeseriesResult ExecuteNRQL(string accountid, string query);
    }
}
