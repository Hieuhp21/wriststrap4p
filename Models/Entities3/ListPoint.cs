using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class ListPoint
    {
        public int IdPoint { get; set; }
        public int IdLine { get; set; }
        public string? NamePoint { get; set; }
        public double MinSpect { get; set; }
        public double MaxSpect { get; set; }
        public string? Addsread { get; set; }
        public string? Addswrite { get; set; }
        public int? Plc { get; set; }
        public double? OffsetValue { get; set; }
        public double? DeltaValue { get; set; }
        public double? Timeoff { get; set; }
        public double Enstatus { get; set; }
        public string? UserChange { get; set; }
        public DateTime? TimeChange { get; set; }
        public bool? Change { get; set; }
        public string? Note { get; set; }
        public double? Csstop { get; set; }
        public double? Cssleft { get; set; }
        public string? Type {  get; set; }
    }
}
