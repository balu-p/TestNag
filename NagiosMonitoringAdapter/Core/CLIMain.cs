using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace LytxNewRelicPluginCore
{
    using CommandLine;
    using CommandLine.Text;

    public class CLIMain
    {
        private static ILog logger = LogManager.GetLogger(typeof(CLIMain));

        public int Run(string[] args)
        {
            var options = new Options();

            if (Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    if (options.AppID != null)
                    {
                        if (options.APIKey == null || options.ApdexURL == null || options.ApdexURL.Length < 1)
                        {
                            Console.WriteLine("Error: if you pass a New Relic app ID you must pass a valid API key");
                            return 3;
                        }
                    }
                    else if (options.NRQLQuery != null)
                    {
                        if (options.QueryKey == null || options.NRQLURL == null || options.NRQLURL.Length < 1)
                        {
                            Console.WriteLine("Error: if you pass a New Relic query you must pass a valid query key");
                            return 3;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: app ID or query is required");
                        return 3;
                    }

                    NewRelicClient client = new NewRelicClient(options.QueryKey, options.APIKey, options.NRQLURL, options.ApdexURL);
                    Plugin plugin = new Plugin();
                    NagiosResponse response = plugin.Run(client, options);
                    Console.WriteLine(response.Message);
                    return response.ExitCode;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: exception occured {0}", e);
                    return 3;
                }
            }
            else
            {
                Console.WriteLine(options.GetUsage());
                return 3;
            }
        }
    }
}
