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
    public class ListFloorsController : Controller
    {
        private readonly DataPointContext _context;

        public ListFloorsController(DataPointContext context)
        {
            _context = context;
        }

        // GET: ListFloors
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
              return _context.ListFloors != null ? 
                          View(await _context.ListFloors.ToListAsync()) :
                          Problem("Entity set 'DataPointContext.ListFloors'  is null.");
        }

        // GET: ListFloors/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ListFloors == null)
            {
                return NotFound();
            }

            var listFloor = await _context.ListFloors
                .FirstOrDefaultAsync(m => m.IdFloor == id);
            if (listFloor == null)
            {
                return NotFound();
            }

            return View(listFloor);
        }

        // GET: ListFloors/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListFloors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFloor,NameFloor,Note")] ListFloor listFloor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listFloor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listFloor);
        }

        // GET: ListFloors/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ListFloors == null)
            {
                return NotFound();
            }

            var listFloor = await _context.ListFloors.FindAsync(id);
            if (listFloor == null)
            {
                return NotFound();
            }
            return View(listFloor);
        }

        // POST: ListFloors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("IdFloor,NameFloor,Note")] ListFloor listFloor)
        {
            if (id != listFloor.IdFloor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listFloor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListFloorExists(listFloor.IdFloor))
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
            return View(listFloor);
        }

        // GET: ListFloors/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ListFloors == null)
            {
                return NotFound();
            }

            var listFloor = await _context.ListFloors
                .FirstOrDefaultAsync(m => m.IdFloor == id);
            if (listFloor == null)
            {
                return NotFound();
            }

            return View(listFloor);
        }

        // POST: ListFloors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ListFloors == null)
            {
                return Problem("Entity set 'DataPointContext.ListFloors'  is null.");
            }
            var listFloor = await _context.ListFloors.FindAsync(id);
            if (listFloor != null)
            {
                _context.ListFloors.Remove(listFloor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListFloorExists(int id)
        {
          return (_context.ListFloors?.Any(e => e.IdFloor == id)).GetValueOrDefault();
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetAllFloor()
        {
            List<ListFloor> LsFloorall = new List<ListFloor>();
            try
            {
                var result = await _context.ListFloors.ToListAsync();
                //var lsled = from led in result
                //            orderby led.IdLed descending
                //             select led;
                LsFloorall = result;
            }
            catch { }
            return LsFloorall.ToJson();
        }
        [HttpPost]
        public JsonResult SetDelayTime(string delayTime)
        {
            // Chạy câu lệnh SQL trực tiếp để cập nhật hoặc thêm mới
            int affectedRows = _context.Database.ExecuteSqlRaw(
                @"IF EXISTS (SELECT 1 FROM options)
            UPDATE options SET delay_time = {0}
          ELSE
            INSERT INTO options (delay_time) VALUES ({0})", delayTime);

            return Json(new { message = "Delay time saved successfully!" });
        }


    }
}
