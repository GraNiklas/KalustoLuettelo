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
    public class TuotteetController : Controller
    {
        private readonly KaluDbContext _context;

        public TuotteetController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Tuotteet
        public async Task<IActionResult> Index()
        {
            var kaluDbContext = _context.Tuotteet.Include(t => t.Kategoria).Include(t => t.Status).Include(t => t.Toimipiste);
            return View(await kaluDbContext.ToListAsync());
        }

        // GET: Tuotteet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Status)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }

            return View(tuote);
        }

        // GET: Tuotteet/Create
        public IActionResult Create()
        {
            ViewData["KategoriaId"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaId");
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId");
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId");
            return View();
        }

        // POST: Tuotteet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TuoteId,IdNumero,KategoriaId,StatusId,Kuvaus,Kuva,OstoPvm,Hinta,Takuu,Aktiivinen,ToimipisteId")] Tuote tuote)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tuote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KategoriaId"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaId", tuote.KategoriaId);
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tuote.StatusId);
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId", tuote.ToimipisteId);
            return View(tuote);
        }

        // GET: Tuotteet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuote = await _context.Tuotteet.FindAsync(id);
            if (tuote == null)
            {
                return NotFound();
            }
            ViewData["KategoriaId"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaId", tuote.KategoriaId);
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tuote.StatusId);
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId", tuote.ToimipisteId);
            return View(tuote);
        }

        // POST: Tuotteet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TuoteId,IdNumero,KategoriaId,StatusId,Kuvaus,Kuva,OstoPvm,Hinta,Takuu,Aktiivinen,ToimipisteId")] Tuote tuote)
        {
            if (id != tuote.TuoteId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tuote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TuoteExists(tuote.TuoteId))
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
            ViewData["KategoriaId"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaId", tuote.KategoriaId);
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tuote.StatusId);
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId", tuote.ToimipisteId);
            return View(tuote);
        }

        // GET: Tuotteet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Status)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }

            return View(tuote);
        }

        // POST: Tuotteet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tuote = await _context.Tuotteet.FindAsync(id);
            if (tuote != null)
            {
                _context.Tuotteet.Remove(tuote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TuoteExists(int id)
        {
            return _context.Tuotteet.Any(e => e.TuoteId == id);
        }
    }
}
