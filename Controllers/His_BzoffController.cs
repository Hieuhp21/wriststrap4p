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
    public class His_BzoffController : Controller
    {
        private readonly DataPointContext _context;

        public His_BzoffController(DataPointContext context)
        {
            _context = context;
        }

        // GET: His_Bzoff
        public async Task<IActionResult> Index()
        {
              return _context.HisBuzzeroffs != null ? 
                          View(await _context.HisBuzzeroffs.ToListAsync()) :
                          Problem("Entity set 'DataledContext.His_Bzoffs'  is null.");
        }

        // GET: His_Bzoff/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.HisBuzzeroffs == null)
            {
                return NotFound();
            }

            var his_Bzoff = await _context.HisBuzzeroffs
                .FirstOrDefaultAsync(m => m.Username == id);
            if (his_Bzoff == null)
            {
                return NotFound();
            }

            return View(his_Bzoff);
        }

        // GET: His_Bzoff/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: His_Bzoff/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Dateclick")] HisBuzzeroff his_Bzoff)
        {
            if (ModelState.IsValid)
            {
                _context.Add(his_Bzoff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(his_Bzoff);
        }

        // GET: His_Bzoff/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.HisBuzzeroffs == null)
            {
                return NotFound();
            }

            var his_Bzoff = await _context.HisBuzzeroffs.FindAsync(id);
            if (his_Bzoff == null)
            {
                return NotFound();
            }
            return View(his_Bzoff);
        }

        // POST: His_Bzoff/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Username,Dateclick")] HisBuzzeroff his_Bzoff)
        {
            if (id != his_Bzoff.Username)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(his_Bzoff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!His_BzoffExists(his_Bzoff.Username))
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
            return View(his_Bzoff);
        }

        // GET: His_Bzoff/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.HisBuzzeroffs == null)
            {
                return NotFound();
            }

            var his_Bzoff = await _context.HisBuzzeroffs
                .FirstOrDefaultAsync(m => m.Username == id);
            if (his_Bzoff == null)
            {
                return NotFound();
            }

            return View(his_Bzoff);
        }

        // POST: His_Bzoff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.HisBuzzeroffs == null)
            {
                return Problem("Entity set 'DataledContext.HisBuzzeroffs'  is null.");
            }
            var his_Bzoff = await _context.HisBuzzeroffs.FindAsync(id);
            if (his_Bzoff != null)
            {
                _context.HisBuzzeroffs.Remove(his_Bzoff);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool His_BzoffExists(string id)
        {
          return (_context.HisBuzzeroffs?.Any(e => e.Username == id)).GetValueOrDefault();
        }

        [Authorize()]
        [HttpPost]
        public async Task<bool> GetHis_Buzzoff()
        {
            bool buzzeron = true;
            HisBuzzeroff his_Bzoff;
            try
            {
                if (User.Identity != null)
                {
                    var result = await _context.HisBuzzeroffs.FirstOrDefaultAsync(e => e.Username == User.Identity.Name);
                    if (result != null)
                    {
                        his_Bzoff = result;
                        if(his_Bzoff.Dateclick != null)
                        if (DateTime.Now.Subtract((DateTime)his_Bzoff.Dateclick).TotalMinutes < 5)
                        {
                            buzzeron = false;
                        }
                    }
                }
            }
            catch { }
            return buzzeron;
        }

        [Authorize()]
        [HttpPost]
        public async Task AddHis_Buzzeroff()
        {
            HisBuzzeroff newhis = new HisBuzzeroff();
            newhis.Username = User.Identity.Name;
            if (newhis.Username != null)
            {
                var result = await _context.HisBuzzeroffs.FirstOrDefaultAsync(e => e.Username == User.Identity.Name);
                if (result != null)
                {
                    result.Dateclick = DateTime.Now;
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    newhis.Dateclick = DateTime.Now;
                    _context.Add(newhis);
                    await _context.SaveChangesAsync();
                }
            }
        }

        [Authorize()]
        [HttpPost]
        public async Task<string> GetTime_Buzzoff()
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            HisBuzzeroff his_Bzoff;
            string TimeSpanStr = "";
            try
            {
                if (User.Identity != null)
                {
                    var result = await _context.HisBuzzeroffs.FirstOrDefaultAsync(e => e.Username == User.Identity.Name);
                    if (result != null)
                    {
                        his_Bzoff = result;
                        if (his_Bzoff.Dateclick != null)
                        {
                            timeSpan = DateTime.Now.Subtract((DateTime)his_Bzoff.Dateclick);
                            if (timeSpan.TotalMinutes < 5)
                            {
                                TimeSpanStr = (new TimeSpan(0,5,0) - timeSpan).ToString("mm\\:ss");
                            }
                        }
                            
                    }
                }
            }
            catch { }
            return TimeSpanStr;
        }
    }
}
