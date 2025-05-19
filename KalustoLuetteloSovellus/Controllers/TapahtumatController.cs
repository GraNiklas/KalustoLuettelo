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
        public async Task<IActionResult> Index(int pageSize = 10, int currentPage = 0, int? statusId = null, int? toimipisteId = null, int? tuoteId = null, string? sortOrder = null)
        {
            // For the initial page load, set up filters and pagination info

            var totalTapahtumat = await _context.Tapahtumat.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalTapahtumat / pageSize);
            
            // Return the shell view
            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;



            // Set ViewBag for dropdowns
            ViewBag.Statuses = new SelectList(await _context.Statukset.ToListAsync(), "StatusId", "StatusNimi",statusId);
            ViewBag.Toimipisteet = new SelectList(await _context.Toimipisteet.ToListAsync(), "ToimipisteId", "KaupunkiJaToimipisteNimi",toimipisteId);
            ViewBag.Tuotteet = new SelectList(await _context.Tuotteet.ToListAsync(), "TuoteId", "Kuvaus", tuoteId);

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
                _ => tapahtumat.OrderByDescending(t => t.Tuote.IdNumero),
            };
        }


        [HttpGet]
        public async Task<IActionResult> GetTapahtumatPartial(int pageSize = 10, int currentPage = 0, int? statusId = null, int? toimipisteId = null, int? tuoteId = null, string? sortOrder = null)
        {
            IQueryable<Tapahtuma> tapahtumat = _context.Tapahtumat
                .Include(t => t.Käyttäjä)
                .Include(t => t.Status)
                .Include(t => t.Tuote)
                .ThenInclude(tu => tu.Toimipiste);

            // Apply sorting
            if(!string.IsNullOrEmpty(sortOrder))
{
                bool descending = sortOrder.EndsWith("_desc");
                string sortField = descending ? sortOrder[..^5] : sortOrder;

                if (!string.IsNullOrEmpty(sortField))
                {
                    tapahtumat = ApplySorting(tapahtumat, sortField, descending);
                }
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
            if (tuoteId.HasValue)
            {
                tapahtumat = tapahtumat.Where(t => t.TuoteId == tuoteId.Value);
            }

            // Get total filtered count
            var totalTapahtumat = await _context.Tapahtumat.CountAsync();
            var totalFiltered = await tapahtumat.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalFiltered / pageSize);

            // lähetä suodatettujen sivujen määrä
            Response.Headers["X-TotalPages"] = totalPages.ToString();

            // Apply pagination
            var tapahtumatPaged = await tapahtumat
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToListAsync();


            ViewData["Kaikki"] = totalTapahtumat;
            ViewData["Suodatetut"] = totalFiltered;


            return PartialView("_TapahtumaRivitPartial", tapahtumatPaged);
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Palauta(int tuoteId)
        {
            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t => t.Status)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(t => t.TuoteId == tuoteId);

            if (tuote == null)
            {
                Console.WriteLine("tuote on null ");
                return RedirectToAction("Index", "Home");
            }
            ViewData["Käyttäjätunnus"] = HttpContext.Session.GetString("käyttäjätunnus");
            ViewData["TuoteId"] = tuote.TuoteId;
            ViewData["IdNumero"] = tuote.IdNumero;
            ViewData["StatusNimi"] = new SelectList(_context.Statukset, "StatusId", "StatusNimi");

            var käyttäjäId = HttpContext.Session.GetInt32("KäyttäjäId");
            var käyttäjä = await _context.Käyttäjät.FirstOrDefaultAsync(k => k.KäyttäjäId == käyttäjäId);
            if (käyttäjä == null || käyttäjäId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var tapahtumat = await _context.Tapahtumat
                .Where(t => t.TuoteId == tuoteId)
                .OrderByDescending(t => t.AloitusPvm)
                .ToListAsync();

            //ensin lajitellaan että saadaan viimeisin sitten haetaan käyttäjän tekemä tapahtuma tuotteelle niin olisi mahdollisesti viimesiin tapahtuma
            var tapahtuma = tapahtumat
                .FirstOrDefault(t => RoleHelper.IsUser(t.KäyttäjäId, HttpContext) || RoleHelper.IsAdmin(HttpContext)); // katotaan onko käyttäjän tekemä tai oletko admin

            if (tapahtuma == null) return View("Error"); // error jos ei löydy tapahtumaa

            // tapahtuma pitää kloonata

            var palautusTapahtuma = new Tapahtuma(); // luodaan uusi tapahtuma

            palautusTapahtuma.Käyttäjä = käyttäjä; // kopioidaan käyttäjä edellisestä tapahtumasta
            palautusTapahtuma.Tuote = tuote; // kopioidaan tuote edellisestä tapahtumasta
            palautusTapahtuma.Status = tapahtuma.Status; // kopioidaan status edellisestä tapahtumasta



            // kopioidaan tiedot edellisestä
            palautusTapahtuma.TuoteId = tapahtuma.TuoteId;
            palautusTapahtuma.KäyttäjäId = tapahtuma.KäyttäjäId; 
            palautusTapahtuma.Kommentti = tapahtuma.Kommentti + "\n" + "PALAUTUS";

            palautusTapahtuma.AloitusPvm = DateTime.Now;
            palautusTapahtuma.LopetusPvm = DateTime.Now; // en tiedä tarviiko muuttaa lopetus päivää palautuspäiväksi mutta ehkäpä.
            palautusTapahtuma.StatusId = 60001; // status vapaa


            return View("Create",palautusTapahtuma);
        }

        // GET: Tapahtumat/Create
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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
            tapahtuma.AloitusPvm = DateTime.Now;
            tapahtuma.LopetusPvm = DateTime.Now.AddDays(7); // lisätään vaikka viikko lopetuspäivään defaultiks

            

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
                tapahtuma.AloitusPvm = tapahtuma.AloitusPvm.AddHours(DateTime.Now.Hour);
                tapahtuma.AloitusPvm = tapahtuma.AloitusPvm.AddMinutes(DateTime.Now.Minute);
                tapahtuma.AloitusPvm = tapahtuma.AloitusPvm.AddSeconds(DateTime.Now.Second);

                _context.Add(tapahtuma);
                await _context.SaveChangesAsync();
                return RedirectToAction("User", "Home");
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
