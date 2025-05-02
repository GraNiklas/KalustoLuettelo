using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalustoLuetteloSovellus.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Routing;

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
        public async Task<IActionResult> Index(string kuvausHakusanalla = null, int ? kategoriaId = null, bool? onAktiivinen = null)
        {
            var tuotteet = _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Status)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Käyttäjä)
                .AsQueryable();

            

            if (!string.IsNullOrEmpty(kuvausHakusanalla))
            {
                tuotteet = tuotteet.Where(t => t.Kuvaus.Contains(kuvausHakusanalla));
            }

            // Laske kaikki tuotteet ja tallenna ViewData
            ViewData["Kaikki"] = await tuotteet.CountAsync();

            // Suodatus kategorian mukaan (jos kategoriaId annettu)
            if (kategoriaId.HasValue)
            {
                tuotteet = tuotteet.Where(t => t.KategoriaId == kategoriaId.Value);
            }

            // Suodatus aktiivisuuden mukaan (jos onAktiivinen annettu)
            if (onAktiivinen.HasValue)
            {
                tuotteet = tuotteet.Where(t => t.Aktiivinen == onAktiivinen.Value);
            }

            // Laske suodatetut tuotteet ja tallenna ViewData
            ViewData["Suodatetut"] = await tuotteet.CountAsync();

            // Ladataan kategoriat ja aktiivisuusvalinnat ViewBagiin
            ViewBag.Kategoriat = new SelectList(await _context.Kategoriat.ToListAsync(), "KategoriaId", "KategoriaNimi", kategoriaId);
            ViewBag.Aktiiviset = new SelectList(new[] { new { Value = true, Text = "Aktiivinen" }, new { Value = false, Text = "Ei aktiivinen" } }, "Value", "Text", onAktiivinen);

            
            return View(await tuotteet.ToListAsync());
        }

        public async Task<IActionResult> _StatusPartial(int statusId)
        {
            var status = await _context.Statukset.FirstOrDefaultAsync(s => s.StatusId == statusId);
            return PartialView("_StatusPartial",status);
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
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t=>t.Status)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t=>t.Käyttäjä)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tuote);
        }

        
        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t => t.Status)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }

            return PartialView("_TuoteKorttiPartial",tuote);
        }
        // GET: Tuotteet/Create
        public IActionResult Create()
        {
            ViewData["KategoriaNimi"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaNimi");
            ViewData["StatusNimi"] = new SelectList(_context.Statukset, "StatusId", "StatusNimi");
            ViewData["ToimipisteNimi"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "KaupunkiJaToimipisteNimi");
            return View();
        }

        // POST: Tuotteet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Tuote tuote)
        {
            if (ModelState.IsValid)
            {
                if(tuote.KuvaFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await tuote.KuvaFile.CopyToAsync(memoryStream);
                        tuote.Kuva = memoryStream.ToArray();
                    }
                }
                _context.Add(tuote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KategoriaId"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaId", tuote.KategoriaId);
            //ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tuote.StatusId);
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId", tuote.ToimipisteId);

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tuote);
        }

        // GET: Tuotteet/Edit/5
        //[ServiceFilter(typeof(AdminOnlyFilter))]
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
                   
            ViewData["KategoriaNimi"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaNimi",tuote.KategoriaId);
            //ViewData["StatusNimi"] = new SelectList(_context.Statukset, "StatusId", "StatusNimi", tuote.StatusId);
            ViewData["ToimipisteNimi"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "KaupunkiJaToimipisteNimi", tuote.ToimipisteId);

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tuote);
        }
        
        // POST: Tuotteet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int id, Tuote tuote)
        {
            if (id != tuote.TuoteId)
            {
                return NotFound();
            }
            var olemassaOlevaTuote = await _context.Tuotteet.FindAsync(id);
            if (olemassaOlevaTuote == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                olemassaOlevaTuote.TuoteId = tuote.TuoteId;
                olemassaOlevaTuote.IdNumero = tuote.IdNumero;
                olemassaOlevaTuote.KategoriaId = tuote.KategoriaId;
                //olemassaOlevaTuote.StatusId = tuote.StatusId;
                olemassaOlevaTuote.Kuvaus = tuote.Kuvaus;
                olemassaOlevaTuote.OstoPvm = tuote.OstoPvm;
                olemassaOlevaTuote.Hinta = tuote.Hinta;
                olemassaOlevaTuote.Takuu = tuote.Takuu;
                olemassaOlevaTuote.Aktiivinen = tuote.Aktiivinen;
                olemassaOlevaTuote.ToimipisteId = tuote.ToimipisteId;
                if (tuote.KuvaFile != null)
                {
                    // Convert uploaded file to byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await tuote.KuvaFile.CopyToAsync(memoryStream);
                        olemassaOlevaTuote.Kuva = memoryStream.ToArray();
                    }

                    // Explicitly mark ProfilePicture as modified
                    _context.Entry(olemassaOlevaTuote).Property(p => p.Kuva).IsModified = true;
                }
                try
                {
                    _context.Update(olemassaOlevaTuote);
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
            //ViewData["StatusId"] = new SelectList(_context.Statukset, "StatusId", "StatusId", tuote.StatusId);
            ViewData["ToimipisteId"] = new SelectList(_context.Toimipisteet, "ToimipisteId", "ToimipisteId", tuote.ToimipisteId);
            return View(tuote);
        }

        // GET: Tuotteet/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tuote = await _context.Tuotteet
                .Include(t => t.Kategoria)
                //.Include(t => t.Status)
                .Include(t => t.Toimipiste)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }

            TempData["ReturnUrl"] = Request.Headers["Referer"].ToString(); // tähän talletetaan viimeisin sivu mihin halutaan palata takasin napista.

            return View(tuote);
        }

        // POST: Tuotteet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
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

        public async Task<IActionResult> _TapahtumaRivitPartial()
        {
            var tapahtumat = await _context.Tapahtumat.Include(t => t.Käyttäjä).Include(t => t.Status).Include(t => t.Tuote).ThenInclude(tu => tu.Toimipiste).OrderBy(t=>t.AloitusPvm).ToListAsync();
            
            return PartialView("_TapahtumaRivitPartial", tapahtumat);
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
                return RedirectToAction("Index", "Tuotteet");
            }
        }
    }
}
