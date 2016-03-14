using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Timers;
using log4net;

namespace LytxNewRelicPluginCore
{
    public class ClsRESTCall
    {

        public string Appid { get; set; }
        public string APIKey { get; set; }
        public string URL { get; set; }
        private decimal IndexLevel { get; set; }
        private decimal HeartBeat { get; set; }
        private APPType App_Type { get; set; }
        public decimal GoodIndex { get; set; }
        public decimal WarningIndex { get; set; }
        public decimal CriticalIndex { get; set; }
        private int Duration { get; set; }
        public string[] Inputparams { get; set; }
        private int Defaultdays { get; set; }
        public string NRQLQuery { get; set; }

        ClsRestClient clnt;
        Timer restCallTimer = new Timer();
        private Logger logger = new Logger();
        private GetResourceString resourcestring = new GetResourceString();
        private const string emptyinputparam = "emptyinputparam";
        private const string invalidappid = "invalidappid";
        private const string invalidapptype = "invalidapptype";
        private const string durationerr = "durationerr";
        private const string apdexgood = "apdexgood";
        private const string warningapdex = "warningapdex";
        private const string criticalapdex = "criticalapdex";
        private const string heartbeaterr = "heartbeat";
        System.Configuration.AppSettingsReader app;
        private const string resultprefix = "SERVICE STATUS: ";
        private const string invalidindexerr = "invalidindexerr";
        private const string invalidaccountid = "invalidaccountid";
        private const string invalidvariance = "invalidvariance";
        private const string invalidquery = "invalidquery";

        private string OKMessage = string.Empty;
        private string WarningMessage = string.Empty;
        private string CriticalMessage = string.Empty;
        private string UnknownMessage = string.Empty;

        
        public ClsRESTCall()
        {
            try
            {
                app = new AppSettingsReader();
                string strdurdays = app.GetValue("ADDDAYS", typeof(String)).ToString();
                int durdays = 0;
                int.TryParse(strdurdays, out durdays);
                Defaultdays = durdays;
                OKMessage = app.GetValue("OKMESSAGE", typeof(String)).ToString();
                WarningMessage = app.GetValue("WARNINGMESSAGE", typeof(String)).ToString();
                CriticalMessage = app.GetValue("CRITICALMESSAGE", typeof(String)).ToString();
                UnknownMessage = app.GetValue("UNKNOWNMESSAGE", typeof(String)).ToString();               
                
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
            }

        }

        public void CalculateAppdex()
        {
            try
            {
                string consoleValue;
           
                if (Inputparams == null || Inputparams.Length == 0)  // Check Whether this app is called by External app or batch file..
                {
                    Console.WriteLine("Please enter Input parameters as comma seperated."
                                     + "\n -------------------For APDEX---------------------"
                                     + "\n1. Application Type.(APDEX or NRQL)\n2. APPID\n3. Duration in Minutes\n"
                                        + "4. Apdex Good\n5. Apdex Warning\n6. Apdex Critical"                  
                                        + "\n -------------------For NRQL---------------------"
                                         + "\n1. Application Type.(APDEX or NRQL)\n2. Account ID\n"
                                        + "3. NRQL Query\n4. Variance(%)");

                    //  "\n7. HeartBeat (Seconds)"   5. HeartBeat (Seconds)
                    consoleValue = Console.ReadLine();
                    logger.WriteInfolog("Value inputted " + consoleValue);
                    Inputparams = consoleValue.Split(new char[] { ',' });
                }
                if (GetAppType())
                {
                    if (App_Type == APPType.APDEX && ValidateInputAndAssignToParams())
                    {
                        InitializeRestParams();
                        clnt.EndPoint = URL.Replace("${APPID}", Appid);
                        Console.Title = app.GetValue("APDEXTITLE", typeof(String)).ToString();
                        CallAPI();
                    }
                    else if (App_Type == APPType.NRQL && ValidateInputAndAssignToParamsNRQL())
                    {
                        InitializeRestParams();                       
                        clnt.EndPoint = URL.Replace("{$ACCOUNTID}", Appid);
                        Console.Title = app.GetValue("NRQLTITLE", typeof(String)).ToString();
                        CallAPINRQL();
                    }
                    else
                    {
                        Inputparams = null;
                        CalculateAppdex();
                    }
                }
                else
                {
                    Inputparams = null;
                    CalculateAppdex();
                }

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.StackTrace);
            }
        }

        private void InitializeRestParams()
        {

            if (App_Type == APPType.APDEX)
            {
                URL = app.GetValue("APDEXURL", typeof(String)).ToString();
                APIKey = app.GetValue("APIKey", typeof(String)).ToString();
            }
            else
            {
                URL = app.GetValue("NRQLURL", typeof(String)).ToString();
                APIKey = app.GetValue("QUERYKEY", typeof(String)).ToString();
            }

            // Initialize REST Client
            clnt = new ClsRestClient();
            clnt.ContentType = "text/json";
            clnt.HeaderValue = APIKey;
            clnt.Method = HttpVerb.GET;
        }

        private bool GetAppType()
        {
            try
            {
                if (Inputparams.Length == 0 || !CheckStringAvailable(Inputparams, 0))
                {
                    logger.WriteLog(resourcestring.GetPropertyValue(emptyinputparam));
                    return false;
                }
                if (Inputparams[(int)InputvaluesApdex.Apptype] != APPType.APDEX.ToString() && Inputparams[(int)InputvaluesApdex.Apptype] != APPType.NRQL.ToString())
                {
                    logger.WriteLog(resourcestring.GetPropertyValue(invalidapptype));
                    return false;
                }
                else
                {
                    App_Type = Inputparams[(int)InputvaluesApdex.Apptype] == APPType.APDEX.ToString() ? APPType.APDEX : APPType.NRQL;
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
                return false;
            }
        }

        public bool ValidateInputAndAssignToParams()
        {
            bool result = false;
            int duration;
            decimal good, warning, critical, heartbeat;
            try
            {
                if (GetAppType())
                {

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.AppID))
                        Appid = Inputparams[(int)InputvaluesApdex.AppID];
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(invalidappid));

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.Duration) &&
                            int.TryParse(Inputparams[(int)InputvaluesApdex.Duration], out duration))
                    {
                        Duration = duration;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(durationerr));

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.Good) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesApdex.Good], out good))
                    {
                        GoodIndex = good;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(apdexgood));

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.Warning) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesApdex.Warning], out warning))
                    {
                        WarningIndex = warning;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(warningapdex));

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.Critical) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesApdex.Critical], out critical))
                    {
                        CriticalIndex = critical;
                        result = true;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(criticalapdex));

                  /*  if (CheckStringAvailable(Inputparams, (int)InputvaluesApdex.HeartBeat) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesApdex.HeartBeat], out heartbeat))
                    {
                        HeartBeat = heartbeat <= 0 ? 1 : heartbeat;
                        result = true;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(heartbeaterr)); */

                    HeartBeat = 1;

                    if (result)
                    {
                        if (!IsGivenIndexValid())
                        {
                            logger.WriteLog(resourcestring.GetPropertyValue(invalidindexerr));
                            result = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
            }
            return result;
        }

        public bool ValidateInputAndAssignToParamsNRQL()
        {
            bool result = false;
            int duration;
            decimal good, heartbeat;
            try
            {
                if (GetAppType())
                {

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesNRQL.AppID))
                        Appid = Inputparams[(int)InputvaluesApdex.AppID];
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(invalidaccountid));

                    /*if (CheckStringAvailable(Inputparams, (int)InputvaluesNRQL.Duration) &&
                            int.TryParse(Inputparams[(int)InputvaluesNRQL.Duration], out duration))
                    {
                        Duration = duration;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(durationerr));*/

                    if (CheckStringAvailable(Inputparams, (int)InputvaluesNRQL.Query) &&
                           Inputparams[(int)InputvaluesNRQL.Query].Length > 0)
                    {
                        NRQLQuery = Inputparams[(int)InputvaluesNRQL.Query];
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(invalidaccountid));


                    if (CheckStringAvailable(Inputparams, (int)InputvaluesNRQL.Variance) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesNRQL.Variance], out good))
                    {
                        GoodIndex = good;
                        result = true;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(invalidvariance));                   


                /*    if (CheckStringAvailable(Inputparams, (int)InputvaluesNRQL.HeartBeat) &&
                           decimal.TryParse(Inputparams[(int)InputvaluesNRQL.HeartBeat], out heartbeat))
                    {
                        HeartBeat = heartbeat <= 0 ? 1 : heartbeat;
                        result = true;
                    }
                    else
                        logger.WriteLog(resourcestring.GetPropertyValue(heartbeaterr)); */

                    HeartBeat = 1;

                    /*if (result)
                    {
                        if (!IsGivenIndexValid())
                        {
                            logger.WriteLog(resourcestring.GetPropertyValue(invalidindexerr));
                            result = false;
                        }
                    }*/
                    
                }

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
            }
            return result;
        }

        public bool IsGivenIndexValid()
        {
            if (GoodIndex <= WarningIndex || GoodIndex <= CriticalIndex)
                return false;
            if (WarningIndex <= CriticalIndex)
                return false;

            return true;
        }
        public bool CheckStringAvailable(string[] ipparams, int position)
        {
            if (ipparams != null && ipparams.Length - 1 >= position && ipparams[position].Length > 0)
                return true;
            else
                return false;
        }

        private void EnableTimer()
        {
            restCallTimer.Elapsed += restCallTimer_Elapsed;
            restCallTimer.Interval = (double)HeartBeat * 1000;
            restCallTimer.Enabled = true;
        }

        private void CallAPI()
        {
            APDexResult finalResult = APDexResult.Critical;
            try
            {

                //names[]=Appdex&names[]=Enduser/Appdex&values[]=score&from=2016-02-26T23:06:00+00:00&to=2016-02-26T23:36:00+00:00
                clnt.HeaderKey = "X-Api-Key";
                clnt.HeaderValue = APIKey;
                restCallTimer.Enabled = false;
                clnt.PostData = "?names[]=Apdex&names[]=EndUser/Apdex&values[]=score" + getUTCFromTo() + "&summarize=true";
                string output = clnt.MakeRequest(clnt.PostData);
                if (output != null && output.Length > 0)
                {
                    JObject scoreSearch = JObject.Parse(output);
                    IList<JToken> results = scoreSearch["metric_data"]["metrics"].Children().ToList();

                    decimal scoreVal = 0;
                    IList<JToken> jtresult = results[0]["timeslices"].Children().ToList();
                    string score = jtresult[0]["values"]["score"].ToString();
                    decimal.TryParse(score, out scoreVal);

                    EnableTimer();
                    finalResult = GetResult(scoreVal);
                    PrintResult(finalResult,string.Empty);
                }
                else
                {
                    EnableTimer();
                    PrintResult(APDexResult.Critical,string.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
                PrintResult(APDexResult.Critical,ex.Message);
            }
        }

        private void CallAPINRQL()
        {
            APDexResult finalResult = APDexResult.Critical;
            try
            {
                clnt.HeaderKey ="X-Query-Key";
                clnt.HeaderValue = APIKey;
                
                restCallTimer.Enabled = false;
                clnt.PostData = "?nrql=" + NRQLQuery;
                string output = clnt.MakeRequest(clnt.PostData);
                if (output != null && output.Length > 0)
                {
                    decimal scoreCurrVal = 0, scorePrevVal=0;
                    JObject scoreSearch = JObject.Parse(output);
                    decimal score;
                    
                    score = ProcessNRQLArrayVal(scoreSearch);
                    EnableTimer();
                    finalResult = GetResultNRQL(score);
                    PrintResult(finalResult,string.Empty);
                }
                else
                {
                    EnableTimer();
                    PrintResult(APDexResult.Critical,String.Empty);
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
                PrintResult(APDexResult.Critical,ex.Message);
            }
        }

        private decimal ProcessNRQLArrayVal(JObject scoreObj)
        {
            decimal score = 0;

            try
            {
                IList<JToken> jtresult = scoreObj["current"]["timeSeries"].ToList();
                List<NRQLScores> listCurrent = GetTimeSeriesArray(jtresult);
                jtresult = scoreObj["previous"]["timeSeries"].ToList();
                List<NRQLScores> listPrevious = GetTimeSeriesArray(jtresult);
                decimal previous = listPrevious.Count > 0 ? listPrevious[listPrevious.Count - 1].Count : 0;
                decimal current = listCurrent.Count > 0 ? listCurrent[listCurrent.Count - 1].Count : 0;
                score = CalculateNRWLScore(previous,current );
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex.Message);
                PrintResult(APDexResult.Critical, ex.Message);
            }
            return score;
        }

        private List<NRQLScores> GetTimeSeriesArray(IList<JToken> result)
        {
            List<NRQLScores> listscore = new List<NRQLScores>();
            // result[0]["results"].Children().ToList()[0]["count"]    result[0]["beginTimeSeconds"]
            foreach (JToken resultarr in result)
            {
                NRQLScores scores = new NRQLScores();
                scores.Count = Convert.ToDecimal(resultarr["results"].Children().ToList()[0]["count"]);
                listscore.Add(scores);
            }

            return listscore;
        }

        private void PrintResult(APDexResult appdex,string ErrorMessage)
        {
            string printMessage = string.Empty;
            if (appdex == APDexResult.OK)
                printMessage = OKMessage;
            else if (appdex == APDexResult.Warning)
                printMessage = WarningMessage;
            else
                printMessage = CriticalMessage;

            Console.WriteLine(printMessage + " - " + ErrorMessage + " shell returned " + ((int)appdex).ToString());

        }

        public APDexResult GetResult(decimal score)
        {
            if (score >= GoodIndex)
                return APDexResult.OK;
            else if (score >= WarningIndex && score < GoodIndex)
                return APDexResult.Warning;
            else if (score >= CriticalIndex && score < WarningIndex)
                return APDexResult.Critical;
            else
                return APDexResult.Unknown;
        }

        public decimal CalculateNRWLScore(decimal current, decimal previous)
        {
            decimal score = 0;

            if (previous != 0)
                score = (current - previous) * 100 / previous;
            else
                score = current * 100;

            return score;
        }

        public APDexResult GetResultNRQL(decimal score)
        {
            if (GoodIndex >= Math.Abs(score) )
                return APDexResult.OK;
            else
                return APDexResult.Critical;
        }


        private string getUTCFromTo()
        {
            string utcdate = string.Empty;
            string from = DateTime.Now.AddDays(Convert.ToInt32(Defaultdays) * -1).AddMinutes(Duration * -1).ToString("yyyy-MM-ddThh:mm:ss+00:00");
            string to = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss+00:00");
            return string.Concat("&from=", from, "&to=", to);
        }


        private void restCallTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (App_Type == APPType.APDEX)
                CallAPI();
            else
                CallAPINRQL();
        }


    }

    public enum APPType
    {
        APDEX,
        NRQL
    };

    public enum InputvaluesApdex
    {
        Apptype,
        AppID,
        Duration,
        Good,
        Warning,
        Critical,
        HeartBeat
    };

    public enum APDexResult
    {
        OK,
        Warning,
        Critical,
        Unknown
    };

    public enum InputvaluesNRQL
    {
        Apptype,
        AppID,
     //   Duration,
        Query,
        Variance,       
        HeartBeat
    }

    public class NRQLScores
    {
        public long BeginTime;
        public long EndTime;
        public decimal Count;
    }
}
