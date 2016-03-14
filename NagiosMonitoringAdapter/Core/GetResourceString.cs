using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace LytxNewRelicPluginCore
{
    public class GetResourceString
    {
          
          public string GetPropertyValue(string key)
          {
              switch (key)
              {
                  case "emptyinputparam":
                      return "Input Parameter cannot be empy";                 
                  case "inputinsuffforapdex":
                      return "Please check the supplied input params for Apdex ";
                  case "invalidappid":
                      return "Enter a valid AppID";
                  case "invalidapptype":
                      return "Enter a Valid Application Type (APDEX or NRQL)";
                  case "apdexgood":
                      return "Enter a valid decimal for Good Apdex";
                  case "warningapdex":
                      return "Enter a valid decimal for Warning Apdex";
                  case "criticalapdex":
                      return "Enter a valid decimal for Critical Apdex";
                  case "durationerr":
                      return "Enter a valid duaration";
                  case "heartbeat":
                      return "Enter a valid Hearbeat";
                  case "invalidindexerr":
                      return "Given status range values are not valid.";
                  case "invalidaccountid":
                      return "Enter  a valid account id";
                  case "invalidvariance":
                      return "Enter  a valid Variance";
                  case "invalidquery":
                      return "Enter  a valid NRQL Query";
                  default:
                      return "";
              }
          }
    }
}
