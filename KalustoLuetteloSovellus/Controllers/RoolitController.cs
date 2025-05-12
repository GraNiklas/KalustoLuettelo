using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalustoLuetteloSovellus.Models;

namespace KalustoLuetteloSovellus.Controllers
{
    public class RoolitController : Controller
    {
        private readonly KaluDbContext _context;
        public RoolitController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Rooli
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Index()
        {
        return View(await _context.Roolit.ToListAsync());
        }

        // GET: Rooli/Details/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rooli = await _context.Roolit
                .FirstOrDefaultAsync(m => m.RooliId == id);
            if (rooli == null)
            {
                return NotFound();
            }

            return View(rooli);
        }

        // GET: Rooli/Create
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooli/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Create([Bind("RooliId,RooliNimi")] Rooli rooli)
        {
                _context.Add(rooli);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: Rooli/Edit/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rooli = await _context.Roolit.FindAsync(id);
            if (rooli == null)
            {
                return NotFound();
            }
            if (IsProtected(rooli))
            {
                TempData["ErrorMessage"] = "Ei voi muuttaa tarvittuja rooleja...";
                return RedirectToAction(nameof(Index));
            }
            return View(rooli);
        }

        // POST: Rooli/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int id, [Bind("RooliId,RooliNimi")] Rooli rooli)
        {
            if (id != rooli.RooliId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rooli);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RooliExists(rooli.RooliId))
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
            return View(rooli);
        }

        // GET: Rooli/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rooli = await _context.Roolit
                .FirstOrDefaultAsync(m => m.RooliId == id);
            if (rooli == null)
            {
                return NotFound();
            }

            if (IsProtected(rooli))
            {
                TempData["ErrorMessage"] = "Ei voi poistaa tarvittuja rooleja...";
                return RedirectToAction(nameof(Index));
            }
            return View(rooli);
        }

        // POST: Rooli/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rooli = await _context.Roolit.FindAsync(id);
            if (rooli != null)
            {
                _context.Roolit.Remove(rooli);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RooliExists(int id)
        {
            return _context.Roolit.Any(e => e.RooliId == id);
        }
        public bool IsProtected(Rooli rooli)
        {
            var protectedRoolit = new List<String> { "Admin", "User" };
            return protectedRoolit.Contains(rooli.RooliNimi);
        }
    }
}
