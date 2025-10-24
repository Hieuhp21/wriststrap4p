using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WEB_SHOW_WRIST_STRAP.Models.Entities;
using WEB_SHOW_WRIST_STRAP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Microsoft.Data.SqlClient;
using WEB_SHOW_WRIST_STRAP.Models.DTO;
using WEB_SHOW_WRIST_STRAP.Configs;
using Dapper;
using System.Data;

public class ListViewController : Controller
{
    private readonly DataPointContext _context;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public ListViewController(DataPointContext context, IDbConnectionFactory dbConnectionFactory)  // Thêm param
    {
        _context = context;
        _dbConnectionFactory = dbConnectionFactory;
    }
    Dataconfig dataconfig = new Dataconfig();

    // Danh sách cụm cố định (hard-coded)
    private readonly Dictionary<int, List<int>> _clusters = new Dictionary<int, List<int>>
    {
        { 1, new List<int> { 1, 2, 3,4 } },       // Cụm 1: Line 1-3
        { 2, new List<int> { 5, 6,7,8,9 } },       // Cụm 2: Line 4-6
        { 3, new List<int> { 10,11,12} },       // Cụm 3: Line 7-9
        { 4, new List<int> { 13,14 } },
        { 5, new List<int> { 15} },
        { 6, new List<int> { 16 } }
    };
   

    public IActionResult Index(int IdPoint = 0, int IdLine = 0)
    {
        ViewBag.IdPoint = IdPoint;
        ViewBag.IdLine = IdLine;

        return View(dataconfig);
    }
    public IActionResult IndexLeakVol(int IdPoint = 0, int IdLine = 0)
    {
        ViewBag.IdPoint = IdPoint;
        ViewBag.IdLine = IdLine;

        return View(dataconfig);
    }
    // Action Detail
    public IActionResult Detail(int IdLine = 0)
    {
        // Tìm cụm chứa IdLine
        var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(IdLine));
        var linesInCluster = cluster.Value != null ? cluster.Value : new List<int> { IdLine }; // Fallback nếu không tìm thấy cụm

        ViewBag.IdCluster = cluster.Key; // IdCluster (có thể null nếu không tìm thấy)
        ViewBag.Lines = linesInCluster;  // Danh sách IdLine trong cụm
        ViewBag.IdLine = IdLine;         // IdLine gốc để hiển thị

        return View(dataconfig);
    }
    public IActionResult EditLayoutLine(int IdLine = 0)
    {
        var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(IdLine));
        var linesInCluster = cluster.Value != null ? cluster.Value : new List<int> { IdLine };

        ViewBag.IdCluster = cluster.Key;
        ViewBag.Lines = linesInCluster;
        ViewBag.IdLine = IdLine;

        return View(dataconfig);
    }
    // Lấy danh sách điểm cho toàn bộ cụm
    [HttpGet]
    public JsonResult GetPointLine(int idLine)
    {
        var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(idLine));
        var lines = cluster.Value != null ? cluster.Value : new List<int> { idLine };

        var points = _context.ListPoints
            .Where(p => lines.Contains(p.IdLine) && p.Hide != true)
            .Select(p => new
            {
                p.IdPoint,
                p.NamePoint,
                p.Csstop,
                p.Cssleft,
                p.TopDetail,
                p.LeftDetail,
                p.IdLine
            }).ToList();

        return Json(points);
    }
    [HttpGet]
    public JsonResult GetPointLine2(int idLine)
    {
        var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(idLine));
        var lines = cluster.Value != null ? cluster.Value : new List<int> { idLine };

        var points = _context.ListPoints2
           
            .Select(p => new
            {
                p.IdPoint,
                p.NamePoint,
                p.Csstop,
                p.Cssleft,
                p.TopDetail,
                p.LeftDetail,
                p.IdLine
            }).ToList();

        return Json(points);
    }

    // Cập nhật trạng thái cho toàn bộ cụm
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UpdateStatus(int idLine)
    {
        try
        {
            // Lấy danh sách lines từ cluster
            var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(idLine));
            var lines = cluster.Value != null ? cluster.Value : new List<int> { idLine };

            // Lấy chỉ các ListPoint có Type = 2
            var type2Points = await _context.ListPoints
                .AsNoTracking()
                .Where(p => p.Type == "2" && lines.Contains(p.IdLine))
                .Select(p => new { p.IdPoint, p.IdLine })
                .ToListAsync();

            // Tạo HashSet với cặp IdPoint, IdLine
            var type2PointSet = new HashSet<(int IdPoint, int IdLine)>(
                type2Points.Select(p => (p.IdPoint, p.IdLine))
            );

            // Lấy dữ liệu từ DataNows
            var latestData = await _context.DataNows
                .AsNoTracking()
                .Where(d => lines.Contains(d.IdLine))
                .OrderByDescending(d => d.TimeCheck)
                .Select(d => new
                {
                    d.IdPoint,
                    d.IdLine,
                    d.TimeCheck,
                    d.Value,
                    d.MinSpect,
                    d.MaxSpect,
                    d.Alarm
                })
                .ToListAsync();

            // Gán Alarm = 7 trong bộ nhớ
            var result = latestData.Select(d => new
            {
                d.IdPoint,
                d.IdLine,
                d.TimeCheck,
                d.Value,
                d.MinSpect,
                d.MaxSpect,
                Alarm = type2PointSet.Contains((d.IdPoint, d.IdLine)) ? 7 : d.Alarm
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An error occurred while updating status." });
        }
    }



    // Cập nhật vị trí điểm cho toàn bộ cụm
    [HttpPost]
    public IActionResult UpdatePointPositions([FromBody] List<ListPoint> points)
    {
        foreach (var point in points)
        {
            var existingPoint = _context.ListPoints
                .FirstOrDefault(p => p.IdPoint == point.IdPoint && p.IdLine == point.IdLine);
            if (existingPoint != null)
            {
                existingPoint.TopDetail = point.Csstop;  // Lưu vào TopDetail
                existingPoint.LeftDetail = point.Cssleft; // Lưu vào LeftDetail
            }
        }
        _context.SaveChanges();
        return Ok();
    }
    [HttpPost]
    public IActionResult UpdatePointPositions2([FromBody] List<ListPoint> points)
    {
        foreach (var point in points)
        {
            var existingPoint = _context.ListPoints2
                .FirstOrDefault(p => p.IdPoint == point.IdPoint && p.IdLine == point.IdLine);
            if (existingPoint != null)
            {
                existingPoint.TopDetail = point.Csstop;  // Lưu vào TopDetail
                existingPoint.LeftDetail = point.Cssleft; // Lưu vào LeftDetail
            }
        }
        _context.SaveChanges();
        return Ok();
    }
    [HttpGet]
    public async Task<JsonResult> GetLogData(int idLine)
    {
        try
        {
            var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(idLine));
            var lines = cluster.Value != null ? cluster.Value : new List<int> { idLine };
            if (idLine == 0)
            {
                lines = new List<int> { };
            }
            var now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1).Date; // Buffer cho weeks cross month
            var toDate = now.Date;
            var limitTime = now;
            var idLinesStr = lines.Any() ? string.Join(",", lines) : "";

            // Dapper: Gọi stored procedure
            using var conn = _dbConnectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@IdLines", idLinesStr);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@LimitTime", limitTime);

            var dailyData = (await conn.QueryAsync<DailyMax>(
                "GetDailyMax",
                parameters,
                commandType: CommandType.StoredProcedure
            )).AsList();

            // Xử lý dữ liệu: Lấy nameLines từ EF Core (query đơn giản)
            var nameLines = _context.ListLines
                .Where(l => lines.Count == 0 || lines.Contains(l.IdLine))
                .ToDictionary(l => l.IdLine, l => l.NameLine);

            var minWeek = calendar.GetWeekOfYear(now.AddDays(-28), CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var last4Weeks = dailyData
                .Select(d => new
                {
                    d,
                    Week = calendar.GetWeekOfYear(d.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Where(x => x.Week >= minWeek)
                .GroupBy(x => new { x.Week, x.d.IdPoint, x.d.IdLine })
                .Select(g => new
                {
                    Type = "Week",
                    Date = "W" + g.Key.Week.ToString(),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(x => x.d.MaxValue),
                    nameLine = nameLines.ContainsKey(g.Key.IdLine) ? nameLines[g.Key.IdLine] : null
                }).ToList();

            var last3Days = dailyData
                .Where(d => d.Date >= now.AddDays(-7))
                .Select(d => new
                {
                    Type = "Day",
                    Date = d.Date.ToString("yyyy-MM-dd"),
                    d.IdPoint,
                    d.IdLine,
                    AvgValue = d.MaxValue,
                    nameLine = nameLines.ContainsKey(d.IdLine) ? nameLines[d.IdLine] : null
                }).ToList();

            var currentMonthData = dailyData
                .Where(d => d.Date.Month == currentMonth && d.Date.Year == currentYear)
                .GroupBy(d => new { d.Date.Year, d.Date.Month, d.IdPoint, d.IdLine })
                .Select(g => new
                {
                    Type = "Month",
                    Date = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(d => d.MaxValue),
                    nameLine = nameLines.ContainsKey(g.Key.IdLine) ? nameLines[g.Key.IdLine] : null
                }).ToList();

            var result = last3Days.Concat(last4Weeks).Concat(currentMonthData).ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stack = ex.StackTrace });
        }
    }
    [HttpGet]
    public async Task<JsonResult> GetLogDataHistory(int idLine, DateTime startTime, DateTime endTime)
    {
        try
        {
            var lines = new List<int> { idLine };
            if (idLine == 0)
            {
                lines = new List<int> { };
            }
            // Lấy thời gian kết thúc làm mốc
            var now = endTime;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(-3).Date; // Buffer cho months và weeks
            var toDate = now.Date;
            var limitTime = now;
            var idLinesStr = lines.Any() ? string.Join(",", lines) : "";

            // Dapper: Gọi stored procedure
            using var conn = _dbConnectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@IdLines", idLinesStr);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@LimitTime", limitTime);

            var dailyData = (await conn.QueryAsync<DailyMax>(
                "GetDailyMax",
                parameters,
                commandType: CommandType.StoredProcedure
            )).AsList();

            var minWeek = calendar.GetWeekOfYear(now.AddDays(-28), CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var last4Weeks = dailyData
                .Select(d => new
                {
                    d,
                    Week = calendar.GetWeekOfYear(d.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Where(x => x.Week >= minWeek)
                .GroupBy(x => new { x.Week, x.d.IdPoint, x.d.IdLine })
                .Select(g => new
                {
                    Type = "Week",
                    Date = "W" + g.Key.Week.ToString(),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(x => x.d.MaxValue)
                }).ToList();

            var last3Days = dailyData
                .Where(d => d.Date >= now.AddDays(-7) && d.Date <= now.Date)
                .Select(d => new
                {
                    Type = "Day",
                    Date = d.Date.ToString("yyyy-MM-dd"),
                    d.IdPoint,
                    d.IdLine,
                    AvgValue = d.MaxValue,
                }).ToList();

            var currentMonthData = dailyData
                .Where(d => d.Date.Month > currentMonth - 3 && d.Date.Month <= currentMonth && d.Date.Year == currentYear)
                .GroupBy(d => new { d.Date.Year, d.Date.Month, d.IdPoint, d.IdLine })
                .Select(g => new
                {
                    Type = "Month",
                    Date = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(d => d.MaxValue)
                }).ToList();

            var result = last3Days.Concat(last4Weeks).Concat(currentMonthData).ToList();
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stack = ex.StackTrace });
        }
    }
    [HttpGet]
    public async Task<JsonResult> GetLogData2(int idLine)
    {
        try
        {
            var cluster = _clusters.FirstOrDefault(c => c.Value.Contains(idLine));
            var lines = cluster.Value != null ? cluster.Value : new List<int> { idLine };
            if (idLine == 0)
            {
                lines = new List<int> { };
            }
            var now = DateTime.Now;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1).Date; // Buffer cho weeks cross month
            var toDate = now.Date;
            var limitTime = now;
            var idLinesStr = lines.Any() ? string.Join(",", lines) : "";

            // Dapper: Gọi stored procedure mới
            using var conn = _dbConnectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@IdLines", idLinesStr);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@LimitTime", limitTime);

            var dailyData = (await conn.QueryAsync<DailyMax>(
                "GetDailyCountNG",  // Thay tên SP
                parameters,
                commandType: CommandType.StoredProcedure
            )).AsList();

            // Xử lý dữ liệu: Lấy nameLines từ EF Core (query đơn giản)
            var nameLines = _context.ListLines
                .Where(l => lines.Count == 0 || lines.Contains(l.IdLine))
                .ToDictionary(l => l.IdLine, l => l.NameLine);

            var minWeek = calendar.GetWeekOfYear(now.AddDays(-28), CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var last4Weeks = dailyData
                .Select(d => new
                {
                    d,
                    Week = calendar.GetWeekOfYear(d.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Where(x => x.Week >= minWeek)
                .GroupBy(x => new { x.Week, x.d.IdPoint, x.d.IdLine })
                .Select(g => new
                {
                    Type = "Week",
                    Date = "W" + g.Key.Week.ToString(),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(x => x.d.MaxValue),  // Giữ max aggregate, nhưng dữ liệu gốc là count
                    nameLine = nameLines.ContainsKey(g.Key.IdLine) ? nameLines[g.Key.IdLine] : null
                }).ToList();

            var last3Days = dailyData
                .Where(d => d.Date >= now.AddDays(-7))
                .Select(d => new
                {
                    Type = "Day",
                    Date = d.Date.ToString("yyyy-MM-dd"),
                    d.IdPoint,
                    d.IdLine,
                    AvgValue = d.MaxValue,
                    nameLine = nameLines.ContainsKey(d.IdLine) ? nameLines[d.IdLine] : null
                }).ToList();

            var currentMonthData = dailyData
                .Where(d => d.Date.Month == currentMonth && d.Date.Year == currentYear)
                .GroupBy(d => new { d.Date.Year, d.Date.Month, d.IdPoint, d.IdLine })
                .Select(g => new
                {
                    Type = "Month",
                    Date = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(d => d.MaxValue),
                    nameLine = nameLines.ContainsKey(g.Key.IdLine) ? nameLines[g.Key.IdLine] : null
                }).ToList();

            var result = last3Days.Concat(last4Weeks).Concat(currentMonthData).ToList();

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stack = ex.StackTrace });
        }
    }
    [HttpGet]
    public async Task<JsonResult> GetLogDataHistory2(int idLine, DateTime startTime, DateTime endTime)
    {
        try
        {
            var lines = new List<int> { idLine };
            if (idLine == 0)
            {
                lines = new List<int> { };
            }
            // Lấy thời gian kết thúc làm mốc
            var now = endTime;
            int currentMonth = now.Month;
            int currentYear = now.Year;
            var calendar = CultureInfo.InvariantCulture.Calendar;

            var fromDate = new DateTime(now.Year, now.Month, 1).AddMonths(-3).Date; // Buffer cho months và weeks
            var toDate = now.Date;
            var limitTime = now;
            var idLinesStr = lines.Any() ? string.Join(",", lines) : "";

            // Dapper: Gọi stored procedure mới
            using var conn = _dbConnectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            var parameters = new DynamicParameters();
            parameters.Add("@IdLines", idLinesStr);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);
            parameters.Add("@LimitTime", limitTime);

            var dailyData = (await conn.QueryAsync<DailyMax>(
                "GetDailyCountNG",  // Thay tên SP
                parameters,
                commandType: CommandType.StoredProcedure
            )).AsList();

            var minWeek = calendar.GetWeekOfYear(now.AddDays(-28), CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            var last4Weeks = dailyData
                .Select(d => new
                {
                    d,
                    Week = calendar.GetWeekOfYear(d.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
                })
                .Where(x => x.Week >= minWeek)
                .GroupBy(x => new { x.Week, x.d.IdPoint, x.d.IdLine })
                .Select(g => new
                {
                    Type = "Week",
                    Date = "W" + g.Key.Week.ToString(),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(x => x.d.MaxValue)  // Giữ max aggregate, nhưng dữ liệu gốc là count
                }).ToList();

            var last3Days = dailyData
                .Where(d => d.Date >= now.AddDays(-7) && d.Date <= now.Date)
                .Select(d => new
                {
                    Type = "Day",
                    Date = d.Date.ToString("yyyy-MM-dd"),
                    d.IdPoint,
                    d.IdLine,
                    AvgValue = d.MaxValue,
                }).ToList();

            var currentMonthData = dailyData
                .Where(d => d.Date.Month > currentMonth - 3 && d.Date.Month <= currentMonth && d.Date.Year == currentYear)
                .GroupBy(d => new { d.Date.Year, d.Date.Month, d.IdPoint, d.IdLine })
                .Select(g => new
                {
                    Type = "Month",
                    Date = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    g.Key.IdPoint,
                    g.Key.IdLine,
                    AvgValue = g.Max(d => d.MaxValue)
                }).ToList();

            var result = last3Days.Concat(last4Weeks).Concat(currentMonthData).ToList();
            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message, stack = ex.StackTrace });
        }
    }
}
