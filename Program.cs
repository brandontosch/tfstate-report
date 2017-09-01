using System;
using Newtonsoft.Json;
using tfstate_report.Models;

namespace tfstate_report
{
    class Program
    {
        static void Main(string[] args)
        {
            string stateFile = args[0];
            StateParser stateParser = new StateParser(stateFile);
            RoleStats[] roleStats = stateParser.ParseState();
            Console.Write(JsonConvert.SerializeObject(roleStats));
        }
    }
}
