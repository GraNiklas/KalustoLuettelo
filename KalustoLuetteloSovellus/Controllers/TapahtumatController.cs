using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalustoLuetteloSovellus.Models;
using System.Drawing.Printing;

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
        public async Task<IActionResult> Index(int pageSize = 10, int currentPage = 0)
        {
            // For the initial page load, set up filters and pagination info
            IQueryable<Tapahtuma> tapahtumat = _context.Tapahtumat
                .Include(t => t.Käyttäjä)
                .Include(t => t.Status)
                .Include(t => t.Tuote)
                .ThenInclude(tu => tu.Toimipiste);

            var totalTapahtumat = _context.Tapahtumat.Count();
            var totalPages = (int)Math.Ceiling((double)totalTapahtumat / pageSize);
            // Return the shell view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;

            

            // Set ViewBag for dropdowns
            ViewBag.Statuses = new SelectList(await _context.Statukset.ToListAsync(), "StatusId", "StatusNimi");
            ViewBag.Toimipisteet = new SelectList(await _context.Toimipisteet.ToListAsync(), "ToimipisteId", "KaupunkiJaToimipisteNimi");
            ViewBag.Tuotteet = new SelectList(await _context.Tuotteet.ToListAsync(), "TuoteId", "Kuvaus");

            return View();
        }

        private IQueryable<Tapahtuma> ApplySorting(IQueryable<Tapahtuma> tapahtumat, string sortField, bool descending)
        {
            return sortField switch
            {
                "TapahtumaId" => descending ? tapahtumat.OrderByDescending(t => t.TapahtumaId) : tapahtumat.OrderBy(t => t.TapahtumaId),
                "TuoteId" => descending ? tapahtumat.OrderByDescending(t => t.TuoteId) : tapahtumat.OrderBy(t => t.TuoteId),
                "Tuote" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.Kuvaus) : tapahtumat.OrderBy(t => t.Tuote.Kuvaus),
                "AloitusPvm" => descending ? tapahtumat.OrderByDescending(t => t.AloitusPvm) : tapahtumat.OrderBy(t => t.AloitusPvm),
                "LopetusPvm" => descending ? tapahtumat.OrderByDescending(t => t.LopetusPvm) : tapahtumat.OrderBy(t => t.LopetusPvm),
                "Status" => descending ? tapahtumat.OrderByDescending(t => t.Status.StatusNimi) : tapahtumat.OrderBy(t => t.Status.StatusNimi),
                "Kommentti" => descending ? tapahtumat.OrderByDescending(t => t.Kommentti) : tapahtumat.OrderBy(t => t.Kommentti),
                "IdNumero" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.IdNumero) : tapahtumat.OrderBy(t => t.Tuote.IdNumero),
                "Käyttäjä" => descending ? tapahtumat.OrderByDescending(t => t.Käyttäjä.Käyttäjätunnus) : tapahtumat.OrderBy(t => t.Käyttäjä.Käyttäjätunnus),
                "Toimipiste" => descending ? tapahtumat.OrderByDescending(t => t.Tuote.Toimipiste.ToimipisteNimi) : tapahtumat.OrderBy(t => t.Tuote.Toimipiste.ToimipisteNimi),
                _ => tapahtumat.OrderByDescending(t => t.AloitusPvm),
            };
        }


        [HttpGet]
        public async Task<IActionResult> GetTapahtumatPartial(int pageSize = 10, int currentPage = 0, int? statusId = null, int? toimipisteId = null, string? sortOrder = null)
        {
            IQueryable<Tapahtuma> tapahtumat = _context.Tapahtumat
                .Include(t => t.Käyttäjä)
                .Include(t => t.Status)
                .Include(t => t.Tuote)
                .ThenInclude(tu => tu.Toimipiste);

            // Apply sorting
            if (!string.IsNullOrEmpty(sortOrder))
            {
                bool descending = sortOrder?.EndsWith("_desc") == true;
                string sortField = sortOrder?.EndsWith("_desc") == true ? sortOrder?[..^5] : sortOrder;
                tapahtumat = ApplySorting(tapahtumat, sortField, descending);
            }

            // Apply filtering
            if (statusId.HasValue)
            {
                tapahtumat = tapahtumat.Where(t => t.StatusId == statusId.Value);
            }

            if (toimipisteId.HasValue)
            {
                tapahtumat = tapahtumat.Where(t => t.Tuote.ToimipisteId == toimipisteId.Value);
            }

            // Get total filtered count
            var totalTapahtumat = await _context.Tapahtumat.CountAsync();
            var totalFiltered = await tapahtumat.CountAsync();

            // Apply pagination
            var tapahtumatPaged = await tapahtumat
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToListAsync();


            ViewData["Kaikki"] = totalTapahtumat;
            ViewData["Suodatetut"] = totalFiltered;

            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalFiltered / pageSize);
            ViewData["PageSize"] = pageSize;

            return PartialView("_TapahtumatTablePartial", tapahtumatPaged);
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
