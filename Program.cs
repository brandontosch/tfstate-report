using System;
using Newtonsoft.Json;
using tfstate_report.Models;

namespace tfstate_report
{
    class Program
    {
        static void Main(string[] args)
        {
            var reportType = args[0];
            var stateFile = args[1];
            var stateParser = new StateParser(stateFile);
            RoleStats[] roleStats = stateParser.ParseState();

            if (string.Equals(reportType, "summary", StringComparison.OrdinalIgnoreCase))
            {
                Summary summary = stateParser.GetSummary(roleStats);
                Console.Write(JsonConvert.SerializeObject(summary));
            }
            else
            {
                Console.Write(JsonConvert.SerializeObject(roleStats));
            }
        }
    }
}
