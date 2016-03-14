using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    using RestSharp;

    class NewRelicClient : INewRelicClient
    {
        private string QueryKey;
        private string APIKey;
        private Uri NRQLUri;
        private Uri ApdexUri;

        private RestClient apdexClient;
        private RestClient nrqlClient;

        private static string DateFormat = "yyyy-MM-ddTHH:mm:ss+00:00";

        public NewRelicClient(string queryKey, string APIKey, string NRQLUrl, string apdexUrl)
        {
            this.QueryKey = queryKey;
            this.APIKey = APIKey;
            this.NRQLUri = new Uri(NRQLUrl);
            this.ApdexUri = new Uri(apdexUrl);

            apdexClient = new RestClient(String.Format("{0}://{1}:{2}", ApdexUri.Scheme, ApdexUri.Host, ApdexUri.Port));

            nrqlClient = new RestClient(String.Format("{0}://{1}:{2}", NRQLUri.Scheme, NRQLUri.Host, NRQLUri.Port));
        }

        List<Metric> INewRelicClient.GetApdex(string appid, int windowMinutes)
        {
            var now = DateTime.UtcNow;
            var from = now.AddMinutes(-windowMinutes);

            var req = new RestRequest(String.Format("{0}", ApdexUri.PathAndQuery.Replace("$%7BAPPID%7D", appid)));
            req.AddHeader("X-Api-Key", APIKey);
            req.AddParameter("names[]", "Apdex");
            req.AddParameter("names[]", "EndUser/Apdex");
            req.AddParameter("values[]", "score");
            req.AddParameter("summarize", "true");
            req.AddParameter("from", from.ToString(DateFormat));
            req.AddParameter("to", now.ToString(DateFormat));

            var res = apdexClient.Execute<MetricDataWrapper>(req);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return res.Data.MetricData.Metrics;
            }

            throw new Exception("Error: unable to retriev apdex for appid");
        }

        NRQLTimeseriesResult INewRelicClient.ExecuteNRQL(string accountid, string query)
        {
            var req = new RestRequest(String.Format("{0}", NRQLUri.PathAndQuery.Replace("$%7BACCOUNTID%7D", accountid)));
            req.AddHeader("X-Query-Key", QueryKey);
            req.AddParameter("nrql", query);
            req.Method = Method.GET;
            req.RequestFormat = DataFormat.Json;

            var res = nrqlClient.Execute<NRQLTimeseriesResult>(req);
            if (res.StatusCode == System.Net.HttpStatusCode.OK && res.Data != null)
            {
                return res.Data;
            }

            throw new Exception(String.Format("Error: unable to retriev NRQL results for account ID {0}", accountid));
        }
    }
}
