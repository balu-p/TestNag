using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using log4net;


public enum HttpVerb
{
    GET,
    POST,
    PUT,
    DELETE
}

namespace LytxNewRelicPluginCore
{
  public class ClsRestClient
  {
      

    public string EndPoint { get; set; }
    public HttpVerb Method { get; set; }
    public string ContentType { get; set; }
    public string PostData { get; set; }

    private Logger logger = new Logger();

    public string HeaderKey { get; set; }

    public string HeaderValue { get; set; }



    public ClsRestClient()
    {

    }
    public ClsRestClient(string endpoint, HttpVerb method, string postData)
    {
      EndPoint = endpoint;
      Method = method;
      ContentType = "text/xml";
      PostData = postData;
      
    }


    public string MakeRequest()
    {
      return MakeRequest("");
    }

    public string MakeRequest(string parameters)
    {
        try
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;
            request.Headers.Add(HeaderKey, HeaderValue);

            if (!string.IsNullOrEmpty(PostData) && Method == HttpVerb.POST)
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }


                // grab the response
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var reader = new StreamReader(responseStream))
                        {
                            responseValue = reader.ReadToEnd();
                        }
                }

                return responseValue;
            }
        }
        catch (WebException ex)
        {
            using (var stream = ex.Response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                Console.WriteLine(reader.ReadToEnd());
                logger.WriteLog(ex.Message);
                return String.Empty;
            }
        }
        catch (Exception ex)
        {
            logger.WriteLog(ex.Message);
            return String.Empty;
        }
    }

  } // class

}
