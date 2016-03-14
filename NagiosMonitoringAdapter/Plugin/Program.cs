using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;


namespace LytxNewRelicNagiosPluigin
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog logger = null;
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                logger = LogManager.GetLogger(typeof(Program));

                var m = new LytxNewRelicPluginCore.CLIMain();
                Environment.ExitCode = m.Run(args);
            }
            catch(Exception e)
            {
                if (logger != null)
                {
                    logger.Error("Unhandled exception", e);
                }
                Console.WriteLine("Unhandled exception: {0}", e.Message);
                Environment.ExitCode = 3;
            }

            /*
            try
            {
                log4net.Config.XmlConfigurator.Configure();                
                Console.SetWindowSize(80, 25);
                Console.Title = "Nagios Monitoring Adapter";
                ClsRESTCall rst = new ClsRESTCall();
                rst.Inputparams = args;
                Logger logger = new Logger();
                logger.WriteInfolog("Application instance started at " + DateTime.Now.ToLongDateString());

                rst.CalculateAppdex();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.WriteLog(ex.Message);
            }
             */
        }
    }


}
