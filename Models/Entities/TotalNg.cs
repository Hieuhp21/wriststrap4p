using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class TotalNg
    {
        public int IdLog { get; set; }
        public int IdPoint { get; set; }
        public int IdLine { get; set; }
        public DateTime TimeCheck { get; set; }
        public double Value { get; set; }
        public double MinSpect { get; set; }
        public double MaxSpect { get; set; }
        public string? Alarm { get; set; }
        public DateTime? TimeStop { get; set; }
    }
}
