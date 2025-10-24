using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Data;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class ShowChartController : Controller
    {
        
        private readonly DataledContext _context;

        public ShowChartController(DataledContext context)
        {
            _context = context;
        }

        Dataconfig dataconfig = new Dataconfig();

        [Authorize()]
        public IActionResult Index(string IdLed)
        {
            ViewBag.IdLed = IdLed != null ? IdLed : "";

            return View(dataconfig);
        }

        [Authorize()]
        [HttpPost]               
        public string GetData(int _idled)
        {
            try
            {
                DataTable dt = new DataTable();
                
                string command = $"select top 1 * from Data_Now where id_led = {_idled}";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }

                string _return = dt.ToJson();
                return _return;
            }
            catch { return ""; }



            //string str2 = JsonConvert.SerializeObject(dt, Formatting.Indented);
            // convert sang json
            //datas = JsonConvert.DeserializeObject<List<Datashow>>(str2, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        }

        [Authorize()]
        [HttpPost]
        public string GetDataFirst(int _idled)
        {
            try
            {
                DataTable dt = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select top {dataconfig.ToalDataFisrt} * from ToalData where id_led = {_idled} ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }
                string _return = dt.ToJson();
                return _return;
            }
            catch { return ""; }

            //string str2 = JsonConvert.SerializeObject(dt, Formatting.Indented);
            // convert sang json
            //datas = JsonConvert.DeserializeObject<List<Datashow>>(str2, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            
        }

        [Authorize()]
        [HttpPost]
        public string GetDataTable(int _idled)
        {
            try
            {
                DataTable dt = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select top {dataconfig.ToalDataTable} * from ToalData where id_led = {_idled} ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }
                string _return = dt.ToJson();
                return _return;
            }
            catch { return ""; }

            //string str2 = JsonConvert.SerializeObject(dt, Formatting.Indented);
            // convert sang json
            //datas = JsonConvert.DeserializeObject<List<Datashow>>(str2, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetListLeds()
        {
            List<ListLed> listLed = new List<ListLed>();
            try
            { 
                var result = await _context.ListLeds.ToListAsync();
                //var lsled = from led in result
                //            orderby led.IdLed descending
                //             select led;
                listLed = result;
            }
            catch {}
            return listLed.ToJson();
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetNameLeds(int idled)
        {
            string nameled = "Null";
            try
            {
                var result = await _context.ListLeds.ToListAsync();
                var lsled = from led in result
                            where led.IdLed == idled
                            select led.NameLed;
                if(lsled.Any())
                nameled = lsled.ToList()[0];
            }
            catch { }
            return nameled;
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetListRooms()
        {
            List<ListRoom> listrooms = new List<ListRoom>();
            try
            {
                var result = await _context.ListRooms.ToListAsync();
                //var lsroom = from room in result
                //            orderby room.IdRoom descending
                //            select room;
                listrooms = result;
            }
            catch { }
            return listrooms.ToJson();
        }

        [Authorize()]
        [HttpPost]
        public string GetDataTopAlarm(int _idled)
        {
            try
            {
                DateTime _timeckstart = DateTime.Now.AddHours(-dataconfig.HourTopAlarm);

                DataTable dt = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select top {dataconfig.TopAlarmmax} * from ToalData where Time0 >= '{_timeckstart}' and id_led = {_idled} and alarm != '' and alarm != '0' ORDER BY id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }

                DataTable dtnew = dt.Clone();
                dtnew.Clear();

                string alarmbf = "-8";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    if (row?[1] != "")
                    {
                        int idled = int.Parse(row[1]?.ToString());
                        string alarm = row[9]?.ToString();
                        if (idled != null && alarm != null)
                        {
                            if (alarmbf != alarm)
                            {
                                dtnew.Rows.Add(row.ItemArray);
                                alarmbf = alarm;
                            }
                        }
                    }
                }

                string _return = dtnew.ToJson();
                return _return;
            }
            catch { return ""; }
        }

        [Authorize()]
        [HttpPost]
        public string GetDataTopAlarmAll(string Allidled)
        {
            try
            {
                DateTime _timeckstart = DateTime.Now.AddHours(-dataconfig.HourTopAlarmall);

                DataTable dt = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select * from ToalData where Time0 >= '{_timeckstart}' and id_led in ({Allidled}) and alarm != '' and alarm != '0' ORDER BY id_led ASC, id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }

                DataTable dtnew = dt.Clone();
                dtnew.Clear();

                int idledbf = -1;
                string alarmbf = "-8";
                DateTime Timealarmbf = DateTime.MaxValue;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    if (row?[1] != "" )
                    {
                        int idled = int.Parse(row[1]?.ToString());
                        string alarm = row[9]?.ToString();
                        if(idled != null && alarm != null)
                        {
                            string Time0 = row["Time0"]?.ToString();
                            DateTime Timealarm;
                            if (DateTime.TryParse(Time0, out Timealarm))
                            {
                                if(idledbf != idled) Timealarmbf = DateTime.MaxValue;
                                if ((Timealarm - Timealarmbf).TotalMinutes < -10)
                                {
                                    alarmbf = "-8";
                                }
                                Timealarmbf = Timealarm;
                            }
                            if (idledbf != idled || alarmbf != alarm)
                            {
                                dtnew.Rows.Add(row.ItemArray);
                                idledbf = idled;
                                alarmbf = alarm;
                            }
                        }
                    }
                }

                //var reslut = from DataRow row in dtnew.Rows
                //             orderby row[2] descending
                //             select row;

                DataView dv = dtnew.DefaultView;
                dv.Sort = "Time0 DESC";

                dtnew = dv.ToTable();

                return dtnew.ToJson();
            }
            catch { return ""; }
        }

        [Authorize()]
        [HttpPost]
        public string GetAlarmleft(string Allidled,string _timeckstart)
        {
            try
            {
                _timeckstart = DateTime.Parse(_timeckstart).ToString("yyyy-MM-dd HH:mm:ss");

                DataTable dt = new DataTable();
                // lấy dữ liệu từ sql
                string command = $"select * from ToalData where Time0 >= '{_timeckstart}' and id_led in ({Allidled}) and alarm != '' and alarm != '0' ORDER BY id_led ASC, id_log DESC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }

                DataTable dtnew = dt.Clone();
                dtnew.Clear();

                int idledbf = -1;
                string alarmbf = "-8";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var row = dt.Rows[i];
                    if (row?[1] != "")
                    {
                        int idled = int.Parse(row[1]?.ToString());
                        string alarm = row[9]?.ToString();
                        if (idled != null && alarm != null)
                        {
                            if (idledbf != idled || alarmbf != alarm)
                            {
                                dtnew.Rows.Add(row.ItemArray);
                                idledbf = idled;
                                alarmbf = alarm;
                            }
                        }
                    }
                }

                string _return = dtnew.ToJson();

                return _return;
            }
            catch { return ""; }
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> Getstatusplcs()
        {
            string ck = "0";
            List<StatusPlc> listplcs = new List<StatusPlc>();
            try
            {
                var result = await _context.StatusPlcs.ToListAsync();
                var plcalarm = from plc in result
                               where plc.Alarm == true
                             select plc;
                if (plcalarm.Any()) ck = "Alarm";
            }
            catch { }
            return ck;
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> Getlistplcs()
        {
            List<StatusPlc> listplcs = new List<StatusPlc>();
            try
            {
                var result = await _context.StatusPlcs.ToListAsync();

                listplcs = result;
            }
            catch { }
            return listplcs.ToJson();
        }

        [Authorize()]
        [HttpPost]
        public string Getalldatanow(string Allidled)
        {
            try
            {
                DataTable dt = new DataTable();
                // lấy 5 dữ liệu từ sql
                string command = $"select * from Data_Now where id_led in ({Allidled}) ORDER BY id_led ASC";
                using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                {
                    c.Open();
                    SqlDataAdapter da = new SqlDataAdapter(new SqlCommand(command, c));
                    da.Fill(dt);
                }

                string _return = dt.ToJson();
                return _return;
            }
            catch { return ""; }

        }
    }
}
