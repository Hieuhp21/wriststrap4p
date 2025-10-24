using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_SHOW_WRIST_STRAP.Models.Entities;

namespace WEB_SHOW_WRIST_STRAP.Controllers
{
    public class ListPlcsController : Controller
    {
        private readonly DataPointContext _context;

        public ListPlcsController(DataPointContext context)
        {
            _context = context;
        }

        // GET: ListPlcs
      
public async Task<IActionResult> Index()
{
    if (_context.ListPlcs == null || _context.ActUnitTypes == null || _context.ActProtocolTypes == null)
    {
        return Problem("Entity sets are null.");
    }

    var listPlcs = await _context.ListPlcs.ToListAsync();
            var actUnitTypes = await _context.ActUnitTypes
                                              .Select(a => new { a.ModuleType, a.ConnectionSystem })
                                              .ToListAsync();

            var actProtocolTypes = await _context.ActProtocolTypes
                                                  .Select(a => new { a.ProtocolType, a.ConnectionSystem })
                                                  .ToListAsync();

            ViewBag.ActUnitTypes = new SelectList(actUnitTypes, "ModuleType", "ConnectionSystem");
            ViewBag.ActProtocolTypes = new SelectList(actProtocolTypes, "ProtocolType", "ConnectionSystem");


            return View(listPlcs);
}


        // GET: ListPlcs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.ListPlcs == null)
            {
                return NotFound();
            }

            var listPlc = await _context.ListPlcs
                .FirstOrDefaultAsync(m => m.IpPlc == id);
            if (listPlc == null)
            {
                return NotFound();
            }

            return View(listPlc);
        }

        // GET: ListPlcs/Create
        public IActionResult Create()
        {
            var actUnitTypes = _context.ActUnitTypes.ToList();
            var actProtocolTypes = _context.ActProtocolTypes.ToList();

            ViewBag.ActUnitTypes = actUnitTypes.Select(u => new SelectListItem
            {
                Value = u.ModuleType,
                Text = u.ConnectionSystem
            }).ToList();

            ViewBag.ActProtocolTypes = actProtocolTypes.Select(p => new SelectListItem
            {
                Value = p.ProtocolType,
                Text = p.ConnectionSystem
            }).ToList();

            return View();
        }

        // POST: ListPlcs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPlc,IpPlc,ActUnitType,ActProtocolType,ActTimeOut,ActPassword,AddRead,NumberReads,AddReadV,NumberReadsV,IdLine,StatusUse")] ListPlc listPlc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listPlc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listPlc);
        }

        // GET: ListPlcs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.ListPlcs == null)
            {
                return NotFound();
            }

            var listPlc = await _context.ListPlcs.FindAsync(id);
            if (listPlc == null)
            {
                return NotFound();
            }

            var actUnitTypes = _context.ActUnitTypes.ToList();
            var actProtocolTypes = _context.ActProtocolTypes.ToList();

            ViewBag.ActUnitTypes = actUnitTypes.Select(u => new SelectListItem
            {
                Value = u.ModuleType,
                Text = u.ConnectionSystem
            }).ToList();

            ViewBag.ActProtocolTypes = actProtocolTypes.Select(p => new SelectListItem
            {
                Value = p.ProtocolType,
                Text = p.ConnectionSystem
            }).ToList();

            return View(listPlc);
        }

        // POST: ListPlcs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdPlc,IpPlc,ActUnitType,ActProtocolType,ActTimeOut,ActPassword,AddRead,NumberReads,AddReadV,NumberReadsV,IdLine,StatusUse")] ListPlc listPlc)
        {
            if (id != listPlc.IpPlc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listPlc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListPlcExists(listPlc.IpPlc))
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
            return View(listPlc);
        }

        // GET: ListPlcs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.ListPlcs == null)
            {
                return NotFound();
            }

            var listPlc = await _context.ListPlcs
                .FirstOrDefaultAsync(m => m.IpPlc == id);
            if (listPlc == null)
            {
                return NotFound();
            }

            return View(listPlc);
        }

        // POST: ListPlcs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.ListPlcs == null)
            {
                return Problem("Entity set 'DataPointContext.ListPlcs'  is null.");
            }
            var listPlc = await _context.ListPlcs.FindAsync(id);
            if (listPlc != null)
            {
                _context.ListPlcs.Remove(listPlc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListPlcExists(string id)
        {
          return (_context.ListPlcs?.Any(e => e.IpPlc == id)).GetValueOrDefault();
        }
    }
}
