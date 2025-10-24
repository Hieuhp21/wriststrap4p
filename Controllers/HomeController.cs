using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using WEB_SHOW_WRIST_STRAP.Models;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class HomeController : Controller
    {
        Dataconfig dataconfig = new Dataconfig();

        private readonly ILogger<HomeController> _logger;

        #region Alert

        public static int _idmess = 0;
        public static string _Mess = "";
        public static string _Username = "";

        protected void SetAlert(string message, int type)
        {
            TempData["AlertMessage"] = message;
            if (type == 1)
            {
                TempData["AlertType"] = "alert-success";
            }
            else if (type == 2)
            {
                TempData["AlertType"] = "alert-warning";
            }
            else if (type == 3)
            {
                TempData["AlertType"] = "alert-danger";
            }
            else
            {
                TempData["AlertType"] = "alert-info";
            }
        }

        public static void ResetMess()
        {
            _idmess = 0;
            _Mess = "";
        }

        #endregion

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        static Thread Test;
        static void AddDatasql()
        {
            while (true)
            {
                try
                {
                    string command = $"INSERT INTO Data_Now (id_led,Time0,Temp,Humi,alarm) VALUES ('1','{DateTime.Now}',{Math.Round((new Random()).NextDouble() * 5 + 20, 2)},{Math.Round((new Random()).NextDouble() * 10 + 70, 2)},'{(new Random()).NextInt64(7,9)}')";
                    using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                    {
                        c.Open();
                        var _insert = new SqlCommand(command, c);
                        _insert.ExecuteNonQuery();
                    }

                    command = $"INSERT INTO TotalDatum (id_led,Time0,Temp,Humi,min_temp,max_temp,min_humi,max_humi,alarm,note) VALUES ('1','{DateTime.Now}',{Math.Round((new Random()).NextDouble() * 5 + 20, 2)},{Math.Round((new Random()).NextDouble() * 10 + 70, 2)},22,33,44,55,'{(new Random()).NextInt64(0, 9)}','Test')";
                    using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                    {
                        c.Open();
                        var _insert = new SqlCommand(command, c);
                        _insert.ExecuteNonQuery();
                    }

                    command = $"INSERT INTO TotalDatum (id_led,Time0,Temp,Humi,min_temp,max_temp,min_humi,max_humi,alarm) VALUES ('2','{DateTime.Now}',{Math.Round((new Random()).NextDouble() * 5 + 20, 2)},{Math.Round((new Random()).NextDouble() * 10 + 70, 2)},22,33,44,55,'{(new Random()).NextInt64(0, 9)}')";
                    using (SqlConnection c = new SqlConnection(Dataconfig.ConnectString_Data))
                    {
                        c.Open();
                        var _insert = new SqlCommand(command, c);
                        _insert.ExecuteNonQuery();
                    }
                }
                catch { }

                Thread.Sleep(10000);
            }
        }

        [Authorize()]
        public IActionResult Index()
        {
            #region Alertshow
            if (User.Identity?.Name != null)
            {
                if (_Username != User.Identity?.Name)
                {
                    _idmess = 1; _Mess = "Login Success !";
                    _Username = User.Identity.Name;
                }

            }
            else if (_Username != "")
            {
                _Username = "";
                _idmess = 2; _Mess = "Logout Success !";
            }

            if (_idmess != 0) SetAlert(_Mess, _idmess);
            #endregion

            //if (Test == null)
            //{
            //    Test = new Thread(AddDatasql);
            //    Test.IsBackground = true;
            //    Test.Start();
            //}
            //if (!Test.IsAlive)
            //{
            //    Test.IsBackground = true;
            //    Test.Start();
            //}


            return View(dataconfig);
        }
        [Authorize()]
        public IActionResult IndexLeakVol()
        {
            #region Alertshow
            if (User.Identity?.Name != null)
            {
                if (_Username != User.Identity?.Name)
                {
                    _idmess = 1; _Mess = "Login Success !";
                    _Username = User.Identity.Name;
                }

            }
            else if (_Username != "")
            {
                _Username = "";
                _idmess = 2; _Mess = "Logout Success !";
            }

            if (_idmess != 0) SetAlert(_Mess, _idmess);
            #endregion

           
            return View(dataconfig);
        }
        [Authorize()]
        public IActionResult PieChart()
        {
            #region Alertshow
            if (User.Identity?.Name != null)
            {
                if (_Username != User.Identity?.Name)
                {
                    _idmess = 1; _Mess = "Login Success !";
                    _Username = User.Identity.Name;
                }

            }
            else if (_Username != "")
            {
                _Username = "";
                _idmess = 2; _Mess = "Logout Success !";
            }

            if (_idmess != 0) SetAlert(_Mess, _idmess);
            #endregion


            return View(dataconfig);
        }
        [Authorize()]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Editlayout()
        {
            return View(dataconfig);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}