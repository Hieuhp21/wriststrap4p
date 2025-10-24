namespace WEB_SHOW_WRIST_STRAP.Core.ViewModels
{
    public class TotalDataViewModel
    {
        public int IdLog { get; set; }
        public string Alarm { get; set; }
        public string IdLine { get; set; }
        public string Note { get; set; }
        public DateTime TimeCheck { get; set; }
        public int? TotalTime { get; set; }
        public int IdPoint { get; set; }
        public string SourceTable { get; set; }
        //public int Status { get; set; }
        public int RepairStatus { get; set; }
    }
}
