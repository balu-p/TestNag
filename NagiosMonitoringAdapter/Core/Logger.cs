using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LytxNewRelicPluginCore
{
    public class Logger
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));
        
        public void WriteLog(string message)
        {
            Console.WriteLine(message);
            log.Debug(message);

        }

        public void WriteInfolog(string message)
        {
            log.Info(message);
        }
    }
}
