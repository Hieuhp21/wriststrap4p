using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NuGet.Protocol;
using System.Data;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class CkHistoryController : Controller
    {
        Dataconfig dataconfig = new Dataconfig();

        [Authorize()]
        public IActionResult Index(string IdLog, string IdLed)
        {
            ViewBag.Dataconfig = dataconfig;

            ViewBag.IdLed = IdLed != null ? IdLed : "";

            ViewBag.IdLog = IdLog != null ? IdLog : "";

            return View();
        }

        [Authorize()]
        public IActionResult LogExport()
        {
            return View();
        }

        [Authorize()]
        [HttpPost]
        public string GetDataIdlog(int _idled, int _idlog)
        {
            try
            {
                DataTable dt1 = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select top {dataconfig.amountpointload} * from ToalData where id_log >= {_idlog} and id_led = {_idled} ORDER BY id_log ASC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt1);
                }

                DataTable dt2 = new DataTable();
                // lấy dữ liệu từ sql
                command = $"select top {dataconfig.amountpointload} * from ToalData where id_log < {_idlog} and id_led = {_idled} ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt2);
                }

                DataTable dt3 = new DataTable();
                dt3 = dt2.Clone();
                dt3.Clear();

                for(int i = dt2.Rows.Count - 1; i >= 0 ; i--)
                {
                    dt3.Rows.Add(dt2.Rows[i].ItemArray);
                }

                foreach(DataRow row in dt1.Rows)
                {
                    dt3.Rows.Add(row.ItemArray);
                }

                string _return = dt3.ToJson();
                return _return;
            }
            catch { return ""; }

            //string str2 = JsonConvert.SerializeObject(dt, Formatting.Indented);
            // convert sang json
            //datas = JsonConvert.DeserializeObject<List<Datashow>>(str2, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        }

        [Authorize()]
        [HttpPost]
        public string GetdataminTime(int idled, string timebegin, string timeend)
        {
            string _result = "";
            try
            {
                string Timebegin = DateTime.Parse(timebegin).ToString("yyyy-MM-dd HH:mm:ss");
                string Timeend = DateTime.Parse(timeend).ToString("yyyy-MM-dd HH:mm:ss");

                DataTable dt1 = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select * from ToalData where Time0 >= '{Timebegin}' and Time0 <= '{Timeend}' and id_led = {idled} ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt1);
                }

                _result = dt1.ToJson();

            }
            catch { }
            return _result;
        }

        [Authorize()]
        [HttpPost]
        public string GetTopalarminTime(int idled,string timebegin,string timeend)
        {
            string _result = "";
            try
            {
                string Timebegin = DateTime.Parse(timebegin).ToString("yyyy-MM-dd HH:mm:ss");
                string Timeend = DateTime.Parse(timeend).ToString("yyyy-MM-dd HH:mm:ss");

                DataTable dt1 = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select * from ToalData where Time0 >= '{Timebegin}' and Time0 <= '{Timeend}' and id_led = {idled}  and alarm != '' and alarm != '0' ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt1);
                }

                DataTable dtnew = dt1.Clone();
                dtnew.Clear();

                string alarmbf = "-8";
                DateTime Timealarmbf = DateTime.MaxValue;
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    var row = dt1.Rows[i];
                    if (row?[1] != "")
                    {
                        int idled1 = int.Parse(row[1]?.ToString());
                        string alarm = row[9]?.ToString();
                        if (idled1 != null && alarm != null)
                        {
                            string Time0 = row["Time0"]?.ToString();
                            DateTime Timealarm;
                            if(DateTime.TryParse(Time0,out Timealarm))
                            {
                                if ((Timealarm - Timealarmbf).TotalMinutes < -10)
                                {
                                    alarmbf = "-8";
                                }
                                Timealarmbf = Timealarm;
                            }
                            if (alarmbf != alarm)
                            {
                                dtnew.Rows.Add(row.ItemArray);
                                alarmbf = alarm;
                            }
                        }
                    }
                }

                _result = dtnew.ToJson();

            }
            catch { }
            return _result;
        }

        [Authorize()]
        [HttpPost]
        public string UpNoteAlarm(string txtAlarm,int Idlog = -92)
        {
            if(Idlog != -92 && txtAlarm != null)
            try
            {
                string command = $"UPDATE ToalData SET note = '{txtAlarm}' WHERE id_log = {Idlog}";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    var _insert = new SqlCommand(command, c);
                    _insert.ExecuteNonQuery();
                }
                return "Done Saved!";
            }
            catch
            {
                return "Error Save!";
            }
            else { return "Error Save!"; }
        }

        [HttpPost]
        [Authorize()]
        public string Getfullfilebyuser(string listroom)
        {
            string Json = PathtoJson.GetJstlscsv(@"D:\Export\", new string[] { "csv" }, 5, listroom);
            return Json;
        }
    }
}
