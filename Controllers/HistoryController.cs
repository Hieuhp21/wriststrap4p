using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Linq;
using WEB_SHOW_WRIST_STRAP.Core.ViewModels;
using WEB_SHOW_WRIST_STRAP.Models;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class HistoryController : Controller
    {

        private readonly DataPointContext _context;

        public HistoryController(DataPointContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public IActionResult Index()
        {
            return View();
        }

        string Alarmcode = "2|6";
        [HttpGet]
        public async Task<IActionResult> GetHistory(DateTime? timeStart, DateTime? timeEnd, int floorSelect, int lineSelect, int? idPoint, int? idLine, int? totalTime, DateTime? beginTime, int page = 1, int pageSize = 100)
        {
            // Kiểm tra thời gian truyền vào tối đa 6 tháng
            if (timeStart == null || timeEnd == null || (timeEnd - timeStart).Value.TotalDays > 180)
            {
                return BadRequest("The maximum time range is 6 months.");
            }

            // Lấy danh sách IdLine từ ListLines dựa trên điều kiện floorSelect và lineSelect
            var lineFloorData = await _context.ListLines
                .Where(l => (floorSelect == 0 || l.Floor == floorSelect)
                            && (lineSelect == 0 || l.IdLine == lineSelect))
                .Select(l => l.IdLine)
                .ToListAsync();

            // Lấy dữ liệu lịch sử từ cơ sở dữ liệu dựa trên các tiêu chí tìm kiếm
            var historyData = _context.TotalData
                .Where(h => !(new[] { "6" }).Contains(h.Alarm) && h.TimeCheck >= timeStart && h.TimeCheck <= timeEnd
             && lineFloorData.Contains(h.IdLine));

            // Áp dụng các filter khác
            if (idPoint.HasValue)
            {
                historyData = historyData.Where(h => h.IdPoint == idPoint.Value);
            }

            if (idLine.HasValue)
            {
                historyData = historyData.Where(h => h.IdLine == idLine.Value);
            }

            // Sắp xếp theo thời gian giảm dần
            historyData = historyData.OrderByDescending(h => h.TimeCheck);

            // Phân trang
            var pagedData = await historyData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await historyData.CountAsync();

            var result = new PagedResult<TotalDatum>
            {
                Items = pagedData,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetHistory2(DateTime? timeStart, DateTime? timeEnd, int floorSelect, int lineSelect, int? idPoint, int? idLine, int? totalTime, DateTime? beginTime, int page = 1, int pageSize = 100)
        {
            // Kiểm tra thời gian truyền vào tối đa 6 tháng
            if (timeStart == null || timeEnd == null || (timeEnd - timeStart).Value.TotalDays > 180)
            {
                return BadRequest("The maximum time range is 6 months.");
            }

            // Lấy danh sách IdLine từ ListLines dựa trên điều kiện floorSelect và lineSelect
            var lineFloorData = await _context.ListLines
                .Where(l => (floorSelect == 0 || l.Floor == floorSelect)
                            && (lineSelect == 0 || l.IdLine == lineSelect))
                .Select(l => l.IdLine)
                .ToListAsync();

            // Lấy dữ liệu lịch sử từ cơ sở dữ liệu dựa trên các tiêu chí tìm kiếm
            var historyData = _context.TotalData2
                .Where(h => h.TimeCheck >= timeStart && h.TimeCheck <= timeEnd
                            && lineFloorData.Contains(h.IdLine));

            // Áp dụng các filter khác
            if (idPoint.HasValue)
            {
                historyData = historyData.Where(h => h.IdPoint == idPoint.Value);
            }

            if (idLine.HasValue)
            {
                historyData = historyData.Where(h => h.IdLine == idLine.Value);
            }

            // Sắp xếp theo thời gian giảm dần
            historyData = historyData.OrderByDescending(h => h.TimeCheck);

            // Phân trang
            var pagedData = await historyData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await historyData.CountAsync();

            var result = new PagedResult<TotalDatum2>
            {
                Items = pagedData,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetHistoryChartData(DateTime? timeStart, DateTime? timeEnd, int floorSelect, int lineSelect)
        {
            // Kiểm tra thời gian truyền vào tối đa 6 tháng
            if (timeStart == null || timeEnd == null || (timeEnd - timeStart).Value.TotalDays > 180)
            {
                return BadRequest("The maximum time range is 6 months.");
            }

            string Alarmcode = "2|4|5|6";
            var alarmCodes = Alarmcode.Split('|').Select(code => int.Parse(code)).ToHashSet();

            try
            {
                // Lấy danh sách IdLine từ ListLines dựa trên điều kiện floorSelect và lineSelect
                var lineFloorData = await _context.ListLines
                    .Where(l => (floorSelect == 0 || l.Floor == floorSelect)
                                && (lineSelect == 0 || l.IdLine == lineSelect))
                    .Select(l => new { l.IdLine, l.Floor })
                    .ToListAsync();

                var lineIds = lineFloorData.Select(l => l.IdLine).ToList();

                // Lọc TotalData dựa trên IdLine và các điều kiện khác
                var historyData = _context.TotalData
                    .Where(h => h.TimeCheck >= timeStart && h.TimeCheck <= timeEnd
                                 && (Alarmcode.Contains(h.Alarm))
                                && lineIds.Contains(h.IdLine));

                // Nếu người dùng chọn floorSelect, lọc danh sách theo tầng
                if (floorSelect != 0)
                {
                    historyData = historyData
                        .Where(h => lineFloorData.Any(l => l.IdLine == h.IdLine && l.Floor == floorSelect));
                }

                // Nếu người dùng chọn lineSelect, lọc danh sách theo line
                if (lineSelect != 0)
                {
                    historyData = historyData.Where(h => h.IdLine == lineSelect);
                }

                // Sắp xếp theo thời gian giảm dần
                historyData = historyData.OrderByDescending(h => h.TimeCheck);

                // Lấy tất cả dữ liệu không phân trang
                var data = await historyData.ToListAsync();

                return Json(data);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về lỗi 500
                
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetHistoryChartData2(DateTime? timeStart, DateTime? timeEnd, int floorSelect, int lineSelect)
        {
            // Kiểm tra thời gian truyền vào tối đa 6 tháng
            if (timeStart == null || timeEnd == null || (timeEnd - timeStart).Value.TotalDays > 180)
            {
                return BadRequest("The maximum time range is 6 months.");
            }

            string Alarmcode = "2";
            var alarmCodes = Alarmcode.Split('|').Select(code => int.Parse(code)).ToHashSet();

            try
            {
                // Lấy danh sách IdLine từ ListLines dựa trên điều kiện floorSelect và lineSelect
                var lineFloorData = await _context.ListLines
                    .Where(l => (floorSelect == 0 || l.Floor == floorSelect)
                                && (lineSelect == 0 || l.IdLine == lineSelect))
                    .Select(l => new { l.IdLine, l.Floor })
                    .ToListAsync();

                var lineIds = lineFloorData.Select(l => l.IdLine).ToList();

                // Lọc TotalData dựa trên IdLine và các điều kiện khác
                var historyData = _context.TotalData2
                    .Where(h => h.TimeCheck >= timeStart && h.TimeCheck <= timeEnd
                                 && (h.Alarm=="2")
                                && lineIds.Contains(h.IdLine));

                // Nếu người dùng chọn floorSelect, lọc danh sách theo tầng
                if (floorSelect != 0)
                {
                    historyData = historyData
                        .Where(h => lineFloorData.Any(l => l.IdLine == h.IdLine && l.Floor == floorSelect));
                }

                // Nếu người dùng chọn lineSelect, lọc danh sách theo line
                if (lineSelect != 0)
                {
                    historyData = historyData.Where(h => h.IdLine == lineSelect);
                }

                // Sắp xếp theo thời gian giảm dần
                historyData = historyData.OrderByDescending(h => h.TimeCheck);

                // Lấy tất cả dữ liệu không phân trang
                var data = await historyData.ToListAsync();

                return Json(data);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về lỗi 500
                //_logger.LogError(ex, "Error in GetHistoryChartData");
                return StatusCode(500, "Internal server error");
            }
        }



        /// <summary>
        /// Phiên bản cũ Chưa gộp trạng thái OK
        /// </summary>
        /// <param name="timeStart"></param>
        /// <param name="timeEnd"></param>
        /// <param name="floorSelect"></param>
        /// <param name="lineSelect"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExportToExcel_old(DateTime timeStart, DateTime timeEnd, int floorSelect, int lineSelect)
        {
            var filteredData = from datum in _context.TotalData
                               join line in _context.ListLines on datum.IdLine equals line.IdLine
                               join point in _context.ListPoints on new { datum.IdPoint, line.IdLine } equals new { point.IdPoint, point.IdLine }
                               join floor in _context.ListFloors on line.Floor equals floor.IdFloor
                               where datum.TimeCheck >= timeStart && datum.TimeCheck <= timeEnd
                               orderby datum.IdLog descending
                               select new
                               {
                                   datum.IdLog,
                                   datum.IdPoint,
                                   datum.TimeCheck,
                                   datum.TimeStop,
                                   datum.Note,
                                   datum.TotalTime,
                                   datum.Alarm,
                                   point.NamePoint,
                                   line.IdLine,
                                   line.NameLine,
                                   line.Floor,
                                   floor.NameFloor,
                                   datum.Value
                               };

            // Apply filtering if provided
            if (floorSelect != 0)
            {
                filteredData = filteredData.Where(d => d.Floor == floorSelect);
            }

            if (lineSelect != 0)
            {
                filteredData = filteredData.Where(d => d.IdLine == lineSelect);
            }

            // Sort data according to the required order
            filteredData = filteredData.OrderByDescending(d => d.IdLog)
                                       .ThenBy(d => d.NameFloor)
                                       .ThenBy(d => d.NameLine)
                                       .ThenBy(d => d.IdPoint)
                                       .ThenBy(d => d.NamePoint)
                                       .ThenBy(d => d.TimeCheck)
                                       .ThenBy(d => d.TimeStop);

            var errorSummaryByPoint = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => new { d.IdPoint, d.NamePoint, d.NameLine, d.NameFloor, d.IdLine })
                .Select(g => new
                {
                    g.Key.IdLine,
                    g.Key.NameFloor,
                    g.Key.NameLine,
                    g.Key.IdPoint,
                    g.Key.NamePoint,
                    TotalAlarmTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .OrderBy(g => g.IdLine)
                .ThenBy(g => g.IdPoint)
                .ToList();

            var errorSummaryByType = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => d.Alarm)
                .Select(g => new
                {
                    Alarm = g.Key,
                    TotalTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("History Data");

                // Add header for the main table
                //worksheet.Cells[1, 1].Value = "ID Log";
                //worksheet.Cells[1, 2].Value = "Begin Time";
                //worksheet.Cells[1, 3].Value = "End Time";
                //worksheet.Cells[1, 4].Value = "Name Floor";
                //worksheet.Cells[1, 5].Value = "Total Time";
                //worksheet.Cells[1, 6].Value = "Name Line";
                //worksheet.Cells[1, 7].Value = "Id Point";
                //worksheet.Cells[1, 8].Value = "Name Point";
                //worksheet.Cells[1, 9].Value = "Status";
                //worksheet.Cells[1, 10].Value = "Note";

                worksheet.Cells[1, 1].Value = "Begin Time";
                worksheet.Cells[1, 2].Value = "Name Line";
                worksheet.Cells[1, 3].Value = "Name Point";
                worksheet.Cells[1, 4].Value = "Status";
                worksheet.Cells[1, 5].Value = "Value";
                worksheet.Cells[1, 6].Value = "Total Time";
                worksheet.Cells[1, 7].Value = "End Time";
                worksheet.Cells[1, 8].Value = "Note";

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                var dataList = filteredData.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    //worksheet.Cells[i + 2, 1].Value = data.IdLog;
                    //worksheet.Cells[i + 2, 2].Value = data.TimeCheck.ToString("g");
                    //worksheet.Cells[i + 2, 3].Value = data.TimeStop.HasValue ? data.TimeStop.Value.ToString("g") : string.Empty;
                    //worksheet.Cells[i + 2, 4].Value = data.NameFloor;
                    //worksheet.Cells[i + 2, 5].Value = data.TotalTime.HasValue ? TimeSpan.FromMinutes(data.TotalTime.Value).ToString(@"hh\:mm\:ss") : string.Empty;
                    //worksheet.Cells[i + 2, 6].Value = data.NameLine;
                    //worksheet.Cells[i + 2, 7].Value = data.IdPoint;
                    //worksheet.Cells[i + 2, 8].Value = data.NamePoint;
                    //worksheet.Cells[i + 2, 9].Value = GetStatus(data.Alarm);
                    //worksheet.Cells[i + 2, 10].Value = data.Note;

                    worksheet.Cells[i + 2, 1].Value = data.TimeCheck.ToString("g");
                    worksheet.Cells[i + 2, 2].Value = data.NameLine;
                    worksheet.Cells[i + 2, 3].Value = data.NamePoint;
                    worksheet.Cells[i + 2, 4].Value = GetStatus(data.Alarm);
                    worksheet.Cells[i + 2, 5].Value = data.Value; // value
                    worksheet.Cells[i + 2, 6].Value = data.TotalTime.HasValue ? TimeSpan.FromMinutes(data.TotalTime.Value).ToString(@"hh\:mm\:ss") : string.Empty;
                    worksheet.Cells[i + 2, 7].Value = data.TimeStop.HasValue ? data.TimeStop.Value.ToString("g") : string.Empty;
                    worksheet.Cells[i + 2, 8].Value = data.Note;
                    using (var range = worksheet.Cells[i + 2, 1, i + 2, 8])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by point (horizontal)
                int summaryStartColumn = 12; // Start from column 12
                                             //worksheet.Cells[1, summaryStartColumn].Value = "Name Floor";
                worksheet.Cells[1, summaryStartColumn].Value = "Name Line";
                worksheet.Cells[1, summaryStartColumn + 1].Value = "Id Point";
                worksheet.Cells[1, summaryStartColumn + 2].Value = "Name Point";
                worksheet.Cells[1, summaryStartColumn + 3].Value = "Total Alarm Time(Min)";
                worksheet.Cells[1, summaryStartColumn + 4].Value = "Error Count";
                using (var range = worksheet.Cells[1, summaryStartColumn, 1, summaryStartColumn + 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByPoint.Count(); i++)
                {
                    var data = errorSummaryByPoint[i];
                    //worksheet.Cells[i + 2, summaryStartColumn].Value = data.NameFloor;
                    worksheet.Cells[i + 2, summaryStartColumn].Value = data.NameLine;
                    worksheet.Cells[i + 2, summaryStartColumn + 1].Value = data.IdPoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 2].Value = data.NamePoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 3].Value = data.TotalAlarmTime / 60;
                    worksheet.Cells[i + 2, summaryStartColumn + 4].Value = data.ErrorCount;
                    using (var range = worksheet.Cells[i + 2, summaryStartColumn, i + 2, summaryStartColumn + 4])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by type (below the summary by point)
                int errorSummaryStartRow = errorSummaryByPoint.Count + 4;
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn].Value = "Alarm";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 1].Value = "Total Time(Min)";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 2].Value = "Error Count";
                using (var range = worksheet.Cells[errorSummaryStartRow, summaryStartColumn, errorSummaryStartRow, summaryStartColumn + 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByType.Count(); i++)
                {
                    var data = errorSummaryByType[i];
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn].Value = GetStatus(data.Alarm);
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 1].Value = data.TotalTime / 60;
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 2].Value = data.ErrorCount;
                    using (var range = worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn, errorSummaryStartRow + i + 1, summaryStartColumn + 2])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }


                // Add first chart for Total Alarm Time by Point

                var chart1 = worksheet.Drawings.AddChart("TotalAlarmTimeChart", eChartType.ColumnClustered);
                chart1.Title.Text = "Total Alarm Time by Point";
                chart1.SetPosition(0, 0, summaryStartColumn + 7, 0);
                chart1.SetSize(800, 400);

                // Series for Total Alarm Time
                var series1 = chart1.Series.Add(worksheet.Cells[2, summaryStartColumn + 3, errorSummaryByPoint.Count + 1, summaryStartColumn + 3], worksheet.Cells[2, summaryStartColumn + 2, errorSummaryByPoint.Count + 1, summaryStartColumn + 2]);
                series1.Header = "Total Alarm Time";

                // Add second chart for Error Count by Point

                var chart2 = worksheet.Drawings.AddChart("ErrorCountChart", eChartType.ColumnClustered);
                chart2.Title.Text = "Error Count by Point";
                chart2.SetPosition(22, 0, summaryStartColumn + 7, 0);
                chart2.SetSize(800, 400);

                // Series for Error Count
                var series2 = chart2.Series.Add(worksheet.Cells[2, summaryStartColumn + 4, errorSummaryByPoint.Count + 1, summaryStartColumn + 4], worksheet.Cells[2, summaryStartColumn + 2, errorSummaryByPoint.Count + 1, summaryStartColumn + 2]);
                series2.Header = "Error Count";

                var pieChart = worksheet.Drawings.AddChart("SummaryByTypeChart", eChartType.Pie3D);
                pieChart.Title.Text = "Error Count by Type";
                pieChart.SetPosition(errorSummaryStartRow + errorSummaryByType.Count + 17, 0, summaryStartColumn + 7, 0);
                pieChart.SetSize(800, 400);

                var pieSeries = pieChart.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 2, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 2], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries.Header = "Error Count";

                // Pie chart for Total Alarm Time by Type
                var pieChart2 = worksheet.Drawings.AddChart("TotalTimeByTypeChart", eChartType.Pie3D);
                pieChart2.Title.Text = "Total Alarm Time by Type";
                pieChart2.SetPosition(errorSummaryStartRow + errorSummaryByType.Count - 5, 0, summaryStartColumn + 7, 0);
                pieChart2.SetSize(800, 400);

                var pieSeries2 = pieChart2.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 1, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 1], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries2.Header = "Total Alarm Time";

                var stream = new MemoryStream(package.GetAsByteArray());
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "HistoryData.xlsx";

                return File(stream, contentType, fileName);
            }
        }
        public IActionResult ExportToExcel(DateTime timeStart, DateTime timeEnd, int floorSelect, int lineSelect)
        {
            var filteredData = from datum in _context.TotalData
                               join line in _context.ListLines on datum.IdLine equals line.IdLine
                               join point in _context.ListPoints on new { datum.IdPoint, line.IdLine } equals new { point.IdPoint, point.IdLine }
                               join floor in _context.ListFloors on line.Floor equals floor.IdFloor
                               where !(new[] { "6" }).Contains(datum.Alarm) && datum.TimeCheck >= timeStart && datum.TimeCheck <= timeEnd
                               orderby datum.IdLog descending
                               select new
                               {
                                   datum.IdLog,
                                   datum.IdPoint,
                                   datum.TimeCheck,
                                   datum.TimeStop,
                                   datum.Note,
                                   datum.TotalTime,
                                   datum.Alarm,
                                   point.NamePoint,
                                   line.IdLine,
                                   line.NameLine,
                                   line.Floor,
                                   floor.NameFloor,
                                   datum.Value
                               };

            // Apply filtering if provided
            if (floorSelect != 0)
            {
                filteredData = filteredData.Where(d => d.Floor == floorSelect);
            }
            if (lineSelect != 0)
            {
                filteredData = filteredData.Where(d => d.IdLine == lineSelect);
            }

            // Sort data according to the required order
            filteredData = filteredData.OrderByDescending(d => d.IdLog)
                                      .ThenBy(d => d.NameFloor)
                                      .ThenBy(d => d.NameLine)
                                      .ThenBy(d => d.IdPoint)
                                      .ThenBy(d => d.NamePoint)
                                      .ThenBy(d => d.TimeCheck)
                                      .ThenBy(d => d.TimeStop);

            var errorSummaryByPoint = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => new { d.IdPoint, d.NamePoint, d.NameLine, d.NameFloor, d.IdLine })
                .Select(g => new
                {
                    g.Key.IdLine,
                    g.Key.NameFloor,
                    g.Key.NameLine,
                    g.Key.IdPoint,
                    g.Key.NamePoint,
                    TotalAlarmTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .OrderBy(g => g.IdLine)
                .ThenBy(g => g.IdPoint)
                .ToList();

            var errorSummaryByType = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => d.Alarm)
                .Select(g => new
                {
                    Alarm = g.Key,
                    TotalTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .ToList();

            // Danh sách kết quả sau khi gộp
            var mergedData = new List<ExportLogDto>();
            // Chuyển filteredData về dạng List để dễ truy cập theo chỉ số
            var rawData = filteredData
                .OrderBy(x => x.NameFloor)
                .ThenBy(x => x.NameLine)
                .ThenBy(x => x.IdPoint)
                .ThenBy(x => x.NamePoint)
                .ThenBy(x => x.TimeCheck)  // Sắp theo thời gian tăng dần để gộp liên mạch theo thời gian
                .ToList();

            int e = 0;
            // Lặp qua từng dòng trong danh sách
            while (e < rawData.Count)
            {
                var current = rawData[e];
                // Nếu là dòng có trạng thái OK (Alarm = "0") thì xét gộp
                if (current.Alarm == "0")
                {
                    // Lưu lại thông tin bắt đầu của chuỗi OK cần gộp
                    var start = current.TimeCheck;
                    var end = current.TimeStop;
                    var point = current.NamePoint;
                    var idPoint = current.IdPoint;
                    var line = current.NameLine;
                    var value = current.Value;
                    var note = current.Note ?? "";
                    int? accumTotalTime = current.TotalTime; // Bắt đầu tích lũy TotalTime thực tế

                    int j = e + 1;
                    // Lặp tiếp các dòng tiếp theo để kiểm tra có thể gộp không
                    while (j < rawData.Count &&
                           rawData[j].Alarm == "0" && // Điều kiện 1: Alarm là OK
                           rawData[j].NamePoint == point && // Điều kiện 2: cùng NamePoint
                           rawData[j].NameLine == line && // Điều kiện 3: cùng NameLine
                           rawData[j].TimeStop.HasValue && // Điều kiện 4: TimeStop không null
                           rawData[j].TimeCheck == end.Value && // Điều kiện mới: liền nhau về thời gian (không có gap)
                           (accumTotalTime + rawData[j].TotalTime) <= 3600) // Điều kiện 6: tổng thời gian tích lũy không vượt quá 1 giờ
                    {
                        // Cập nhật thời gian kết thúc và cộng dồn giá trị
                        end = rawData[j].TimeStop;
                        value = Math.Max(value, rawData[j].Value);
                        note = string.IsNullOrEmpty(rawData[j].Note) ? note : $"{note}; {rawData[j].Note}";
                        accumTotalTime += rawData[j].TotalTime; // Cộng dồn TotalTime thực tế
                        j++;
                    }

                    // Tính TotalTime (sử dụng giá trị tích lũy)
                    int? totalTime = accumTotalTime;

                    // Nếu TimeStop < TimeCheck hoặc tổng âm, gán TotalTime = 0 (dù khó xảy ra vì đã cộng dồn)
                    if (totalTime < 0)
                    {
                        totalTime = 0;
                    }

                    // Thêm bản ghi gộp vào mergedData
                    mergedData.Add(new ExportLogDto
                    {
                        Alarm = "0",
                        TimeCheck = start,
                        TimeStop = end,
                        NamePoint = point,
                        NameLine = line,
                        Note = note,
                        Value = value,
                        TotalTime = totalTime
                    });

                    // Bỏ qua các dòng đã được gộp
                    e = j;
                }
                else
                {
                    // Nếu không phải Alarm = "0", giữ nguyên dữ liệu gốc
                    mergedData.Add(new ExportLogDto
                    {
                        Alarm = current.Alarm,
                        TimeCheck = current.TimeCheck,
                        TimeStop = current.TimeStop,
                        NamePoint = current.NamePoint,
                        NameLine = current.NameLine,
                        Note = current.Note,
                        Value = current.Value,
                        TotalTime = current.TotalTime
                    });
                    e++;
                }
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("History Data");
                // Add header for the main table
                worksheet.Cells[1, 1].Value = "Begin Time";
                worksheet.Cells[1, 2].Value = "Name Line";
                worksheet.Cells[1, 3].Value = "Name Point";
                worksheet.Cells[1, 4].Value = "Status";
                worksheet.Cells[1, 5].Value = "Value";
                worksheet.Cells[1, 6].Value = "Total Time";
                worksheet.Cells[1, 7].Value = "End Time";
                worksheet.Cells[1, 8].Value = "Note";

                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                var dataList = mergedData;
                for (int i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    worksheet.Cells[i + 2, 1].Value = data.TimeCheck.ToString("g");
                    worksheet.Cells[i + 2, 2].Value = data.NameLine;
                    worksheet.Cells[i + 2, 3].Value = data.NamePoint;
                    worksheet.Cells[i + 2, 4].Value = GetStatus(data.Alarm.ToString());
                    worksheet.Cells[i + 2, 5].Value = data.Value;
                    worksheet.Cells[i + 2, 6].Style.Numberformat.Format = @"hh\:mm\:ss";
                    worksheet.Cells[i + 2, 6].Value = data.TotalTime.HasValue ? TimeSpan.FromSeconds(data.TotalTime.Value).ToString(@"hh\:mm\:ss") : string.Empty;
                    worksheet.Cells[i + 2, 7].Value = data.TimeStop.HasValue ? data.TimeStop.Value.ToString("g") : string.Empty;
                    worksheet.Cells[i + 2, 8].Value = data.Note;

                    using (var range = worksheet.Cells[i + 2, 1, i + 2, 8])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by point (horizontal)
                int summaryStartColumn = 12;
                worksheet.Cells[1, summaryStartColumn].Value = "Name Line";
                worksheet.Cells[1, summaryStartColumn + 1].Value = "Id Point";
                worksheet.Cells[1, summaryStartColumn + 2].Value = "Name Point";
                worksheet.Cells[1, summaryStartColumn + 3].Value = "Total Alarm Time(Min)";
                worksheet.Cells[1, summaryStartColumn + 4].Value = "Error Count";

                using (var range = worksheet.Cells[1, summaryStartColumn, 1, summaryStartColumn + 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByPoint.Count(); i++)
                {
                    var data = errorSummaryByPoint[i];
                    worksheet.Cells[i + 2, summaryStartColumn].Value = data.NameLine;
                    worksheet.Cells[i + 2, summaryStartColumn + 1].Value = data.IdPoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 2].Value = data.NamePoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 3].Value = data.TotalAlarmTime / 60;
                    worksheet.Cells[i + 2, summaryStartColumn + 4].Value = data.ErrorCount;

                    using (var range = worksheet.Cells[i + 2, summaryStartColumn, i + 2, summaryStartColumn + 4])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by type (below the summary by point)
                int errorSummaryStartRow = errorSummaryByPoint.Count + 4;
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn].Value = "Alarm";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 1].Value = "Total Time(Min)";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 2].Value = "Error Count";

                using (var range = worksheet.Cells[errorSummaryStartRow, summaryStartColumn, errorSummaryStartRow, summaryStartColumn + 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByType.Count(); i++)
                {
                    var data = errorSummaryByType[i];
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn].Value = GetStatus(data.Alarm);
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 1].Value = data.TotalTime / 60;
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 2].Value = data.ErrorCount;

                    using (var range = worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn, errorSummaryStartRow + i + 1, summaryStartColumn + 2])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add first chart for Total Alarm Time by Point
                var chart1 = worksheet.Drawings.AddChart("TotalAlarmTimeChart", eChartType.ColumnClustered);
                chart1.Title.Text = "Total Alarm Time by Point";
                chart1.SetPosition(0, 0, summaryStartColumn + 7, 0);
                chart1.SetSize(800, 400);
                var series1 = chart1.Series.Add(worksheet.Cells[2, summaryStartColumn + 3, errorSummaryByPoint.Count + 1, summaryStartColumn + 3], worksheet.Cells[2, summaryStartColumn + 2, errorSummaryByPoint.Count + 1, summaryStartColumn + 2]);
                series1.Header = "Total Alarm Time";

                // Add second chart for Error Count by Point
                var chart2 = worksheet.Drawings.AddChart("ErrorCountChart", eChartType.ColumnClustered);
                chart2.Title.Text = "Error Count by Point";
                chart2.SetPosition(22, 0, summaryStartColumn + 7, 0);
                chart2.SetSize(800, 400);
                var series2 = chart2.Series.Add(worksheet.Cells[2, summaryStartColumn + 4, errorSummaryByPoint.Count + 1, summaryStartColumn + 4], worksheet.Cells[2, summaryStartColumn + 2, errorSummaryByPoint.Count + 1, summaryStartColumn + 2]);
                series2.Header = "Error Count";

                var pieChart = worksheet.Drawings.AddChart("SummaryByTypeChart", eChartType.Pie3D);
                pieChart.Title.Text = "Error Count by Type";
                pieChart.SetPosition(errorSummaryStartRow + errorSummaryByType.Count + 17, 0, summaryStartColumn + 7, 0);
                pieChart.SetSize(800, 400);
                var pieSeries = pieChart.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 2, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 2], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries.Header = "Error Count";

                // Pie chart for Total Alarm Time by Type
                var pieChart2 = worksheet.Drawings.AddChart("TotalTimeByTypeChart", eChartType.Pie3D);
                pieChart2.Title.Text = "Total Alarm Time by Type";
                pieChart2.SetPosition(errorSummaryStartRow + errorSummaryByType.Count - 5, 0, summaryStartColumn + 7, 0);
                pieChart2.SetSize(800, 400);
                var pieSeries2 = pieChart2.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 1, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 1], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries2.Header = "Total Alarm Time";

                var stream = new MemoryStream(package.GetAsByteArray());
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "HistoryData.xlsx";
                return File(stream, contentType, fileName);
            }
        }
        [HttpGet]
        public IActionResult ExportToExcel2(DateTime timeStart, DateTime timeEnd, int floorSelect, int lineSelect)
        {
            var filteredData = from datum in _context.TotalData2
                               join line in _context.ListLines on datum.IdLine equals line.IdLine
                               join point in _context.ListPoints on new { datum.IdPoint, line.IdLine } equals new { point.IdPoint, point.IdLine }
                            //   join floor in _context.ListFloors on line.Floor equals floor.IdFloor
                               where !(new[] { "6" }).Contains(datum.Alarm) && datum.TimeCheck >= timeStart && datum.TimeCheck <= timeEnd
                               orderby datum.IdLog descending
                               select new
                               {
                                   datum.IdLog,
                                   datum.IdPoint,
                                   datum.TimeCheck,
                                   datum.TimeStop,
                                   datum.Note,
                                   datum.TotalTime,
                                   datum.Alarm,
                                   point.NamePoint,
                                   line.IdLine,
                                   line.NameLine,
                                   line.Floor,
                                  // floor.NameFloor,
                               };

            // Apply filtering if provided
            if (floorSelect != 0)
            {
                filteredData = filteredData.Where(d => d.Floor == floorSelect);
            }

            if (lineSelect != 0)
            {
                filteredData = filteredData.Where(d => d.IdLine == lineSelect);
            }

            // Sort data according to the required order
            filteredData = filteredData.OrderByDescending(d => d.IdLog)
                                     //  .ThenBy(d => d.NameFloor)
                                       .ThenBy(d => d.NameLine)
                                       .ThenBy(d => d.IdPoint)
                                       .ThenBy(d => d.NamePoint)
                                       .ThenBy(d => d.TimeCheck)
                                       .ThenBy(d => d.TimeStop);

            var errorSummaryByPoint = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => new { d.IdPoint, d.NamePoint, d.NameLine, d.IdLine })
                .Select(g => new
                {
                    g.Key.IdLine,
                   // g.Key.NameFloor,
                    g.Key.NameLine,
                    g.Key.IdPoint,
                    g.Key.NamePoint,
                    TotalAlarmTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .OrderBy(g => g.IdLine)
                .ThenBy(g => g.IdPoint)
                .ToList();

            var errorSummaryByType = filteredData
                .Where(d => Alarmcode.Contains(d.Alarm))
                .GroupBy(d => d.Alarm)
                .Select(g => new
                {
                    Alarm = g.Key,
                    TotalTime = g.Sum(d => d.TotalTime),
                    ErrorCount = g.Count()
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("History Data");

                // Add header for the main table
                worksheet.Cells[1, 1].Value = "ID Log";
                worksheet.Cells[1, 2].Value = "Begin Time";
                worksheet.Cells[1, 3].Value = "End Time";
               
                worksheet.Cells[1, 4].Value = "Total Time";
                worksheet.Cells[1, 5].Value = "Name Line";
                worksheet.Cells[1, 6].Value = "Id Point";
                worksheet.Cells[1, 7].Value = "Name Point";
                worksheet.Cells[1, 8].Value = "Status";
                worksheet.Cells[1, 9].Value = "Note";
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                var dataList = filteredData.ToList();
                for (int i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    worksheet.Cells[i + 2, 1].Value = data.IdLog;
                    worksheet.Cells[i + 2, 2].Value = data.TimeCheck.ToString("g");
                    worksheet.Cells[i + 2, 3].Value = data.TimeStop.HasValue ? data.TimeStop.Value.ToString("g") : string.Empty;
   
                    worksheet.Cells[i + 2, 4].Value = data.TotalTime.HasValue ? TimeSpan.FromSeconds(data.TotalTime.Value).ToString(@"hh\:mm\:ss") : string.Empty;
                    worksheet.Cells[i + 2, 5].Value = data.NameLine;
                    worksheet.Cells[i + 2, 6].Value = data.IdPoint;
                    worksheet.Cells[i + 2, 7].Value = data.NamePoint;
                    worksheet.Cells[i + 2, 8].Value = GetStatus(data.Alarm);
                    worksheet.Cells[i + 2, 9].Value = data.Note;
                    using (var range = worksheet.Cells[i + 2, 1, i + 2, 10])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by point (horizontal)
                int summaryStartColumn = 12; // Start from column 12
               // worksheet.Cells[1, summaryStartColumn].Value = "Name Floor";
                worksheet.Cells[1, summaryStartColumn + 1].Value = "Name Line";
                worksheet.Cells[1, summaryStartColumn + 2].Value = "Id Point";
                worksheet.Cells[1, summaryStartColumn + 3].Value = "Name Point";
                worksheet.Cells[1, summaryStartColumn + 4].Value = "Total Alarm Time(Min)";
                worksheet.Cells[1, summaryStartColumn + 5].Value = "Error Count";
                using (var range = worksheet.Cells[1, summaryStartColumn, 1, summaryStartColumn + 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByPoint.Count(); i++)
                {
                    var data = errorSummaryByPoint[i];
                   // worksheet.Cells[i + 2, summaryStartColumn].Value = data.NameFloor;
                    worksheet.Cells[i + 2, summaryStartColumn + 1].Value = data.NameLine;
                    worksheet.Cells[i + 2, summaryStartColumn + 2].Value = data.IdPoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 3].Value = data.NamePoint;
                    worksheet.Cells[i + 2, summaryStartColumn + 4].Value = data.TotalAlarmTime / 60;
                    worksheet.Cells[i + 2, summaryStartColumn + 5].Value = data.ErrorCount;
                    using (var range = worksheet.Cells[i + 2, summaryStartColumn, i + 2, summaryStartColumn + 5])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                // Add summary by type (below the summary by point)
                int errorSummaryStartRow = errorSummaryByPoint.Count + 4;
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn].Value = "Alarm";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 1].Value = "Total Time(Min)";
                worksheet.Cells[errorSummaryStartRow, summaryStartColumn + 2].Value = "Error Count";
                using (var range = worksheet.Cells[errorSummaryStartRow, summaryStartColumn, errorSummaryStartRow, summaryStartColumn + 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < errorSummaryByType.Count(); i++)
                {
                    var data = errorSummaryByType[i];
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn].Value = GetStatus(data.Alarm);
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 1].Value = data.TotalTime / 60;
                    worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn + 2].Value = data.ErrorCount;
                    using (var range = worksheet.Cells[errorSummaryStartRow + i + 1, summaryStartColumn, errorSummaryStartRow + i + 1, summaryStartColumn + 2])
                    {
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }


                // Add first chart for Total Alarm Time by Point

                var chart1 = worksheet.Drawings.AddChart("TotalAlarmTimeChart", eChartType.ColumnClustered);
                chart1.Title.Text = "Total Alarm Time by Point";
                chart1.SetPosition(0, 0, summaryStartColumn + 7, 0);
                chart1.SetSize(800, 400);

                // Series for Total Alarm Time
                var series1 = chart1.Series.Add(worksheet.Cells[2, summaryStartColumn + 4, errorSummaryByPoint.Count + 1, summaryStartColumn + 4], worksheet.Cells[2, summaryStartColumn + 3, errorSummaryByPoint.Count + 1, summaryStartColumn + 3]);
                series1.Header = "Total Alarm Time";

                // Add second chart for Error Count by Point

                var chart2 = worksheet.Drawings.AddChart("ErrorCountChart", eChartType.ColumnClustered);
                chart2.Title.Text = "Error Count by Point";
                chart2.SetPosition(22, 0, summaryStartColumn + 7, 0);
                chart2.SetSize(800, 400);

                // Series for Error Count
                var series2 = chart2.Series.Add(worksheet.Cells[2, summaryStartColumn + 5, errorSummaryByPoint.Count + 1, summaryStartColumn + 5], worksheet.Cells[2, summaryStartColumn + 3, errorSummaryByPoint.Count + 1, summaryStartColumn + 3]);
                series2.Header = "Error Count";

                var pieChart = worksheet.Drawings.AddChart("SummaryByTypeChart", eChartType.Pie3D);
                pieChart.Title.Text = "Error Count by Type";
                pieChart.SetPosition(errorSummaryStartRow + errorSummaryByType.Count + 17, 0, summaryStartColumn + 7, 0);
                pieChart.SetSize(800, 400);

                var pieSeries = pieChart.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 2, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 2], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries.Header = "Error Count";

                // Pie chart for Total Alarm Time by Type
                var pieChart2 = worksheet.Drawings.AddChart("TotalTimeByTypeChart", eChartType.Pie3D);
                pieChart2.Title.Text = "Total Alarm Time by Type";
                pieChart2.SetPosition(errorSummaryStartRow + errorSummaryByType.Count - 5, 0, summaryStartColumn + 7, 0);
                pieChart2.SetSize(800, 400);

                var pieSeries2 = pieChart2.Series.Add(worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn + 1, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn + 1], worksheet.Cells[errorSummaryStartRow + 1, summaryStartColumn, errorSummaryStartRow + errorSummaryByType.Count, summaryStartColumn]);
                pieSeries2.Header = "Total Alarm Time";

                var stream = new MemoryStream(package.GetAsByteArray());
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "HistoryData.xlsx";

                return File(stream, contentType, fileName);
            }
        }

        private string GetStatus(string alarm)
        {
            if (string.IsNullOrEmpty(alarm))
            {
                return "OFF";
            }

            if (int.TryParse(alarm, out int alarmCode))
            {

                switch (alarmCode)
                {
                    case 0:
                        return "OK";
                    case 1:
                        return "WAIT";
                    case 2:
                        return "NG";
                    case 3:
                        return "OFF";
                    case 4:
                        return "ERR";
                    case 5:
                        return "DIS. CONNECT";
                    case 6:
                        return "DIS. POINT";
                    case 7:
                        return "REPAIRING";
                    default:
                        return "UNKNOWN";
                }
            }
            else
            {
                return "UNKNOWN";
            }
        }
        [Authorize()]
        [HttpPost]
        public async Task<string> GetDataTopAlarmAll(string[] LslineID)
        {
            List<TotalDataViewModel> LsHistopAlarm = new List<TotalDataViewModel>();

            if (LslineID.Length > 0)
            {
                try
                {
                    var currentUser = User.Identity.Name;
                    var now = DateTime.Now;

                    // Lấy dữ liệu từ TotalData
                    var result1 = await _context.TotalData
                        .Where(x => Alarmcode.Contains(x.Alarm) &&
                                    LslineID.Contains(x.IdLine.ToString())
                                    && x.TimeCheck <= DateTime.Now && x.TimeCheck >= DateTime.Now.AddDays(-3))
                        .Select(x => new TotalDataViewModel
                        {
                            IdLog = x.IdLog,
                            Alarm = x.Alarm,
                            IdLine = x.IdLine.ToString(),
                            Note = x.Note,
                            TimeCheck = x.TimeCheck,
                            TotalTime = x.TotalTime,
                            IdPoint = x.IdPoint,
                            SourceTable = "TotalData",
                            RepairStatus = x.Status
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    // Lấy dữ liệu từ TotalData2 (uncomment và sử dụng)
                    var result2 = await _context.TotalData2
                        .Where(x => Alarmcode.Contains(x.Alarm) &&
                                    LslineID.Contains(x.IdLine.ToString())
                                    && x.TimeCheck <= DateTime.Now && x.TimeCheck >= DateTime.Now.AddDays(-3))  // Thêm điều kiện thời gian tương tự TotalData
                        .Select(x => new TotalDataViewModel
                        {
                            IdLog = x.IdLog,
                            Alarm = x.Alarm,
                            IdLine = x.IdLine.ToString(),
                            Note = x.Note,
                            TimeCheck = x.TimeCheck,
                            TotalTime = x.TotalTime,
                            IdPoint = x.IdPoint,
                            SourceTable = "TotalData2",
                            RepairStatus = x.Status
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    // Gộp dữ liệu từ hai bảng
                    var combinedResults = result1.Concat(result2).ToList();

                    // Lọc dữ liệu theo user (bỏ những lỗi user đã cập nhật)
                    var filteredResults = combinedResults.Where(x =>
                    {
                        if (string.IsNullOrEmpty(x.Note))
                        {
                            return true;
                        }

                        var notes = x.Note.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var note in notes)
                        {
                            var parts = note.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 1)
                            {
                                var userPart = parts[1].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                if (userPart.Length > 0 && userPart[0].Trim() == currentUser)
                                {
                                    return false; // User đã cập nhật lỗi này
                                }
                            }
                        }
                        return true;
                    }).ToList();

                    // Lọc trùng lỗi trong vòng 1 phút
                    var filteredTotalData = filteredResults
                 .Where(x => x.SourceTable == "TotalData")
                 .GroupBy(x => new { x.IdPoint, x.IdLine, x.Alarm })
                 .Select(g => g.OrderBy(x => x.TimeCheck).First())
                 .Where(x =>
                 {
                     var OKdata = _context.TotalData
                         .Where(e => e.IdPoint == x.IdPoint && e.IdLine.ToString() == x.IdLine)
                         .OrderByDescending(e => e.TimeCheck)
                         .FirstOrDefault();
                     return OKdata == null || OKdata.Alarm != "0"; // Ẩn nếu bản ghi mới nhất là OK
                 })
                 .ToList();

                    // Không lọc trùng cho TotalData2, chỉ lọc bản ghi OK
                    var filteredTotalData2 = filteredResults
                        .Where(x => x.SourceTable == "TotalData2")
                        .Where(x =>
                        {
                            var OKdata = _context.TotalData2
                                .Where(e => e.IdPoint == x.IdPoint && e.IdLine.ToString() == x.IdLine)
                                .OrderByDescending(e => e.TimeCheck)
                                .FirstOrDefault();
                            return OKdata == null || OKdata.Alarm != "0"; // Ẩn nếu bản ghi mới nhất là OK
                        })
                        .ToList();

                    // Gộp lại dữ liệu đã lọc
                    LsHistopAlarm = filteredTotalData.Concat(filteredTotalData2)
                        .OrderByDescending(x => x.TimeCheck)
                        .ToList();

                    // Lấy bảng ListPoint 
                    // Lấy listID point
                    var listIDpoit = LsHistopAlarm.Select(x => x.IdPoint).ToList();
                    var listIDline = LsHistopAlarm.Select(x => x.IdLine).ToList();
                    var listPoint = _context.ListPoints.Where(e =>
                        listIDpoit.Contains(e.IdPoint) && listIDline.Contains(e.IdLine.ToString())
                    ).ToList();

                    var listJoin = (from alarm in LsHistopAlarm
                                    join point in _context.ListPoints
                                    on new { alarm.IdPoint, alarm.IdLine }
                                    equals new { point.IdPoint, IdLine = point.IdLine.ToString() }
                                    select new TotalDataViewModel
                                    {
                                        IdLog = alarm.IdLog,
                                        Alarm = alarm.Alarm,
                                        IdLine = alarm.IdLine.ToString(),
                                        Note = alarm.Note,
                                        TimeCheck = alarm.TimeCheck,
                                        TotalTime = (int)(DateTime.Now - alarm.TimeCheck).TotalSeconds,
                                        //TotalTime = GetThoiGianThucHien(alarm.IdPoint, int.Parse(alarm.IdLine), alarm.SourceTable),  // Nếu muốn dùng hàm tính thời gian nâng cấp
                                        IdPoint = alarm.IdPoint,
                                        SourceTable = alarm.SourceTable,
                                        RepairStatus = alarm.RepairStatus == 1 ? 1 : int.Parse(point.Type)
                                    }).ToList();
                    LsHistopAlarm = listJoin;
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu cần
                }
            }

            return LsHistopAlarm.ToJson();
        }
        private int GetThoiGianThucHien(int idPoint, int idLine)
        {
            // Lấy ra bản ghi OK mới nhất
            var qr = _context.TotalData.Where(x => x.IdPoint == idPoint && x.IdLine == idLine && x.Alarm == "0").OrderByDescending(x => x.TimeCheck).ToQueryString();
            var OKdata1 = _context.TotalData.Where(x => x.IdPoint == idPoint && x.IdLine == idLine && x.Alarm == "0").OrderByDescending(x => x.TimeCheck).FirstOrDefault();
            //var OKdata2 = _context.TotalData2.Where(x => x.IdPoint == idPoint && x.IdLine == idLine && x.Alarm == "1").OrderByDescending(x => x.TimeCheck).FirstOrDefault();
            if (OKdata1 == null)
            {
                return 0;
            }
            // Lấy ra bản ghi Error đầu tiên tính từ thời gian OK đến hiện tại
            var ErorData = _context.TotalData.Where(x => x.IdPoint == idPoint && x.IdLine == idLine && Alarmcode.Contains(x.Alarm ?? "") && x.TimeCheck > OKdata1.TimeCheck).OrderBy(x => x.TimeCheck).FirstOrDefault();
            DateTime startTime = (ErorData?.TimeCheck) ?? DateTime.Now;
            DateTime endTime = DateTime.Now;

            return (int)(endTime - startTime).TotalSeconds;

        }

        [HttpPost]
        public async Task<IActionResult> UpdateRepairStatus(UpdateRepairedStatusViewModel input)
        {
            using var transaction = _context.Database.BeginTransaction();
            var result = false;
            try
            {
                // Lấy bản ghi cần cập nhật dựa trên sourceTable
                if (input.sourceTable == "TotalData")
                {
                    var totalData = _context.TotalData.FirstOrDefault(x => x.IdLog == input.id_log);
                    if (totalData != null)
                    {
                        var lspoint = _context.ListPoints.FirstOrDefault(x => x.IdPoint == totalData.IdPoint && x.IdLine == totalData.IdLine);
                        if (input.status == 1) // Error -> Repairing
                        {
                            totalData.Status = 0;
                            lspoint.Type = "2";
                            // Thêm bản ghi mới vào TotalData
                            _context.TotalData.Add(new TotalDatum
                            {
                                IdPoint = totalData.IdPoint,
                                IdLine = totalData.IdLine,
                                TimeCheck = DateTime.Now,
                                Value = 0,
                                MinSpect = 0,
                                MaxSpect = 1,
                                Alarm = "7",
                                Note = null,
                                OldValue = null,
                                TotalTime = 0,
                                TimeStop = null,
                                Status = 0
                            });
                        }
                        else if (input.status == 2) // Repairing -> Repaired
                        {
                            totalData.Status = 2;
                            lspoint.Type = "1";
                            // Thêm bản ghi mới vào TotalData
                            _context.TotalData.Add(new TotalDatum
                            {
                                IdPoint = totalData.IdPoint,
                                IdLine = totalData.IdLine,
                                TimeCheck = DateTime.Now,
                                Value = 0,
                                MinSpect = 0,
                                MaxSpect = 1,
                                Alarm = "0",
                                Note = null,
                                OldValue = null,
                                TotalTime = 0,
                                TimeStop = null,
                                Status = 1
                            });
                        }
                        result = true;
                    }
                }
                else if (input.sourceTable == "TotalData2")
                {
                    var totalData2 = _context.TotalData2.FirstOrDefault(x => x.IdLog == input.id_log);
                    if (totalData2 != null)
                    {
                        var lspoint = _context.ListPoints2.FirstOrDefault(x => x.IdPoint == totalData2.IdPoint && x.IdLine == totalData2.IdLine);
                        if (input.status == 1) // Error -> Repairing
                        {
                            totalData2.Status = 0;
                            lspoint.Type = "2";
                            // Thêm bản ghi mới vào TotalData2
                            _context.TotalData2.Add(new TotalDatum2
                            {
                                IdPoint = totalData2.IdPoint,
                                IdLine = totalData2.IdLine,
                                TimeCheck = DateTime.Now,
                                Value = 0,
                                MinSpect = 0,
                                MaxSpect = 1,
                                Alarm = "7",
                                Note = null,
                                OldValue = null,
                                TotalTime = 0,
                                TimeStop = null,
                                Status = 0
                            });
                        }
                        else if (input.status == 2) // Repairing -> Repaired
                        {
                            totalData2.Status = 2;
                            lspoint.Type = "1";
                            // Thêm bản ghi mới vào TotalData2
                            _context.TotalData2.Add(new TotalDatum2
                            {
                                IdPoint = totalData2.IdPoint,
                                IdLine = totalData2.IdLine,
                                TimeCheck = DateTime.Now,
                                Value = 0,
                                MinSpect = 0,
                                MaxSpect = 1,
                                Alarm = "0",
                                Note = null,
                                OldValue = null,
                                TotalTime = 0,
                                TimeStop = null,
                                Status = 1
                            });
                        }
                        result = true;
                    }
                }

                if (result)
                {
                    _context.SaveChanges();
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Xử lý lỗi hoặc log ra
            }

            return Ok(new { success = result });
        }
        [Authorize()]
        [HttpPost]
        public async Task<string> GetAlarmleft(string[] LslineID, string timeckstart)
        {
            List<int> Alarmcodes = new List<int> { 2, 4, 6 };

            if (LslineID == null) return null;
            List<DataNow> LsHistopTop = new List<DataNow>();
            DateTime Dtcheck;
            if (LslineID.Length > 0 && DateTime.TryParse(timeckstart, out Dtcheck))
                try
                {
                    var result = await _context.DataNows.Where(x => (Alarmcodes.Contains(x.Alarm) && LslineID.Contains(x.IdLine.ToString()) && x.TimeCheck >= Dtcheck)).ToListAsync();
                    LsHistopTop = result;
                }
                catch { }
            return LsHistopTop.ToJson();
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
        public async Task<string> GetAlldatapointofline(int idline)
        {
            List<TotalDatum> listhisofline = new List<TotalDatum>();
            try
            {
                var result = await _context.TotalData.Where(x => (x.IdLine == idline && x.TimeCheck > (DateTime.Now.AddDays(-1)))).OrderByDescending(x => x.IdLog).Take(1000).ToListAsync();

                listhisofline = result;
            }
            catch { }
            return listhisofline.ToJson();
        }
        [Authorize()]
        [HttpGet]
        public async Task<IActionResult> GetPaginatedDataTable(int idLine, int page = 1, int pageSize = 100)
        {
            var timeCheckThreshold = DateTime.Now.AddDays(-1);

            // Lấy dữ liệu từ cơ sở dữ liệu dựa trên IdLine và thời gian trong vòng 1 ngày qua
            var historyData = _context.TotalData
                .Where(h => h.IdLine == idLine && h.TimeCheck > timeCheckThreshold)
                .OrderByDescending(h => h.TimeCheck);

            // Phân trang
            var pagedData = await historyData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await historyData.CountAsync();

            var result = new PagedResult<TotalDatum>
            {
                Items = pagedData,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Json(result);
        }
        [Authorize()]
        [HttpGet]
        public async Task<IActionResult> GetPaginatedDataTable2(int idLine, int page = 1, int pageSize = 100)
        {
            var timeCheckThreshold = DateTime.Now.AddDays(-1);

            // Lấy dữ liệu từ cơ sở dữ liệu dựa trên IdLine và thời gian trong vòng 1 ngày qua
            var historyData = _context.TotalData2
                .Where(h => h.IdLine == idLine && h.TimeCheck > timeCheckThreshold)
                .OrderByDescending(h => h.TimeCheck);

            // Phân trang
            var pagedData = await historyData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await historyData.CountAsync();

            var result = new PagedResult<TotalDatum2>
            {
                Items = pagedData,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetChartDataNew(int idPoint, int idLine, string mode = "day")
        {
            try
            {
                DateTime timeCheckThreshold;
                TimeSpan aggregationInterval;

                // Xác định khoảng thời gian và khoảng gộp dựa trên mode
                switch (mode.ToLower())
                {
                    case "week":
                        timeCheckThreshold = DateTime.Now.AddDays(-7);
                        aggregationInterval = TimeSpan.FromMinutes(30); // Gộp mỗi 30 phút
                        break;
                    case "month":
                        timeCheckThreshold = DateTime.Now.AddDays(-30);
                        aggregationInterval = TimeSpan.FromHours(4); // Gộp mỗi 4 giờ
                        break;
                    case "day":
                    default:
                        timeCheckThreshold = DateTime.Now.AddDays(-1);
                        aggregationInterval = TimeSpan.FromMinutes(1); // Không gộp (1 phút)
                        break;
                }

                // Lấy dữ liệu từ database
                var rawData = await _context.TotalData
                    .Where(d => d.IdPoint == idPoint && d.IdLine == idLine && d.TimeCheck > timeCheckThreshold)
                    .OrderBy(d => d.TimeCheck)
                    .Select(d => new
                    {
                        u = d.Value,
                        timestamp = d.TimeCheck,
                        mini = d.MinSpect,
                        maxi = d.MaxSpect
                    })
                    .ToListAsync();

                // Gộp dữ liệu nếu mode là week hoặc month
                var aggregatedData = new List<object>();
                if (mode.ToLower() != "day")
                {
                    var currentWindowStart = timeCheckThreshold;
                    var currentWindowEnd = currentWindowStart + aggregationInterval;
                    var windowData = new List<dynamic>();

                    foreach (var item in rawData)
                    {
                        if (item.timestamp <= currentWindowEnd)
                        {
                            windowData.Add(item);
                        }
                        else
                        {
                            // Xử lý cửa sổ hiện tại
                            aggregatedData.AddRange(AggregateWindow(windowData, currentWindowStart, mode));
                            windowData.Clear();
                            windowData.Add(item);
                            currentWindowStart = currentWindowEnd;
                            currentWindowEnd += aggregationInterval;
                        }
                    }

                    // Xử lý cửa sổ cuối cùng
                    if (windowData.Any())
                    {
                        aggregatedData.AddRange(AggregateWindow(windowData, currentWindowStart, mode));
                    }
                }
                else
                {
                    aggregatedData = rawData.Cast<object>().ToList();
                }

                return Ok(aggregatedData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Hàm gộp dữ liệu trong một cửa sổ thời gian
        private List<object> AggregateWindow(List<dynamic> windowData, DateTime windowStart, string mode)
        {
            var result = new List<object>();
            if (!windowData.Any()) return result;

            // Nếu tất cả giá trị đều bằng 0 hoặc không đổi, chỉ lấy đầu và cuối
            var values = windowData.Select(d => d.u).Distinct().ToList();
            if (values.Count == 1)
            {
                // Chỉ lấy bản ghi đầu và cuối
                if (windowData.Count >= 1)
                {
                    result.Add(new
                    {
                        u = windowData.First().u,
                        timestamp = windowData.First().timestamp,
                        mini = windowData.First().mini,
                        maxi = windowData.First().maxi
                    });
                }
                if (windowData.Count > 1)
                {
                    result.Add(new
                    {
                        u = windowData.Last().u,
                        timestamp = windowData.Last().timestamp,
                        mini = windowData.Last().mini,
                        maxi = windowData.Last().maxi
                    });
                }
            }
            else
            {
                // Lấy giá trị max trong mỗi cửa sổ nếu có thay đổi
                var maxValue = windowData.Max(d => d.u);
                var maxItem = windowData.OrderByDescending(d => d.u).First();
                result.Add(new
                {
                    u = maxValue,
                    timestamp = maxItem.timestamp,
                    mini = maxItem.mini,
                    maxi = maxItem.maxi
                });

                // Nếu giá trị trở về 0, thêm điểm cuối của cửa sổ
                if (windowData.Last().u == 0)
                {
                    result.Add(new
                    {
                        u = windowData.Last().u,
                        timestamp = windowData.Last().timestamp,
                        mini = windowData.Last().mini,
                        maxi = windowData.Last().maxi
                    });
                }
            }

            return result;
        }
        [Authorize()]
        [HttpGet]
        public async Task<IActionResult> GetDataForChart(int idLine)
        {
            var timeCheckThreshold = DateTime.Now.AddDays(-1);
            var historyData = await _context.TotalData
              .Where(h => h.IdLine == idLine && h.TimeCheck > timeCheckThreshold && (Alarmcode.Contains(h.Alarm)))
              .OrderByDescending(h => h.TimeCheck)
              .ToListAsync();

            return Json(historyData);
        }
        [Authorize()]
        [HttpGet]
        public async Task<IActionResult> GetDataForChart2(int idLine)
        {
            var timeCheckThreshold = DateTime.Now.AddDays(-1);
            var historyData = await _context.TotalData2
              .Where(h => h.IdLine == idLine && h.TimeCheck > timeCheckThreshold && (Alarmcode.Contains(h.Alarm)))
              .OrderByDescending(h => h.TimeCheck)
              .ToListAsync();

            return Json(historyData);
        }

        [Authorize()]
        [HttpPost]
        public IActionResult UpdateNote(int id, string note)
        {
            // Tìm đối tượng bằng id
            var item = _context.TotalData.FirstOrDefault(i => i.IdLog == id);
            if (item == null)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng hiện tại
            var currentUser = User.Identity.Name;

            // Tạo ghi chú mới với thời gian và người cập nhật
            var updatedNote = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} , {currentUser}, {note}";

            // Cộng thêm ghi chú mới vào ghi chú cũ
            item.Note = item.Note != null ? item.Note + "\n" + updatedNote : updatedNote;

            // Lưu thay đổi
            _context.SaveChanges();

            // Trả về kết quả thành công
            return Json(new { success = true });
        }
        [Authorize()]
        [HttpPost]
        public IActionResult UpdateNote2(int id, string note)
        {
            // Tìm đối tượng bằng id
            var item = _context.TotalData2.FirstOrDefault(i => i.IdLog == id);
            if (item == null)
            {
                return NotFound();
            }

            // Lấy thông tin người dùng hiện tại
            var currentUser = User.Identity.Name;

            // Tạo ghi chú mới với thời gian và người cập nhật
            var updatedNote = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} , {currentUser}, {note}";

            // Cộng thêm ghi chú mới vào ghi chú cũ
            item.Note = item.Note != null ? item.Note + "\n" + updatedNote : updatedNote;

            // Lưu thay đổi
            _context.SaveChanges();

            // Trả về kết quả thành công
            return Json(new { success = true });
        }


        [Authorize()]
        public IActionResult LogExport()
        {
            return View();
        }
        [HttpPost]
        [Authorize()]
        public string Getfullfilebyuser(string listline)
        {
            string Json = PathtoJson.GetJstlscsv(@"D:\Export\", new string[] { "xls" }, 5, listline);
            return Json;
        }
        public async Task<IActionResult> GetHistoryChartDataV2(DateTime? timeStart, DateTime? timeEnd, int floorSelect, int lineSelect)
        {
            // Kiểm tra thời gian truyền vào tối đa 6 tháng
            if (timeStart == null || timeEnd == null || (timeEnd - timeStart).Value.TotalDays > 180)
            {
                return BadRequest("The maximum time range is 6 months.");
            }

            string Alarmcode = "2|4|5|6";
            var alarmCodes = Alarmcode.Split('|').Select(code => int.Parse(code)).ToHashSet();

            try
            {
                var qr = _context.TotalNg.Where(d => d.TimeCheck >= timeStart && d.TimeCheck <= timeEnd)
                    .Where(d => d.IdLine == lineSelect)
                    .OrderByDescending(d => d.TimeCheck).ToQueryString();
                var data = await _context.TotalNg.Where(d => d.TimeCheck >= timeStart && d.TimeCheck <= timeEnd)
                    .Where(d => (lineSelect == 0 || d.IdLine == lineSelect))
                    .OrderByDescending(d => d.TimeCheck)
                    .Select(x => new
                    {
                        x.IdLog,
                        x.IdPoint,
                        x.IdLine,
                        x.TimeCheck,
                        x.Value,
                        x.MinSpect,
                        x.MaxSpect,
                        x.Alarm,
                        x.TimeStop,
                        totalTime = (x.TimeStop.HasValue ? (int)(x.TimeStop.Value - x.TimeCheck).TotalSeconds : 0)
                    })
                    .ToListAsync();
                return Json(data);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về lỗi 500
                //_logger.LogError(ex, "Error in GetHistoryChartData");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public IActionResult UpdateHistory(int idLog, string status, double value, string note)
        {
            var item = _context.TotalData.FirstOrDefault(i => i.IdLog == idLog);
            if (item == null)
            {
                return NotFound();
            }

            Dictionary<string, int> statusToAlarm = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
          { "OK", 0 },
        { "WAIT", 1 },
        { "NG", 2 },
        { "OFF", 3 },
        { "DIS. POINT", 6 },
        { "Fixing",7}
    };

            if (statusToAlarm.TryGetValue(status, out int alarmCode))
            {
                item.Alarm = alarmCode.ToString();
            }

            item.Value = value;

            // Lấy note hiện tại
            string existingNote = item.Note ?? "";
            var currentUser = User.Identity.Name;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Tách reason và solution từ note mới
            string newReason = "";
            string newSolution = "";
            if (!string.IsNullOrEmpty(note))
            {
                var reasonMatch = System.Text.RegularExpressions.Regex.Match(note, @"Reason:\s*(.+?)(?:,\s*Solution:|$)");
                var solutionMatch = System.Text.RegularExpressions.Regex.Match(note, @"Solution:\s*(.+)$");
                newReason = reasonMatch.Success ? reasonMatch.Groups[1].Value.Trim() : "";
                newSolution = solutionMatch.Success ? solutionMatch.Groups[1].Value.Trim() : "";
            }

            string updatedNote;
            if (string.IsNullOrEmpty(existingNote))
            {
                // Nếu chưa có note, thêm mới
                updatedNote = $"{timestamp} , {currentUser}, Reason: {newReason}, Solution: {newSolution}";
            }
            else
            {
                // Nếu có note, cập nhật Reason và Solution trong dòng cuối cùng
                var noteLines = existingNote.Split('\n').ToList();
                var lastLine = noteLines.LastOrDefault() ?? "";

                // Tách các phần của lastLine: timestamp , user, Reason: ..., Solution: ...
                var lastLineParts = lastLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (lastLineParts.Length >= 3)
                {
                    // Giữ timestamp và user, cập nhật Reason và Solution
                    string prefix = $"{lastLineParts[0].Trim()} , {lastLineParts[1].Trim()}";
                    string newLastLine = $"{prefix}, Reason: {newReason}, Solution: {newSolution}";

                    // Thay thế dòng cuối
                    noteLines[noteLines.Count - 1] = newLastLine;
                    updatedNote = string.Join("\n", noteLines);
                }
                else
                {
                    // Nếu lastLine không đúng định dạng, thêm dòng mới
                    string newNoteLine = $"{timestamp} , {currentUser}, Reason: {newReason}, Solution: {newSolution}";
                    updatedNote = existingNote + "\n" + newNoteLine;
                }
            }

            item.Note = updatedNote;

            _context.SaveChanges();

            return Json(new { success = true, updatedNote = updatedNote });
        }

        [HttpPost]
        public IActionResult UpdateHistory2(int idLog, string status, double value, string note)
        {
            var item = _context.TotalData2.FirstOrDefault(i => i.IdLog == idLog);
            if (item == null)
            {
                return NotFound();
            }

            Dictionary<string, int> statusToAlarm = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "OK", 0 },
        { "WAIT", 1 },
        { "NG", 2 },
        { "OFF", 3 },
        { "DISCONNECT", 6 }
    };

            if (statusToAlarm.TryGetValue(status, out int alarmCode))
            {
                item.Alarm = alarmCode.ToString();
            }

            item.Value = value;

            // Lấy note hiện tại
            string existingNote = item.Note ?? "";
            var currentUser = User.Identity.Name;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Tách reason và solution từ note mới
            string newReason = "";
            string newSolution = "";
            if (!string.IsNullOrEmpty(note))
            {
                var reasonMatch = System.Text.RegularExpressions.Regex.Match(note, @"Reason:\s*(.+?)(?:,\s*Solution:|$)");
                var solutionMatch = System.Text.RegularExpressions.Regex.Match(note, @"Solution:\s*(.+)$");
                newReason = reasonMatch.Success ? reasonMatch.Groups[1].Value.Trim() : "";
                newSolution = solutionMatch.Success ? solutionMatch.Groups[1].Value.Trim() : "";
            }

            string updatedNote;
            if (string.IsNullOrEmpty(existingNote))
            {
                // Nếu chưa có note, thêm mới
                updatedNote = $"{timestamp} , {currentUser}, Reason: {newReason}, Solution: {newSolution}";
            }
            else
            {
                // Nếu có note, cập nhật Reason và Solution trong dòng cuối cùng
                var noteLines = existingNote.Split('\n').ToList();
                var lastLine = noteLines.LastOrDefault() ?? "";

                // Tách các phần của lastLine: timestamp , user, Reason: ..., Solution: ...
                var lastLineParts = lastLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (lastLineParts.Length >= 3)
                {
                    // Giữ timestamp và user, cập nhật Reason và Solution
                    string prefix = $"{lastLineParts[0].Trim()} , {lastLineParts[1].Trim()}";
                    string newLastLine = $"{prefix}, Reason: {newReason}, Solution: {newSolution}";

                    // Thay thế dòng cuối
                    noteLines[noteLines.Count - 1] = newLastLine;
                    updatedNote = string.Join("\n", noteLines);
                }
                else
                {
                    // Nếu lastLine không đúng định dạng, thêm dòng mới
                    string newNoteLine = $"{timestamp} , {currentUser}, Reason: {newReason}, Solution: {newSolution}";
                    updatedNote = existingNote + "\n" + newNoteLine;
                }
            }

            item.Note = updatedNote;

            _context.SaveChanges();

            return Json(new { success = true, updatedNote = updatedNote });
        }
    }
}
