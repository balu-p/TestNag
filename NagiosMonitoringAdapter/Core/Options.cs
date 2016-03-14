using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommandLine;
using CommandLine.Text;

namespace LytxNewRelicPluginCore
{
    public class Options
    {
        [Option('c', "critical", Required = true, HelpText = "When apdex or variance is greater than value the plugin will flag critical")]
        public double CriticalThreshold { get; set; }

        [Option('w', "warn", Required = false, HelpText = "When apdex or variance is greater than value the plugin will flag critical", DefaultValue=10)]
        public double WarnThreshold { get; set; }

        [Option("apdex-url", Required = false, HelpText = "Override the URL used to retrieve apdex scores from New Relic", DefaultValue="https://api.newrelic.com/v2/applications/${APPID}/metrics/data.json")]
        public string ApdexURL { get; set; }

        [Option("app-id", Required = false, HelpText = "The New Relic app id to query apdex scores from", DefaultValue = null)]
        public string AppID { get; set; }

        [Option('a', "api-key", Required = false, HelpText = "The New Relic API key to use", DefaultValue = null)]
        public string APIKey { get; set; }

        [Option("nrql-url", Required = false, HelpText = "Override the URL used to execute NRQL requests", DefaultValue="https://insights-api.newrelic.com/v1/accounts/${ACCOUNTID}/query")]
        public string NRQLURL { get; set; }

        [Option("account-id", Required = false, HelpText = "Account id to execute NRQL with")]
        public string AccountID { get; set; }

        [Option('q', "query-key", Required = false, HelpText = "The New Relic query key for executing NRQL", DefaultValue = null)]
        public string QueryKey { get; set; }

        [Option("query", Required = false, HelpText = "The NRQL query to execute", DefaultValue = null)]
        public string NRQLQuery { get; set; }

        [Option("window-minutes", Required = false, HelpText = "The window size to use in minutes (will use now - minutes to now)", DefaultValue = 30)]
        public int WindowMinutes { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
