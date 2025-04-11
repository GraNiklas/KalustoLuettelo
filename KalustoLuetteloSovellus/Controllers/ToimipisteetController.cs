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
    public class ToimipisteetController : Controller
    {
        private readonly KaluDbContext _context;

        public ToimipisteetController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Toimipisteet
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Toimipisteet.ToListAsync());
        }

        // GET: Toimipisteet/Details/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toimipiste = await _context.Toimipisteet
                .FirstOrDefaultAsync(m => m.ToimipisteId == id);
            if (toimipiste == null)
            {
                return NotFound();
            }

            return View(toimipiste);
        }

        // GET: Toimipisteet/Create
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Toimipisteet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Create([Bind("ToimipisteId,Oppilaitos,Kaupunki,ToimipisteNimi")] Toimipiste toimipiste)
        {
            if (ModelState.IsValid)
            {
                _context.Add(toimipiste);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toimipiste);
        }

        // GET: Toimipisteet/Edit/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toimipiste = await _context.Toimipisteet.FindAsync(id);
            if (toimipiste == null)
            {
                return NotFound();
            }
            return View(toimipiste);
        }

        // POST: Toimipisteet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int id, [Bind("ToimipisteId,Oppilaitos,Kaupunki,ToimipisteNimi")] Toimipiste toimipiste)
        {
            if (id != toimipiste.ToimipisteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(toimipiste);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ToimipisteExists(toimipiste.ToimipisteId))
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
            return View(toimipiste);
        }

        // GET: Toimipisteet/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toimipiste = await _context.Toimipisteet
                .FirstOrDefaultAsync(m => m.ToimipisteId == id);
            if (toimipiste == null)
            {
                return NotFound();
            }

            return View(toimipiste);
        }

        // POST: Toimipisteet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toimipiste = await _context.Toimipisteet.FindAsync(id);
            if (toimipiste != null)
            {
                _context.Toimipisteet.Remove(toimipiste);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ToimipisteExists(int id)
        {
            return _context.Toimipisteet.Any(e => e.ToimipisteId == id);
        }
    }
}
