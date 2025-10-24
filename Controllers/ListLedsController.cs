using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class ListLedsController : Controller
    {
        private readonly DataledContext _context;

        public ListLedsController(DataledContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListLeds
        public async Task<IActionResult> Index()
        {
              return _context.ListLeds != null ? 
                          View(await _context.ListLeds.ToListAsync()) :
                          Problem("Entity set 'TESTDTContext.ListLeds'  is null.");
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListLeds/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null || _context.ListLeds == null)
            {
                return NotFound();
            }

            var listLed = await _context.ListLeds
                .FirstOrDefaultAsync(m => m.IdLed == id);
            if (listLed == null)
            {
                return NotFound();
            }

            return View(listLed);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListLeds/Create
        public async Task<IActionResult> Create()
        {
            var listidRoom = (from room in (await _context.ListRooms.ToListAsync()) select room.IdRoom).Distinct().ToList();

            ViewBag.ListRoom = listidRoom;

            return View();
        }

        // POST: ListLeds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLed,IdRoom,NameLed,MinTemp,MaxTemp,MinHumi,MaxHumi,Note,Adds,Addsread,Plc,OffsetTemp,OffsetHumi,DeltaT,DeltaH,Timeoff,Enstatus,Csstop,Cssleft")] ListLed listLed)
        {
            if (ModelState.IsValid)
            {
                listLed.Change = true;
                _context.Add(listLed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listLed);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListLeds/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null || _context.ListLeds == null)
            {
                return NotFound();
            }

            var listLed = await _context.ListLeds.FindAsync(id);
            var listidRoom = (from room in (await _context.ListRooms.ToListAsync()) select room.IdRoom).Distinct().ToList();

            ViewBag.ListRoom = listidRoom;

            if (listLed == null)
            {
                return NotFound();
            }
            return View(listLed);
        }

        // POST: ListLeds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLed,IdRoom,NameLed,MinTemp,MaxTemp,MinHumi,MaxHumi,Note,Adds,Addsread,Plc,OffsetTemp,OffsetHumi,DeltaT,DeltaH,Timeoff,Enstatus,Csstop,Cssleft")] ListLed listLed)
        {
            if (id != listLed.IdLed)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    listLed.Change = true;
                    listLed.User_change = User.Identity?.Name;
                    listLed.Time_change = DateTime.Now;
                    _context.Update(listLed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListLedExists(listLed.IdLed))
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
            return View(listLed);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListLeds/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || _context.ListLeds == null)
            {
                return NotFound();
            }

            var listLed = await _context.ListLeds
                .FirstOrDefaultAsync(m => m.IdLed == id);
            if (listLed == null)
            {
                return NotFound();
            }

            return View(listLed);
        }

        // POST: ListLeds/Delete/5
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ListLeds == null)
            {
                return Problem("Entity set 'TESTDTContext.ListLeds'  is null.");
            }
            var listLed = await _context.ListLeds.FindAsync(id);
            if (listLed != null)
            {
                _context.ListLeds.Remove(listLed);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListLedExists(int id)
        {
          return (_context.ListLeds?.Any(e => e.IdLed == id)).GetValueOrDefault();
        }

        [Authorize]
        [HttpPost]
        public async Task UpdateXYBox(int id, string top, string left)
        {
            if (id >= 0)
            {
                var result = await _context.ListLeds.FirstOrDefaultAsync(e => e.IdLed == id);
                if (result != null)
                {
                    result.Csstop = double.Parse(top.Substring(0, top.Length-2));
                    result.Cssleft = double.Parse(left.Substring(0, left.Length - 2));
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}
