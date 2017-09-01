using System.Collections.Generic;

namespace tfstate_report.Models
{
    public class Summary
    {
        public Dictionary<string, Dictionary<string, int>> VMs { get; set; }
        public Dictionary<string, int> Storage { get; set; }
    }
}