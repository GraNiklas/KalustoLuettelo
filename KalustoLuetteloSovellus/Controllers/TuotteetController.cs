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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO;
using System.Threading.Tasks;
using System;

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
        public IActionResult Index(int currentPage = 0, int pageSize = 10)
        {
            // Get the total number of products
            var totalTuotteet = _context.Tuotteet.Count();
            var totalPages = (int)Math.Ceiling((double)totalTuotteet / pageSize);

            // Ensure currentPage is within bounds
            currentPage = Math.Max(0, Math.Min(currentPage, totalPages - 1));



            // Get the products for the current page
            var tuotteet = _context.Tuotteet
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;
            ViewData["PageSize"] = pageSize;

            return View(tuotteet);
        }

        [HttpGet]
        public async Task<IActionResult> GetTuotteetPartial(int pageSize = 10, int currentPage = 0, string? kuvausHakusanalla = null, int? kategoriaId = null, bool? onAktiivinen = null, int? toimipisteId = null)
        {
            var tuotteet = _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat).ThenInclude(t => t.Status)
                .Include(t => t.Tapahtumat).ThenInclude(t => t.Käyttäjä)
                .AsQueryable();

            if (!string.IsNullOrEmpty(kuvausHakusanalla))
                tuotteet = tuotteet.Where(t => t.Kuvaus.Contains(kuvausHakusanalla));
            if (kategoriaId.HasValue)
                tuotteet = tuotteet.Where(t => t.KategoriaId == kategoriaId.Value);
            if (onAktiivinen.HasValue)
                tuotteet = tuotteet.Where(t => t.Aktiivinen == onAktiivinen.Value);
            if (toimipisteId.HasValue)
                tuotteet = tuotteet.Where(t => t.ToimipisteId == toimipisteId.Value);

            var totalFiltered = await tuotteet.CountAsync();

            var tuotteetPaged = await tuotteet
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ladataan kategoriat ja aktiivisuusvalinnat ViewBagiin
            ViewBag.Kategoriat = new SelectList(await _context.Kategoriat.ToListAsync(), "KategoriaId", "KategoriaNimi", kategoriaId);
            ViewBag.Toimipisteet = new SelectList(await _context.Toimipisteet.ToListAsync(), "ToimipisteId", "KaupunkiJaToimipisteNimi", toimipisteId);
            ViewBag.Aktiiviset = new SelectList(new[] { new { Value = true, Text = "Aktiivinen" }, new { Value = false, Text = "Ei aktiivinen" } }, "Value", "Text", onAktiivinen);

            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalFiltered / pageSize);

            return PartialView("_TuotteetPartial", tuotteetPaged);
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

        [HttpPost]
        public async Task<IActionResult> CropExistingImage(int productId, IFormFile file)
        {
            // Oletetaan, että meillä on tiedot jo olemassa olevasta kuvasta, joka on tallennettu tietokantaan.
            var existingImagePath = Path.Combine("wwwroot/images", "existing_image.jpg"); // Tai hae polku tietokannasta

            // Lataa olemassa oleva kuva
            using var image = await Image.LoadAsync(existingImagePath);

            // Rajaa kuva (tässä esimerkissä rajataan alue 100px x 100px ja 300px x 300px)
            var cropRectangle = new Rectangle(100, 100, 300, 300);
            image.Mutate(x => x.Crop(cropRectangle));

            // Tallenna kuva uudelleen
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var savePath = Path.Combine("wwwroot/images", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));

            await image.SaveAsJpegAsync(savePath, new JpegEncoder
            {
                Quality = 75 // Voit säätää laatua
            });

            // Palauta uusi polku tai vaihtoehtoisesti voit tallentaa tietokantaan uuden kuvan polun
            return Ok(new { ImagePath = "/images/" + fileName });
        }
    }
}
