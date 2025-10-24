using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class DataNow
    {
        public int IdPoint { get; set; }
        public int IdLine { get; set; }
        public string? NamePoint { get; set; }
        public DateTime TimeCheck { get; set; }
        public double Value { get; set; }
        public double? MinSpect { get; set; }
        public double? MaxSpect { get; set; }
        public string? Type { get; set; }
        public int? Alarm { get; set; }
    }
}
