using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class ListLinesController : Controller
    {
        private readonly DataPointContext _context;
        private static List<DataNow> PreviousData = new List<DataNow>();
        public ListLinesController(DataPointContext context)
        {
            _context = context;
        }

        // GET: ListLines
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            return _context.ListLines != null ?
                View(await _context.ListLines.OrderBy(x => x.STT).ToListAsync()) :
                Problem("Entity set 'DataPointContext.ListLines' is null.");
        }

        // GET: ListLines/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ListLines == null)
            {
                return NotFound();
            }

            var listLine = await _context.ListLines
                .FirstOrDefaultAsync(m => m.IdLine == id);
            if (listLine == null)
            {
                return NotFound();
            }

            return View(listLine);
        }

        // GET: ListLines/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListLines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("IdLine,IdIviLine,NameLine,ListUser,Floor,Note,Csstop,Cssleft,Csswidth,Cssheight,TotalPointAlarm")] ListLine listLine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listLine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listLine);
        }

        // GET: ListLines/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ListLines == null)
            {
                return NotFound();
            }

            var listLine = await _context.ListLines.FindAsync(id);
            if (listLine == null)
            {
                return NotFound();
            }
            return View(listLine);
        }

        // POST: ListLines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("IdLine,IdIviLine,NameLine,ListUser,Floor,Note,Csstop,Cssleft,Csswidth,Cssheight,TotalPointAlarm")] ListLine listLine)
        {
            if (id != listLine.IdLine)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listLine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListLineExists(listLine.IdLine))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(listLine);
        }

        // GET: ListLines/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ListLines == null)
            {
                return NotFound();
            }

            var listLine = await _context.ListLines
                .FirstOrDefaultAsync(m => m.IdLine == id);
            if (listLine == null)
            {
                return NotFound();
            }

            return View(listLine);
        }

        // POST: ListLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ListLines == null)
            {
                return Problem("Entity set 'DataPointContext.ListLines'  is null.");
            }
            var listLine = await _context.ListLines.FindAsync(id);
            if (listLine != null)
            {
                _context.ListLines.Remove(listLine);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListLineExists(int id)
        {
          return (_context.ListLines?.Any(e => e.IdLine == id)).GetValueOrDefault();
        }

        [Authorize()]
        [HttpPost]
     
        public async Task<string> GetAllLine()
        {
            List<ListLine> LsLineall = new List<ListLine>();
            try
            {
                LsLineall = await _context.ListLines
                    .OrderBy(x => x.STT) // Sắp xếp theo cột stt
                    .ToListAsync();
            }
            catch
            {
                // Có thể thêm log lỗi ở đây để debug
            }
            return LsLineall.ToJson();
        }

        [Authorize()]
        [HttpGet]
        public async Task<IActionResult> GetLineName(int idLine)
        {
            try
            {
                var line = await _context.ListLines
                    .Where(l => l.IdLine == idLine)
                    .Select(l => new { NameLine = l.NameLine, STT = l.STT })
                    .FirstOrDefaultAsync();

                if (line == null)
                {
                    return NotFound(new { NameLine = $"Line {idLine}", STT = 0 });
                }

                return Json(line);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { NameLine = $"Line {idLine}", STT = 0, Error = ex.Message });
            }
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task UpdateXYLine(int idline, string top, string left, string width, string height)
        {
            if (idline >= 0)
            {
                var result = await _context.ListLines.FirstOrDefaultAsync(e => e.IdLine == idline);
                if (result != null)
                {
                    result.Csstop = double.Parse(top.Substring(0, top.Length - 2));
                    result.Cssleft = double.Parse(left.Substring(0, left.Length - 2));
                    result.Csswidth = double.Parse(width.Substring(0, width.Length - 2));
                    result.Cssheight = double.Parse(height.Substring(0, height.Length - 2));
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
            }
        }
        [HttpPost]
        public IActionResult UpdateStatus(int id, int status)
        {
            // Tìm dòng Line cần cập nhật
            var line = _context.ListLines.Find(id);
            if (line == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái
            line.Status = status;
            _context.SaveChanges();

            // Điều hướng lại trang Index
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CheckDataNow()
        {
            // Lần gọi đầu tiên đến database
            var previousData = GetFilteredData();

            // Chờ 60 giây
            await Task.Delay(1000);

            // Lần gọi thứ hai đến database
            var currentData = GetFilteredData();

            // So sánh dữ liệu giữa lần gọi trước và lần gọi hiện tại
            var result = currentData.Select(cd => new
            {
                IdPoint = cd.IdPoint,
                IdLine = cd.IdLine,
                CurrentTotalTime = cd.TotalTime,
                PreviousTotalTime = previousData
                                        .FirstOrDefault(pd => pd.IdPoint == cd.IdPoint && pd.IdLine == cd.IdLine)?.TotalTime,
                Status = previousData
                             .Any(pd => pd.IdPoint == cd.IdPoint && pd.IdLine == cd.IdLine && pd.TotalTime != cd.TotalTime)
                             ? "on"
                             : "off"
            })
            //.Where(r => r.Status == "off") // Chỉ lấy các kết quả có trạng thái là "off"
            .ToList();

            // Trả về dữ liệu dưới dạng JSON
            return Json(result);
        }

        private List<DataNow> GetFilteredData()
        {
            // Sử dụng context hiện có (_context) để lấy dữ liệu
            return _context.DataNows
                .Where(d => d.IdPoint > 0 && d.Alarm != 5)
                .AsNoTracking()
                .GroupBy(d => d.IdLine)
                .Select(g => g.FirstOrDefault()) // Lấy một bản ghi đại diện cho mỗi id_line
                .ToList();
        }


    }
}
