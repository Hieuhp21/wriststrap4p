using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class HisAlarmLed
    {
        public int IdPoint { get; set; }
        public double MinSpect { get; set; }
        public double MaxSpect { get; set; }
        public double? OffsetValue { get; set; }
        public double? DeltaValue { get; set; }
        public double? Timeoff { get; set; }
        public double Enstatus { get; set; }
        public double MinSpectN { get; set; }
        public double MaxSpectN { get; set; }
        public double? OffsetValueN { get; set; }
        public double? DeltaValueN { get; set; }
        public double? TimeoffN { get; set; }
        public double? EnstatusN { get; set; }
        public DateTime TimeCheck { get; set; }
    }
}
