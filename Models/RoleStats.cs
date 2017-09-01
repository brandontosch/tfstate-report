namespace tfstate_report.Models
{
    public class RoleStats
    {
        public string Name { get; set; }
        public VMStats[] VMs { get; set; }
    }
}