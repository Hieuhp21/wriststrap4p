using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class StatusPlc
    {
        public int IdPlc { get; set; }
        public string? NamePlc { get; set; }
        public bool StatusNow { get; set; }
        public bool Alarm { get; set; }
        public int? Sl { get; set; }
    }
}
