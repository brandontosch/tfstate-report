namespace tfstate_report.Models
{
    public class VMStats
    {
        public string Size { get; set; }
        public string OS { get; set; }
        public DiskStats[] Disks { get; set; }
    }
}