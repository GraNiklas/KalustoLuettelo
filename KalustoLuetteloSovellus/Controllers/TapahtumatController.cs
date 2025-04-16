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
    public class TapahtumatController : Controller
    {
        private readonly KaluDbContext _context;

        public TapahtumatController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Tapahtumat
        public async Task<IActionResult> Index()
        {
            var kaluDbContext = _context.Tapahtumat.Include(t => t.Käyttäjä).Include(t=>t.Status).Include(t => t.Tuote).ThenInclude(tu => tu.Toimipiste);
            return View(await kaluDbContext.ToListAsync());
        }

        // GET: Tapahtumat/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapahtuma = await _context.Tapahtumat
                .Include(t => t.Käyttäjä)
                .Include(t => t.Tuote)
                .FirstOrDefaultAsync(m => m.TapahtumaId == id);
            if (tapahtuma == null)
            {
                return NotFound();
            }

            return View(tapahtuma);
        }

        // GET: Tapahtumat/Create
        public async Task<IActionResult> Create(int tuoteId)
        {
            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(t => t.TuoteId == tuoteId);
            Console.WriteLine("tuoteid create: ", tuoteId);
            if(tuote == null)
            {
                Console.WriteLine("tuote on null ");
                return RedirectToAction("Index","Home");
            }
            ViewData["Käyttäjätunnus"] = HttpContext.Session.GetString("käyttäjätunnus");
            ViewData["TuoteId"] = tuote.TuoteId;
            ViewData["IdNumero"] = tuote.IdNumero;
            ViewData["StatusNimi"] = new SelectList(_context.Statukset, "StatusId", "StatusNimi");

            var tapahtuma = new Tapahtuma();
            var käyttäjäId = HttpContext.Session.GetInt32("KäyttäjäId");
            var käyttäjä = await _context.Käyttäjät.FirstOrDefaultAsync(k => k.KäyttäjäId == käyttäjäId);
            if(käyttäjä == null || käyttäjäId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            tapahtuma.KäyttäjäId = (int)käyttäjäId;
            tapahtuma.Käyttäjä = käyttäjä;
            tapahtuma.TuoteId = tuoteId;
            tapahtuma.Tuote = tuote;
            tapahtuma.AloitusPvm = DateOnly.FromDateTime(DateTime.Today);
            tapahtuma.LopetusPvm = DateOnly.FromDateTime(DateTime.Today.AddDays(7)); // lisätään vaikka viikko lopetuspäivään defaultiks
            return View(tapahtuma);
        }

        // POST: Tapahtumat/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tapahtuma tapahtuma)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tapahtuma);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KäyttäjäId"] = new SelectList(_context.Käyttäjät, "KäyttäjäId", "KäyttäjäId", tapahtuma.KäyttäjäId);
            ViewData["TuoteId"] = new SelectList(_context.Tuotteet, "TuoteId", "TuoteId", tapahtuma.TuoteId);
            return View(tapahtuma);
        }

        // GET: Tapahtumat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapahtuma = await _context.Tapahtumat.FindAsync(id);
            if (tapahtuma == null)
            {
                return NotFound();
            }
            ViewData["KäyttäjäId"] = new SelectList(_context.Käyttäjät, "KäyttäjäId", "Käyttäjätunnus", tapahtuma.KäyttäjäId);
            ViewData["TuoteId"] = new SelectList(_context.Tuotteet, "TuoteId", "Kuvaus", tapahtuma.TuoteId);
            return View(tapahtuma);
        }

        // POST: Tapahtumat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TapahtumaId,TuoteId,AloitusPvm,LopetusPvm,Kommentti,KäyttäjäId")] Tapahtuma tapahtuma)
        {
            if (id != tapahtuma.TapahtumaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tapahtuma);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TapahtumaExists(tapahtuma.TapahtumaId))
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
            ViewData["KäyttäjäId"] = new SelectList(_context.Käyttäjät, "KäyttäjäId", "KäyttäjäId", tapahtuma.KäyttäjäId);
            ViewData["TuoteId"] = new SelectList(_context.Tuotteet, "TuoteId", "TuoteId", tapahtuma.TuoteId);
            return View(tapahtuma);
        }

        // GET: Tapahtumat/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapahtuma = await _context.Tapahtumat
                .Include(t => t.Käyttäjä)
                .Include(t => t.Tuote)
                .FirstOrDefaultAsync(m => m.TapahtumaId == id);
            if (tapahtuma == null)
            {
                return NotFound();
            }

            return View(tapahtuma);
        }

        // POST: Tapahtumat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tapahtuma = await _context.Tapahtumat.FindAsync(id);
            if (tapahtuma != null)
            {
                _context.Tapahtumat.Remove(tapahtuma);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TapahtumaExists(int id)
        {
            return _context.Tapahtumat.Any(e => e.TapahtumaId == id);
        }
    }
}
