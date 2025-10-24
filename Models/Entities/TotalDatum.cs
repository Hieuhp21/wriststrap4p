using System;
using System.Collections.Generic;

namespace WEB_SHOW_WRIST_STRAP.Models.Entities
{
    public partial class TotalDatum
    {
        public int IdLog { get; set; }
        public int IdPoint { get; set; }
        public int IdLine { get; set; }
        public DateTime TimeCheck { get; set; }
        public double Value { get; set; }
        public double MinSpect { get; set; }
        public double MaxSpect { get; set; }
        public string? Alarm { get; set; }
        public string? Note { get; set; }
        public double? OldValue { get; set; }
        public int? TotalTime { get; set; }
        public DateTime? TimeStop { get; set; }
        public int Status { get; set; }
    }
    public class ExportLogDto
    {
        public DateTime TimeCheck { get; set; }         // Cột 1
        public string NameLine { get; set; }            // Cột 2
        public string NamePoint { get; set; }           // Cột 3
        public string Alarm { get; set; }                  // Cột 4 (GetStatus sẽ dùng int Alarm để hiển thị "OK"/"NG")
        public double Value { get; set; }               // Cột 5
        public double? TotalTime { get; set; }          // Cột 6 (sẽ format sang hh:mm:ss)
        public DateTime? TimeStop { get; set; }         // Cột 7
        public string Note { get; set; }                // Cột 8
    }
}
