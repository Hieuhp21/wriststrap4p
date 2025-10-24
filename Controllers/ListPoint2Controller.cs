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
    public class ListPoint2Controller : Controller
    {
        private readonly DataPointContext _context;

        public ListPoint2Controller(DataPointContext context)
        {
            _context = context;
        }

        // GET: ListPoint2
        public async Task<IActionResult> Index()
        {
              return _context.ListPoints2 != null ? 
                          View(await _context.ListPoints2.ToListAsync()) :
                          Problem("Entity set 'DataPointContext.ListPoints2'  is null.");
        }

        // GET: ListPoint2/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ListPoints2 == null)
            {
                return NotFound();
            }

            var listPoint2 = await _context.ListPoints2
                .FirstOrDefaultAsync(m => m.IdPoint == id);
            if (listPoint2 == null)
            {
                return NotFound();
            }

            return View(listPoint2);
        }

        // GET: ListPoint2/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListPoint2/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPoint,IdLine,NamePoint,MinSpect,MaxSpect,Addsread,Addswrite,Plc,OffsetValue,DeltaValue,Timeoff,Enstatus,UserChange,TimeChange,Change,Note,Csstop,Cssleft,Type")] ListPoint2 listPoint2)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool exists = await _context.ListPoints2
                                 .AnyAsync(lp => lp.IdPoint == listPoint2.IdPoint && lp.IdLine == listPoint2.IdLine);

                    if (exists)
                    {
                       
                        ModelState.AddModelError(string.Empty, "Combination of IdPoint and IdLine already exists.");
                        return View(listPoint2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");

                }
                _context.Add(listPoint2);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listPoint2);
        }

        // GET: ListPoints/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.ListPoints2 == null || id == "")
            {
                return NotFound();
            }

            string[] Arrid = id.Split('|');
            int id_point, id_line;

            if (Arrid.Length == 2 && int.TryParse(Arrid[0], out id_point) && int.TryParse(Arrid[1], out id_line))
            {
                var listPoint = await _context.ListPoints2.FirstOrDefaultAsync(x => (x.IdPoint == id_point && x.IdLine == id_line));
                if (listPoint == null)
                {
                    return NotFound();
                }
                return View(listPoint);
            }
            else
            {
                return NotFound();
            }

        }

        // POST: ListPoints/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int idpoint, int idline, [Bind("IdPoint,IdLine,NamePoint,MinSpect,MaxSpect,Addsread,Addswrite,Plc,OffsetValue,DeltaValue,Timeoff,Enstatus,UserChange,TimeChange,Change,Note,Csstop,Cssleft,Type")] ListPoint2 listPoint)
        {
            if (idpoint != listPoint.IdPoint || idline != listPoint.IdLine)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listPoint);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListPointExists(listPoint.IdPoint))
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
            return View(listPoint);
        }

        // GET: ListPoints/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.ListPoints2 == null || id == "")
            {
                return NotFound();
            }

            string[] Arrid = id.Split('|');
            int id_point, id_line;

            if (Arrid.Length == 2 && int.TryParse(Arrid[0], out id_point) && int.TryParse(Arrid[1], out id_line))
            {
                var listPoint = await _context.ListPoints2.FirstOrDefaultAsync(x => (x.IdPoint == id_point && x.IdLine == id_line));
                if (listPoint == null)
                {
                    return NotFound();
                }
                return View(listPoint);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: ListPoints/Delete/5
        [HttpPost, ActionName("DeletePoint")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int idpoint, int idline)
        {
            if (_context.ListPoints == null)
            {
                return Problem("Entity set 'DataPointContext.ListPoints'  is null.");
            }
            var listPoint = await _context.ListPoints2.FirstAsync(x => (x.IdPoint == idpoint && x.IdLine == idline));
            if (listPoint != null)
            {
                _context.ListPoints2.Remove(listPoint);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListPointExists(int id)
        {
            return (_context.ListPoints?.Any(e => e.IdPoint == id)).GetValueOrDefault();
        }

        [Authorize]
        [HttpPost]
        public async Task UpdateXYPoint(int idpoint, int idline, string top, string left)
        {
            if (idpoint >= 0 && idline >= 0)
            {
                var result = await _context.ListPoints2.FirstAsync(x => (x.IdPoint == idpoint && x.IdLine == idline));
                if (result != null)
                {
                    result.Csstop = double.Parse(top.Substring(0, top.Length - 2));
                    result.Cssleft = double.Parse(left.Substring(0, left.Length - 2));
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
            }
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetAllpoint()
        {
            List<ListPoint2> LsPointall = new List<ListPoint2>();
            try
            {
                var result = await _context.ListPoints2.ToListAsync();
                //var lsled = from led in result
                //            orderby led.IdLed descending
                //             select led;
                LsPointall = result;
            }
            catch { }
            return LsPointall.ToJson();
        }
        [HttpGet]
        public async Task<string> GetHubs()
        {
            List<Hub> hubs = new List<Hub>();
            try
            {
                var result = await _context.Hubs.ToListAsync();
                hubs = result;
            }
            catch { }
            return hubs.ToJson();
        }
        [Authorize()]
        [HttpPost]
        public async Task<string> GetAllpointNow()
        {
            List<DataNow2> LsPointNow = new List<DataNow2>();
            try
            {
                var result = await _context.DataNows2.ToListAsync();
                //var lsled = from led in result
                //            orderby led.IdLed descending
                //             select led;
                LsPointNow = result;
            }
            catch { }
            return LsPointNow.ToJson();
        }

    }
}
