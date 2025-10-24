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
    public class ListRoomsController : Controller
    {
        private readonly DataledContext _context;

        public ListRoomsController(DataledContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListRooms
        public async Task<IActionResult> Index()
        {
              return _context.ListRooms != null ? 
                          View(await _context.ListRooms.ToListAsync()) :
                          Problem("Entity set 'TESTDTContext.ListRooms'  is null.");
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListRooms/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null || _context.ListRooms == null)
            {
                return NotFound();
            }

            var listRoom = await _context.ListRooms
                .FirstOrDefaultAsync(m => m.IdRoom == id);
            if (listRoom == null)
            {
                return NotFound();
            }

            return View(listRoom);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListRooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ListRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRoom,NameRoom,ListUser,Floor,Note")] ListRoom listRoom)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(listRoom);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListRooms/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            

            if (id == null || _context.ListRooms == null)
            {
                return NotFound();
            }

            var listRoom = await _context.ListRooms.FindAsync(id);
            if (listRoom == null)
            {
                return NotFound();
            }
            return View(listRoom);
        }

        // POST: ListRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdRoom,NameRoom,ListUser,Floor,Note")] ListRoom listRoom)
        {
            if (id != listRoom.IdRoom)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listRoom);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListRoomExists(listRoom.IdRoom))
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
            return View(listRoom);
        }

        [Authorize(Roles = "Administrator")]
        // GET: ListRooms/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null || _context.ListRooms == null)
            {
                return NotFound();
            }

            var listRoom = await _context.ListRooms
                .FirstOrDefaultAsync(m => m.IdRoom == id);
            if (listRoom == null)
            {
                return NotFound();
            }

            return View(listRoom);
        }

        // POST: ListRooms/Delete/5
        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ListRooms == null)
            {
                return Problem("Entity set 'TESTDTContext.ListRooms'  is null.");
            }
            var listRoom = await _context.ListRooms.FindAsync(id);
            if (listRoom != null)
            {
                _context.ListRooms.Remove(listRoom);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ListRoomExists(int id)
        {
          return (_context.ListRooms?.Any(e => e.IdRoom == id)).GetValueOrDefault();
        }
    }
}
