using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KalustoLuetteloSovellus.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;

namespace KalustoLuetteloSovellus.Controllers;

public class HomeController : Controller
{
    private readonly KaluDbContext _context;
    private readonly ILogger<HomeController> _logger;


    public HomeController(ILogger<HomeController> logger, KaluDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Tämä ottaa nyt vaa 10 ensimmäistä tuotetta tietokannasta, voidaan keksiä joku juttu missä se hakee tapahtumista viimeisimmät 10 tapahtumaa ja listaa ne tuotteet, en tiedä.
        //var t = _context.Tapahtumat.OrderByDescending(t=>t.AloitusPvm).Take(10);  // tämä on nyt vain esimerkki viimeisimmistä tapahtumista


        var kaluDbContext = _context.Tuotteet.Include(t => t.Kategoria).Include(t => t.Toimipiste).Take(10); 
        return View(await kaluDbContext.ToListAsync());
    }

    public async Task<IActionResult> User()
    {
        var käyttäjäId = HttpContext.Session.GetInt32("KäyttäjäId");
        var käyttäjä = await _context.Käyttäjät.Include(k => k.Tapahtumat).ThenInclude(k => k.Status).Include(k => k.Tapahtumat).ThenInclude(t => t.Tuote).ThenInclude(t => t.Toimipiste).FirstOrDefaultAsync(k => k.KäyttäjäId == käyttäjäId);
        return View(käyttäjä);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public ActionResult Login()
    {
        return View();
    }
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(Käyttäjä käyttäjä)
    {
        //if (!ModelState.IsValid)
        //{
        //    return View(käyttäjä);
        //}

        if ((käyttäjä.Käyttäjätunnus.Contains("@student.careeria.fi") ||
             käyttäjä.Käyttäjätunnus.Contains("@careeria.fi")) &&
            käyttäjä.Käyttäjätunnus != "@student.careeria.fi" &&
            käyttäjä.Salasana != "")
        {
            käyttäjä.RooliId = 40001;
            var olemassaolevaKäyttäjä = await _context.Käyttäjät.FirstOrDefaultAsync(k => k.Käyttäjätunnus == käyttäjä.Käyttäjätunnus);
            if (olemassaolevaKäyttäjä != null)
            {
                // käyttäjätunnus on käytössä  MITEN ME KERROTAAN KÄYTTÄJÄLLE ETTÄ KÄYTTÄJÄTUNNUS ON KÄYTÖSSÄ
                ViewBag.ErrorMessage = "Käyttäjätunnus käytössä";
                return View();
            }
            else
            {
                var hasher = new PasswordHasher<Käyttäjä>();

                käyttäjä.Salasana = hasher.HashPassword(käyttäjä, käyttäjä.Salasana); // tämä hashaa salasanan

                await _context.Käyttäjät.AddAsync(käyttäjä);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Login");

                //return RedirectToAction("Authorize",käyttäjä); // koitin autorisoida, registerin jälkeen mutta tämä ei toiminnut 
            }
        }
        return View();
    }
    public ActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }


    [HttpPost]
    public ActionResult Authorize(Käyttäjä käyttäjä)
    {

        var loggedUser = _context.Käyttäjät.SingleOrDefault(x => x.Käyttäjätunnus == käyttäjä.Käyttäjätunnus);
        if (loggedUser == null)
        {
            ViewBag.ErrorMessage = "Tuntematon käyttäjätunnus";
            return View("Login");
        }

        var hasher = new PasswordHasher<Käyttäjä>();
        if (string.IsNullOrEmpty(käyttäjä.Salasana))
        {
            ViewBag.ErrorMessage = "Tyhjä salasana, arvaa mitä sun pitää täyttää se...";
            return View("Login");
        }


        var result = hasher.VerifyHashedPassword(käyttäjä, loggedUser.Salasana, käyttäjä.Salasana); // tämä varmistaa että salasana on oikein

        if (result == PasswordVerificationResult.Success) // tarkistaa onko salasana verifikaatio success
        {

            if (loggedUser != null)
            {
                ViewBag.LoginMessage = "Successful login";
                ViewBag.LoggedStatus = "In";
                ViewBag.LoginError = 0;
                HttpContext.Session.SetString("Käyttäjätunnus", loggedUser.Käyttäjätunnus);
                HttpContext.Session.SetInt32("KäyttäjäId", loggedUser.KäyttäjäId);
                HttpContext.Session.SetInt32("RooliId", loggedUser.RooliId);
                return RedirectToAction("Index", "Home"); // mihin mennään kun login onnistuu
            }
            else
            {
                ViewBag.LoginMessage = "Väärä salasana";
                ViewBag.LoggedStatus = "Out";
                ViewBag.LoginError = 1;
                //käyttäjä.LoginErrorMessage = "Tuntematon käyttäjätunnus tai salasana";
                return View("Login");
            }
        }
        return View("Index", käyttäjä);

    }
}
