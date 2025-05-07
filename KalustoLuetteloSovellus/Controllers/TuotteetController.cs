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
using SixLabors.ImageSharp.Formats.Png;

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
        public async Task<IActionResult> Index(int pageSize = 10, int currentPage = 0, string? kuvausHakusanalla = null, int? kategoriaId = null, bool? onAktiivinen = null, int? toimipisteId = null)
        {
            // Get the total number of products
            var totalTuotteet = _context.Tuotteet.Count();
            var totalPages = (int)Math.Ceiling((double)totalTuotteet / pageSize);

            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;


            // Ladataan kategoriat ja aktiivisuusvalinnat ViewBagiin
            ViewBag.Kategoriat = new SelectList(await _context.Kategoriat.ToListAsync(), "KategoriaId", "KategoriaNimi", kategoriaId);
            ViewBag.Toimipisteet = new SelectList(await _context.Toimipisteet.ToListAsync(), "ToimipisteId", "KaupunkiJaToimipisteNimi", toimipisteId);
            ViewBag.Aktiiviset = new SelectList(new[] { new { Value = true, Text = "Aktiivinen" }, new { Value = false, Text = "Ei aktiivinen" } }, "Value", "Text", onAktiivinen);

            return View();
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
            var totalTuotteet = await _context.Tuotteet.CountAsync();

            var tuotteetPaged = await tuotteet
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToListAsync();

            

            ViewData["Kaikki"] = totalTuotteet;
            ViewData["Suodatetut"] = totalFiltered;


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
                        // Lataa kuva ImageSharpilla
                        using var image = await Image.LoadAsync(tuote.KuvaFile.OpenReadStream());

                        // Pienennetään kuvaa, säilytetään max-koko 800x800
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(800, 800), // Maksimi koko 800x800
                            Mode = ResizeMode.Max       // Pienennetään niin, että molemmat ulottuvuudet mahtuvat maxiin
                        }));

                        // Tarkistetaan kuvatiedoston tyyppi ja pakataan sen mukaan
                        var extension = Path.GetExtension(tuote.KuvaFile.FileName).ToLower();

                        using var ms = new MemoryStream();
                        if (extension == ".jpg" || extension == ".jpeg")
                        {
                            // JPEG-pakkaus, voidaan säätää laatua
                            await image.SaveAsJpegAsync(ms, new JpegEncoder
                            {
                                Quality = 50 // Alhaisempi laatu vähentää tiedostokokoa (50% laatu on usein riittävä)
                            });
                        }
                        else if (extension == ".png")
                        {
                            // PNG-pakkaus, voi käyttää parempaa optimointia
                            await image.SaveAsPngAsync(ms, new PngEncoder
                            {
                                CompressionLevel = PngCompressionLevel.BestCompression // Paras pakkaus
                            });
                        }

                        // Tallennetaan pienennetty ja pakattu kuva byte array -muodossa
                        tuote.Kuva = ms.ToArray();
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
                // Jos kuva on valittu, käsitellään ja tallennetaan
                if (tuote.KuvaFile != null)
                {
                    try
                    {
                    using (var memoryStream = new MemoryStream())
                    {
                        using var image = await Image.LoadAsync(tuote.KuvaFile.OpenReadStream());

                        // Pienennetään kuvaa, säilytetään max-koko 800x800
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(800, 800),
                            Mode = ResizeMode.Max
                        }));

                        // Tarkistetaan kuvatiedoston tyyppi ja pakataan sen mukaan
                        var extension = Path.GetExtension(tuote.KuvaFile.FileName).ToLower();

                        using var ms = new MemoryStream();
                        if (extension == ".jpg" || extension == ".jpeg")
                        {
                            // Tallennetaan JPEG-kuva 50% laadulla
                            await image.SaveAsJpegAsync(ms, new JpegEncoder
                            {
                                Quality = 50
                            });
                        }
                        else if (extension == ".png")
                        {
                            // Tallennetaan PNG-kuva parhaalla pakkaustasolla
                            await image.SaveAsPngAsync(ms, new PngEncoder
                            {
                                CompressionLevel = PngCompressionLevel.BestCompression
                            });
                        }
                        else
                        {
                            // Jos ei ole JPEG tai PNG, ilmoita virheestä
                            ModelState.AddModelError("KuvaFile", "Vain JPG ja PNG kuvat hyväksytään.");
                            return View(tuote);  // Palautetaan takaisin muokkausnäkymään
                        }

                        // Tallennetaan kuva tietokantaan
                        olemassaOlevaTuote.Kuva = ms.ToArray();
                    }
                    }
                    catch (Exception ex)
                    {
                        // Jos kuvan käsittelyssä tapahtuu virhe, lisätään virhemalliin ja palataan takaisin
                        ModelState.AddModelError("", "Virhe kuvan käsittelyssä: " + ex.Message);
                        return View(tuote);
                    }
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
        public async Task<IActionResult> DowngradeExistingImage(int productId)
        {
            var tuote = await _context.Tuotteet.FindAsync(productId);
            if (tuote == null || tuote.Kuva == null)
                return NotFound("Tuotetta tai kuvaa ei löytynyt.");

            using var originalStream = new MemoryStream(tuote.Kuva);
            using var image = await Image.LoadAsync(originalStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 800), // tai haluamasi maksimi
                Mode = ResizeMode.Max // säilyttää kuvasuhteen, skaalaa niin että kumpikaan puoli ei ylitä kokoa
            }));

            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 75 });

            tuote.Kuva = outputStream.ToArray();
            _context.Tuotteet.Update(tuote);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kuva skaalattu ja tallennettu onnistuneesti." });
        }
    }
}
