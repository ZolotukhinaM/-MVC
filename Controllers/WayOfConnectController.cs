using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Курсовая_работа_MVC.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Курсовая_работа_MVC.Controllers
{
    public class WayOfConnectController : Controller
    {
        private readonly NewAppContext _context;

        public WayOfConnectController(NewAppContext context)
        {
            _context = context;
        }

        // GET: WayOfConnect
        public async Task<IActionResult> Index()
        {
            return View(await _context.WaysOfConnect.ToListAsync());
        }

        // GET: WayOfConnect/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WayOfConnect/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Way")] WayOfConnect wayOfConnect)
        {
            // Проверка на существование способа связи с таким же названием
            if (_context.WaysOfConnect.Any(w => w.Way == wayOfConnect.Way))
            {
                ModelState.AddModelError(nameof(wayOfConnect.Way), "Способ связи с таким названием уже существует");
            }

            if (ModelState.IsValid)
            {
                _context.Add(wayOfConnect);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wayOfConnect);
        }

        // GET: WayOfConnect/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wayOfConnect = await _context.WaysOfConnect.FindAsync(id);
            if (wayOfConnect == null)
            {
                return NotFound();
            }
            return View(wayOfConnect);
        }

        // POST: WayOfConnect/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Way")] WayOfConnect wayOfConnect)
        {
            if (id != wayOfConnect.Id)
            {
                return NotFound();
            }

            // Проверка на существование другого способа связи с таким же названием
            if (_context.WaysOfConnect.Any(w => w.Way == wayOfConnect.Way && w.Id != wayOfConnect.Id))
            {
                ModelState.AddModelError(nameof(wayOfConnect.Way), "Способ связи с таким названием уже существует");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wayOfConnect);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WayOfConnectExists(wayOfConnect.Id))
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
            return View(wayOfConnect);
        }

        // GET: WayOfConnect/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wayOfConnect = await _context.WaysOfConnect
                .FirstOrDefaultAsync(m => m.Id == id);
            if (wayOfConnect == null)
            {
                return NotFound();
            }

            return View(wayOfConnect);
        }

        // POST: WayOfConnect/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var wayOfConnect = await _context.WaysOfConnect.FindAsync(id);
            _context.WaysOfConnect.Remove(wayOfConnect);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WayOfConnectExists(long id)
        {
            return _context.WaysOfConnect.Any(e => e.Id == id);
        }
    }
}