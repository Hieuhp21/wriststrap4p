namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public class ListPlc
    {
        public int IdPlc { get; set; }
        public string IpPlc { get; set; }
        public string ActUnitType { get; set; }
        public string ActProtocol { get; set; }
        public int ActTimeOut { get; set; }
        public string? ActPassword { get; set; }
        public string? AddRead { get; set; }
        public string? NumberReads { get; set; }
        public string? AddReadV { get; set; }
        public string? NumberReadsV { get; set; }
        public int IdLine { get; set; }
        public int StatusUse { get; set; }

    }
}
