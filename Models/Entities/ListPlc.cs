using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class ListPlc
    {
        public int IdPlc { get; set; }
        public string IpPlc { get; set; } = null!;
        public string ActUnitType { get; set; } = null!;
        public string ActProtocolType { get; set; } = null!;
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
