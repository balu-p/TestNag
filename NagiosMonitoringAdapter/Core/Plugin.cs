using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public class Plugin
    {
        private INewRelicClient client;
        private Options options;

        public NagiosResponse Run(INewRelicClient client, Options options)
        {
            this.options = options;
            this.client = client;

            if (options.AppID != null)
            {
                return CheckApdex(options.CriticalThreshold, options.WarnThreshold, options.AppID,  options.WindowMinutes);
            }
            else if (options.NRQLQuery != null)
            {
                return CheckNRQL(options.CriticalThreshold, options.WarnThreshold, options.NRQLQuery);
            }
            else
            {
                throw new Exception("Invalid option, app id or query are required");
            }
        }

        public NagiosResponse CheckApdex(double critical, double warning, string appid, int windowMinutes)
        {
            List<Metric> apdexMetrics = client.GetApdex(appid, windowMinutes);

            double apdex = 1.0;
            double enduserApdex = 1.0;

            apdexMetrics.ForEach(delegate(Metric metric) {
                if (metric.Name == "Apdex" && metric.TimeSlices.Count > 0)
                {
                    apdex = Convert.ToDouble(metric.TimeSlices[0].Values["score"]);
                }
                else if (metric.Name == "EndUser/Apdex" && metric.TimeSlices.Count > 0)
                {
                    enduserApdex = Convert.ToDouble(metric.TimeSlices[0].Values["score"]);
                }

            });

            if (apdex <= critical || enduserApdex <= critical)
            {
                return new NagiosResponse
                {
                    ExitCode = 2,
                    Message = String.Format("CRITICAL - Apdex for app {0} is above critical threshold app {1} <= {3} or browser {2} <= {3}",
                        appid, apdex, enduserApdex, critical),
                };
            }
            else if (apdex <= warning || enduserApdex <= warning)
            {
                return new NagiosResponse
                {
                    ExitCode = 1,
                    Message = String.Format("WARNING - Apdex for app {0} is above warning threshold app {1} <= {3} or browser {2} <= {3}",
                        appid, apdex, enduserApdex, warning),
                };
                
            }
            else
            {
                return new NagiosResponse
                {
                    ExitCode = 0,
                    Message = String.Format("OK - Apdex for app {0} is below warning threshold app {1} > {3} or browser {2} > {3}", appid, apdex, enduserApdex, warning),
                };
            }
        }

        public NagiosResponse CheckNRQL(double critical, double warning, string query)
        {
           var res = client.ExecuteNRQL(options.AccountID, query);
           var current = Convert.ToInt64(res.Current.TimeSeries[res.Current.TimeSeries.Count - 1].Results[0]["count"]);
           var previous = Convert.ToInt64(res.Previous.TimeSeries[res.Previous.TimeSeries.Count - 1].Results[0]["count"]);
           double pdiff = Math.Abs((1.0 * current - previous) / previous * 100.0);


           if (pdiff < 0 && Math.Abs(pdiff) >= Math.Abs(critical))
           {
               return new NagiosResponse
               {
                   ExitCode = 2,
                   Message = String.Format("CRITICAL - Variance for Account ID {0} is above critical threshold app {1} >= {2}",
                       options.AccountID, pdiff, critical),
               };
           }
           else if (pdiff < 0 && Math.Abs(pdiff) >= Math.Abs(warning))
           {
               return new NagiosResponse
               {
                   ExitCode = 1,
                   Message = String.Format("WARNING - Variance for Account ID {0} is above warning threshold app {1} >= {2}",
                       options.AccountID, pdiff, warning),
               };

           }
           else
           {
               return new NagiosResponse
               {
                   ExitCode = 0,
                   Message = String.Format("OK - Variance for Account ID {0} is below warning threshold app {1} >= {2}",
                         options.AccountID, pdiff, warning),
               };
           }

          
        }
    }
}
