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
    public class KäyttäjätController : Controller
    {
        private readonly KaluDbContext _context;


        public KäyttäjätController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Käyttäjät

        //[ServiceFilter(typeof(AdminOnlyFilter))]
        //public async Task<IActionResult> Index()
        //{
        //    var kaluDbContext = _context.Käyttäjät.Include(k => k.Rooli);
        //    return View(await kaluDbContext.ToListAsync());
        //}

        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Index(string käyttäjäNimi = null, string? rooliId = null)
        {
            var käyttäjät = _context.Käyttäjät
                .Include(k => k.Rooli) // navigaatioprop
                .AsQueryable();

            if (!string.IsNullOrEmpty(käyttäjäNimi))
            {
                käyttäjät = käyttäjät.Where(k => k.Käyttäjätunnus.Contains(käyttäjäNimi));
            }


            ViewData["Kaikki"] = await _context.Käyttäjät.CountAsync();
            ViewData["Suodatetut"] = await käyttäjät.CountAsync();

            var roolit = await _context.Roolit.ToListAsync();


            return View(await käyttäjät.ToListAsync());
        }



        // GET: Käyttäjät/Details/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjät
                .Include(k => k.Rooli)
                .FirstOrDefaultAsync(m => m.KäyttäjäId == id);
            if (käyttäjä == null)
            {
                return NotFound();
            }

            return View(käyttäjä);
        }

        // GET: Käyttäjät/Create
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public IActionResult Create()
        {
            ViewData["RooliId"] = new SelectList(_context.Roolit, "RooliId", "RooliId");
            return View();
        }

        // POST: Käyttäjät/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Create([Bind("KäyttäjäId,Käyttäjätunnus,Salasana,RooliId")] Käyttäjä käyttäjä)
        {
            if (ModelState.IsValid)
            {
                _context.Add(käyttäjä);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RooliId"] = new SelectList(_context.Roolit, "RooliId", "RooliId", käyttäjä.RooliId);
            return View(käyttäjä);
        }

        // GET: Käyttäjät/Edit/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjät.FindAsync(id);
            if (käyttäjä == null)
            {
                return NotFound();
            }
            ViewData["RooliId"] = new SelectList(_context.Roolit, "RooliId", "RooliId", käyttäjä.RooliId);
            return View(käyttäjä);
        }

        // POST: Käyttäjät/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int id, [Bind("KäyttäjäId,Käyttäjätunnus,Salasana,RooliId")] Käyttäjä käyttäjä)
        {
            if (id != käyttäjä.KäyttäjäId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(käyttäjä);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KäyttäjäExists(käyttäjä.KäyttäjäId))
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
            ViewData["RooliId"] = new SelectList(_context.Roolit, "RooliId", "RooliId", käyttäjä.RooliId);
            return View(käyttäjä);
        }

        // GET: Käyttäjät/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjät
                .Include(k => k.Rooli)
                .FirstOrDefaultAsync(m => m.KäyttäjäId == id);
            if (käyttäjä == null)
            {
                return NotFound();
            }

            return View(käyttäjä);
        }

        // POST: Käyttäjät/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var käyttäjä = await _context.Käyttäjät.FindAsync(id);
            if (käyttäjä != null)
            {
                _context.Käyttäjät.Remove(käyttäjä);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KäyttäjäExists(int id)
        {
            return _context.Käyttäjät.Any(e => e.KäyttäjäId == id);
        }
    }
}
