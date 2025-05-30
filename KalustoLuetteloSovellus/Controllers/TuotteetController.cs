﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalustoLuetteloSovellus.Models;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;


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
        public async Task<IActionResult> Index(int pageSize = 10, int currentPage = 0, string? kuvausHakusanalla = null, int? kategoriaId = null, bool? onAktiivinen = null, int? toimipisteId = null, int? statusId = null)
        {
            // Get the total number of products
            var totalTuotteet = await _context.Tuotteet.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalTuotteet / pageSize);


            //// Ensure currentPage is within bounds
            currentPage = Math.Max(0, Math.Min(currentPage, totalPages - 1));


            // NÄITÄ EI TARVITSE HAKEA TÄSSÄ KOHTAA KUN JAVASCRIPT HAKEE SEN KUITENKIN, JOS TÄN JÄTTÄÄ POIS NIIN TUOTE SIVU LATAA NOPEAMMIN

            //// Get the products for the current page
            //var tuotteet = await _context.Tuotteet
            //    .Skip(currentPage * pageSize)
            //    .Take(pageSize)
            //    .ToListAsync();


            //// Ladataan kategoriat ja aktiivisuusvalinnat ViewBagiin
            ViewBag.Kategoriat = new SelectList(await _context.Kategoriat.ToListAsync(), "KategoriaId", "KategoriaNimi", kategoriaId);
            ViewBag.Toimipisteet = new SelectList(await _context.Toimipisteet.ToListAsync(), "ToimipisteId", "KaupunkiJaToimipisteNimi", toimipisteId);
            ViewBag.Aktiiviset = new SelectList(new[] { new { Value = true, Text = "Aktiivinen" }, new { Value = false, Text = "Ei aktiivinen" } }, "Value", "Text", onAktiivinen);
            ViewBag.Statukset = new SelectList(await _context.Statukset.ToListAsync(), "StatusId", "StatusNimi", statusId);

            ViewData["PageSize"] = pageSize;
            ViewData["CurrentPage"] = currentPage;
            ViewData["TotalPages"] = totalPages;

            return View();
        }

        //ESITTELE TÄMÄ NÄYTÖSSÄ v
        [HttpGet]
        public async Task<IActionResult> GetTuotteetPartial(int pageSize = 10, int currentPage = 0, string? kuvausHakusanalla = null, int? kategoriaId = null, bool? onAktiivinen = null, int? toimipisteId = null, int? statusId = null,string? sortOrder = null)
        {
            var tuotteet = _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat).ThenInclude(t => t.Status)
                .Include(t => t.Tapahtumat).ThenInclude(t => t.Käyttäjä)
                .AsQueryable();
            var tuotteetCount = await tuotteet.CountAsync();

            if (!string.IsNullOrEmpty(kuvausHakusanalla))
                tuotteet = tuotteet.Where(t => t.Kuvaus.Contains(kuvausHakusanalla));
            if (kategoriaId.HasValue)
                tuotteet = tuotteet.Where(t => t.KategoriaId == kategoriaId.Value);
            if (onAktiivinen.HasValue)
                tuotteet = tuotteet.Where(t => t.Aktiivinen == onAktiivinen.Value);
            if (toimipisteId.HasValue)
                tuotteet = tuotteet.Where(t => t.ToimipisteId == toimipisteId.Value);
            if (statusId.HasValue)
                tuotteet = tuotteet.Where(t => t.Tapahtumat.OrderByDescending(t=>t.AloitusPvm).FirstOrDefault().StatusId == statusId.Value); //ota viimeisimmän tapahtuman status

            // Sorting
            tuotteet = sortOrder switch
            {
                "Status" => tuotteet
                    .Select(t => new
                    {
                        Tuote = t,
                        LatestStatusName = t.Tapahtumat
                            .OrderByDescending(x => x.AloitusPvm)
                            .Select(x => x.Status.StatusNimi)
                            .FirstOrDefault()
                    })
                    .OrderBy(x => x.LatestStatusName)
                    .Select(x => x.Tuote),
                "Status_desc" => tuotteet
                    .Select(t => new
                    {
                        Tuote = t,
                        LatestStatusName = t.Tapahtumat
                            .OrderByDescending(x => x.AloitusPvm)
                            .Select(x => x.Status.StatusNimi)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(x => x.LatestStatusName)
                    .Select(x => x.Tuote),

                "Hinta" => tuotteet.OrderBy(t => t.Hinta),
                "Hinta_desc" => tuotteet.OrderByDescending(t => t.Hinta),

                "Kuvaus" => tuotteet.OrderBy(t => t.Kuvaus),
                "Kuvaus_desc" => tuotteet.OrderByDescending(t => t.Kuvaus),

                "Ostopvm" => tuotteet.OrderBy(t => t.OstoPvm), 
                "Ostopvm_desc" => tuotteet.OrderByDescending(t => t.OstoPvm),

                "Takuu" => tuotteet.OrderBy(t => t.Takuu),
                "Takuu_desc" => tuotteet.OrderByDescending(t => t.Takuu),

                "Kategoria" => tuotteet.OrderBy(t => t.Kategoria.KategoriaNimi),
                "Kategoria_desc" => tuotteet.OrderByDescending(t => t.Kategoria.KategoriaNimi),

                "Toimipiste" => tuotteet.OrderBy(t => t.Toimipiste.ToimipisteNimi),
                "Toimipiste_desc" => tuotteet.OrderByDescending(t => t.Toimipiste.ToimipisteNimi),

                "TunnisteNro" => tuotteet.OrderBy(t => t.IdNumero),
                "TunnisteNro_desc" => tuotteet.OrderByDescending(t => t.IdNumero),

                "Aktiivinen" => tuotteet.OrderBy(t => t.Aktiivinen),
                "Aktiivinen_desc" => tuotteet.OrderByDescending(t => t.Aktiivinen),

                "TuoteId" => tuotteet.OrderBy(t => t.TuoteId),
                "TuoteId_desc" => tuotteet.OrderByDescending(t => t.TuoteId),
                
                "StatusId" => tuotteet.OrderBy(t => t.Tapahtumat.OrderByDescending(t => t.AloitusPvm).FirstOrDefault().StatusId),
                "StatusId_desc" => tuotteet.OrderByDescending(t => t.Tapahtumat.OrderByDescending(t => t.AloitusPvm).FirstOrDefault().StatusId),

                _ => tuotteet.OrderByDescending(t => t.TuoteId)
            };

            var totalFiltered = await tuotteet.CountAsync();

            var tuotteetPaged = await tuotteet
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Kaikki"] = tuotteetCount; 
            ViewData["Suodatetut"] = totalFiltered;

            Response.Headers["X-TotalPages"] = Math.Ceiling((double)totalFiltered / pageSize).ToString();

            return PartialView("_TuoteRivitPartial", tuotteetPaged);
        }
        public async Task<IActionResult> _StatusPartial(int statusId)
        {
            var status = await _context.Statukset.FirstOrDefaultAsync(s => s.StatusId == statusId);
            return PartialView("_StatusPartial", status);
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
                    .ThenInclude(t => t.Status)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(t => t.Käyttäjä)
                .FirstOrDefaultAsync(m => m.TuoteId == id);
            if (tuote == null)
            {
                return NotFound();
            }
            //ESITTELE RETURN FUNKITIO
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

            return PartialView("_TuoteKorttiPartial", tuote);
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
        public async Task<IActionResult> Create(Tuote tuote)
        {
            if (ModelState.IsValid)
            {
                if (tuote.KuvaFile != null)
                {
                    using (var inputStream = tuote.KuvaFile.OpenReadStream())
                    using (var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream))
                    {
                        // Resize image
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new SixLabors.ImageSharp.Size(256, 256),
                            Mode = ResizeMode.Max
                        }));

                        // compressoi ja tallentaa memory streamiin
                        using (var outputStream = new MemoryStream())
                        {
                            var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                            {
                                Quality = 50 // Pienentää laatua
                            };

                            await image.SaveAsJpegAsync(outputStream, encoder);
                            tuote.Kuva = outputStream.ToArray();
                        }
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

            ViewData["KategoriaNimi"] = new SelectList(_context.Kategoriat, "KategoriaId", "KategoriaNimi", tuote.KategoriaId);
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
            var tapahtumat = await _context.Tapahtumat.Include(t => t.Käyttäjä).Include(t => t.Status).Include(t => t.Tuote).ThenInclude(tu => tu.Toimipiste).OrderBy(t => t.AloitusPvm).ToListAsync();

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

        [HttpGet]
        public async Task<IActionResult> GetCompressedImage(int id)
        {
            var tuote = await _context.Tuotteet.FindAsync(id);
            if (tuote == null || tuote.Kuva == null)
                return NotFound();

            using var originalStream = new MemoryStream(tuote.Kuva);
            using var image = await Image.LoadAsync(originalStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(256, 256), // Näyttökoon rajoitus
                Mode = ResizeMode.Max
            }));

            var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 50 });
            ms.Seek(0, SeekOrigin.Begin);

            return File(ms.ToArray(), "image/jpeg");
        }

        public async Task<IActionResult> Huollossatuotteet()
        {
            var tuotteet = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Käyttäjä)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Status)
                .Where(t => t.Tapahtumat
                    .OrderByDescending(tap => tap.AloitusPvm)
                    .Select(tap => tap.Status.StatusNimi)
                    .FirstOrDefault() == "Huollossa")
                .ToListAsync();
            // YLLÄ OLEVA FILTTERÖINTI TOIMII. EI TARVITSE FILTTERÖIDÄ UUDELLEEN NÄKYMÄSSÄ JA VOI TEHDÄ YHDEN GENEERISEN SIVUN MISSÄ NÄKYY TUOTTET LISTASSA SITTEN KUTSUU return view("Sivunimi",tuotteet) NIIN PITÄISI TOIMIA NÄIHIN KAIKKIIN EIKÄ TARVITA KOLMEA ERI SIVUA.

            return View("TuoteWrapper", tuotteet);
        }

        public async Task<IActionResult> Varatuttuotteet()
        {
            var tuotteet = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Käyttäjä)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Status)
                .Where(t => t.Tapahtumat
                    .OrderByDescending(tap => tap.AloitusPvm)
                    .Select(tap => tap.Status.StatusNimi)
                    .FirstOrDefault() == "Varattu")
                .ToListAsync();
            // YLLÄ OLEVA FILTTERÖINTI TOIMII. EI TARVITSE FILTTERÖIDÄ UUDELLEEN NÄKYMÄSSÄ JA VOI TEHDÄ YHDEN GENEERISEN SIVUN MISSÄ NÄKYY TUOTTET LISTASSA SITTEN KUTSUU return view("Sivunimi",tuotteet) NIIN PITÄISI TOIMIA NÄIHIN KAIKKIIN EIKÄ TARVITA KOLMEA ERI SIVUA.

            return View("TuoteWrapper", tuotteet);
        }
        public async Task<IActionResult> Vapaanatuotteet()
        {
            var tuotteet = await _context.Tuotteet
                .Include(t => t.Kategoria)
                .Include(t => t.Toimipiste)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Käyttäjä)
                .Include(t => t.Tapahtumat)
                    .ThenInclude(tap => tap.Status)
                .Where(t =>
                    !t.Tapahtumat.Any() ||
                    t.Tapahtumat
                        .OrderByDescending(tap => tap.AloitusPvm)
                        .Select(tap => tap.Status.StatusNimi)
                        .FirstOrDefault() == "Vapaa"
                )
                .ToListAsync();

            return View("TuoteWrapper", tuotteet);
        }


    }
}
