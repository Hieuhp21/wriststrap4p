namespace WEB_SHOW_WRIST_STRAP
{
    public class Dataconfig
    {
        public static int RefckAlarmall { get; set; } = 3600; //ms

        public static string ConnectStirng_User { get; set; }
        public static string ConnectString_Data { get; set; }
        public int Duration { get; set; } = 3600 * 2 * 1000; //ms
        public int Refresh { get; set; } = 500; //ms
        public double XTempMax { get; set; } = 55.0f;
        public double XTempMin { get; set; } = 15.0f;
        public double XHumiMax { get; set; } = 90.0f;
        public double XHumiMin { get; set; } = 20.0f;
        public int TotalDatumFisrt { get; set; } = 130; //So diem
        public int TotalDatumTable { get; set; } = 60;
        public int HourTopAlarm { get; set; } = 12;
        public int TopAlarmmax { get; set; } = 50;
        public int HourTopAlarmall { get; set; } = 12;
        public int TopAlarmallmax { get; set; } = 50;
        public int amountpointload { get; set; } = 120 / 2;

        public int LimitchartHis { get; set; } = 300000;
    }
}
