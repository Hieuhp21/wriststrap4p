using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class ListLine
    {
        public int IdLine { get; set; }
        public string? IdIviLine { get; set; }
        public string? NameLine { get; set; }
        public string? ListUser { get; set; }
        public int Floor { get; set; }
        public string? Note { get; set; }
        public double? Csstop { get; set; }
        public double? Cssleft { get; set; }
        public double? Csswidth { get; set; }
        public double? Cssheight { get; set; }
        public int? TotalPointAlarm { get; set; }
    }
}
