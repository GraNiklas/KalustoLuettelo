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
    public class KategoriatController : Controller
    {
        private readonly KaluDbContext _context;

        public KategoriatController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Kategoriat
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kategoriat.ToListAsync());
        }

        // GET: Kategoriat/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoriat
                .FirstOrDefaultAsync(m => m.KategoriaId == id);
            if (kategoria == null)
            {
                return NotFound();
            }

            return View(kategoria);
        }

        // GET: Kategoriat/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kategoriat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KategoriaId,KategoriaNimi")] Kategoria kategoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kategoria);
        }

        // GET: Kategoriat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoriat.FindAsync(id);
            if (kategoria == null)
            {
                return NotFound();
            }
            return View(kategoria);
        }

        // POST: Kategoriat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KategoriaId,KategoriaNimi")] Kategoria kategoria)
        {
            if (id != kategoria.KategoriaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriaExists(kategoria.KategoriaId))
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
            return View(kategoria);
        }

        // GET: Kategoriat/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kategoria = await _context.Kategoriat
                .FirstOrDefaultAsync(m => m.KategoriaId == id);
            if (kategoria == null)
            {
                return NotFound();
            }

            return View(kategoria);
        }

        // POST: Kategoriat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategoria = await _context.Kategoriat.FindAsync(id);
            if (kategoria != null)
            {
                _context.Kategoriat.Remove(kategoria);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KategoriaExists(int id)
        {
            return _context.Kategoriat.Any(e => e.KategoriaId == id);
        }
    }
}
