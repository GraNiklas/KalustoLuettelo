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
        public async Task<IActionResult> Index(int? statusId = null,int? toimipisteId = null,string? sortOrder = null)
        {
            //LAJITTELU
            ViewData["TuoteIdSortParam"] = String.IsNullOrEmpty(sortOrder) ? "TuoteId" : "";
            ViewData["KuvausSortParam"] = String.IsNullOrEmpty(sortOrder) ? "Tuote" : "";
            ViewData["StatusSortParam"] = String.IsNullOrEmpty(sortOrder) ? "Status" : "";
            ViewData["AloitusSortParam"] = String.IsNullOrEmpty(sortOrder) ? "AloitusPvm" : "";
            ViewData["KommenttiSortParam"] = String.IsNullOrEmpty(sortOrder) ? "Kommentti" : "";
            ViewData["LopetusSortParam"] = String.IsNullOrEmpty(sortOrder) ? "LopetusPvm" : "";
            ViewData["IdNumeroSortParam"] = String.IsNullOrEmpty(sortOrder) ? "IdNumero" : "";
            ViewData["KäyttäjäSortParam"] = String.IsNullOrEmpty(sortOrder) ? "Käyttäjä" : "";
            ViewData["ToimipisteSortParam"] = String.IsNullOrEmpty(sortOrder) ? "Toimipiste" : "";

            // Parse field and direction
            string sortField = sortOrder?.EndsWith("_desc") == true
                ? sortOrder[..^5] // remove "_desc"
                : sortOrder;

            bool descending = sortOrder?.EndsWith("_desc") == true;
            //LAJITTELU


            IQueryable<Tapahtuma> tapahtumat = _context.Tapahtumat.Include(t => t.Käyttäjä).Include(t => t.Status).Include(t => t.Tuote).ThenInclude(tu => tu.Toimipiste);

            ViewData["Kaikki"] = tapahtumat.Count();

            //FILTERÖINTI
            //STATUS
            if (statusId.HasValue)
            {
                tapahtumat = tapahtumat.Where(t => t.StatusId == statusId.Value);
            }
            //TOIMIPISTE
            if (toimipisteId.HasValue)
            {
                tapahtumat = tapahtumat.Where(t => t.Tuote.ToimipisteId == toimipisteId.Value);
            }
            //LAJITTELU
            if (!string.IsNullOrEmpty(sortField))
            {
                tapahtumat = sortField switch
                {
                    "TuoteId" => descending ? tapahtumat.OrderByDescending(t => t.TuoteId) : tapahtumat.OrderBy(t => t.TuoteId),
                    "Tuote" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.Kuvaus) : tapahtumat.OrderBy(t => t.Tuote.Kuvaus),
                    "AloitusPvm" => descending ? tapahtumat.OrderByDescending(t => t.AloitusPvm) : tapahtumat.OrderBy(t => t.AloitusPvm),
                    "LopetusPvm" => descending ? tapahtumat.OrderByDescending(t => t.LopetusPvm) : tapahtumat.OrderBy(t => t.LopetusPvm),
                    "Status" => descending ? tapahtumat.OrderByDescending(t => t.Status.StatusNimi) : tapahtumat.OrderBy(t => t.Status.StatusNimi),
                    "Kommentti" => descending ? tapahtumat.OrderByDescending(t => t.Kommentti) : tapahtumat.OrderBy(t => t.Kommentti),
                    "IdNumero" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.IdNumero) : tapahtumat.OrderBy(t => t.Tuote.IdNumero),
                    "Käyttäjä" => descending ? tapahtumat.OrderByDescending(t => t.Käyttäjä.Käyttäjätunnus) : tapahtumat.OrderBy(t => t.Käyttäjä.Käyttäjätunnus),
                    "Toimipiste" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.ToimipisteId) : tapahtumat.OrderBy(t => t.Tuote.ToimipisteId),
                    _ => tapahtumat.OrderByDescending(t => t.AloitusPvm),
                };
            }


            var statukset = await _context.Statukset.ToListAsync();
            var toimpisteet = await _context.Toimipisteet.ToListAsync();

            ViewBag.Statuses = new SelectList(statukset, "StatusId", "StatusNimi",statusId);
            ViewBag.Toimipisteet = new SelectList(toimpisteet, "ToimipisteId", "KaupunkiJaToimipisteNimi",toimipisteId);

            
            ViewData["Suodatetut"] = tapahtumat.Count();

            return View(await tapahtumat.ToListAsync());
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

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tapahtuma);
        }

        // GET: Tapahtumat/Create
        public async Task<IActionResult> Create(int tuoteId)
        {
            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t => t.Status)
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

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

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
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tapahtuma.StatusId);
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
            ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusNimi", tapahtuma.StatusId);

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tapahtuma);
        }

        // POST: Tapahtumat/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tapahtuma tapahtuma)
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

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

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
        public ActionResult ReturnToPreviousPage()
        {
            // katotaan onko täällä mitään
            var returnUrl = TempData["ReturnUrl"]?.ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // jos on niin palataan sinne
                return Redirect(returnUrl);
            }
            else
            {
                // muuten palataan index sivulle
                return RedirectToAction("Index", "Tapahtumat");
            }
        }
    }
}
