namespace WEB_SHOW_WRIST_STRAP.Models.DTO
{
    public class ReportDTO
    {
    }
    public class DailyMax
    {
        public int IdPoint { get; set; }
        public int IdLine { get; set; }
        public DateTime Date { get; set; }
        public float MaxValue { get; set; }
    }
}
