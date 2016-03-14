using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public class NagiosResponse
    {
        public string Message { get; set; }
        public int ExitCode { get; set; }

        public NagiosResponse()
        {
        }

        public NagiosResponse(int exitCode, string msg)
        {
            ExitCode = exitCode;
            Message = msg;

        }     
    }
}
