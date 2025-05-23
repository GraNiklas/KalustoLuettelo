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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http;
using System.Drawing.Printing;
using KalustoLuetteloSovellus.ViewModels;

namespace KalustoLuetteloSovellus.Controllers;

public class HomeController : Controller
{
    private readonly KaluDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;


    public HomeController(ILogger<HomeController> logger, KaluDbContext context, IEmailService emailService, IWebHostEnvironment env)
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _env = env;
    }

    public async Task<IActionResult> About()
    {
        return View(); // Näytä About-näkymä
    }
    public async Task<IActionResult> Index()
    {
        if (_context.Käyttäjät.Any() && HttpContext.User.Identity.IsAuthenticated)
        {
            ViewData["Statukset"] = await _context.Statukset.ToListAsync(); // lisää statukset index sivulle piirakkakaaviolle
            // Extract claims from the cookie
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            var userNameClaim = HttpContext.User.FindFirst("UserName")?.Value;
            var userRoleClaim = HttpContext.User.FindFirst("Role")?.Value;



            if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(userNameClaim) && !string.IsNullOrEmpty(userRoleClaim))
            {
                // Only set session variables if the claims are valid
                HttpContext.Session.SetString("Käyttäjätunnus", userNameClaim);
                HttpContext.Session.SetInt32("KäyttäjäId", int.Parse(userIdClaim));
                HttpContext.Session.SetString("Rooli", userRoleClaim); 
            }
            else
            {
                // Handle the case when any of the claims are missing or invalid
                ViewBag.ErrorMessage = "Failed to retrieve user data from authentication.";
                return RedirectToAction("Login");  // Redirect to login or show an error page
            }

            // Continue with logic for logged-in users
        }
        else
        {
            // User is not logged in
            return RedirectToAction("Login");
        }
        // Tämä ottaa nyt vaa 10 ensimmäistä tuotetta tietokannasta, voidaan keksiä joku juttu missä se hakee tapahtumista viimeisimmät 10 tapahtumaa ja listaa ne tuotteet, en tiedä.
        //var t = _context.Tapahtumat.OrderByDescending(t=>t.AloitusPvm).Take(10);  // tämä on nyt vain esimerkki viimeisimmistä tapahtumista

        var määrä = 20;
        var viimeisimmätTuotteet = await _context.Tuotteet
            .Include(t => t.Kategoria)
            .Include(t => t.Toimipiste)
            .Include(t => t.Tapahtumat)
                .ThenInclude(t => t.Status)
            .OrderByDescending(t => t.TuoteId) 
            .Take(määrä)
            .ToListAsync();

        return View(viimeisimmätTuotteet);
    }

    public async Task<IActionResult> User()
    {
        var käyttäjäId = HttpContext.Session.GetInt32("KäyttäjäId");
        var käyttäjä = await _context.Käyttäjät
            .Include(k => k.Rooli)
            .Include(k => k.Tapahtumat)
                .ThenInclude(k => k.Status)
            .Include(k => k.Tapahtumat)
                .ThenInclude(t => t.Tuote)
                .ThenInclude(t => t.Toimipiste)
            .FirstOrDefaultAsync(k => k.KäyttäjäId == käyttäjäId);
        return View(käyttäjä);
    }
    [HttpGet]
    public async Task<IActionResult> DeleteAccount(int käyttäjäId)
    {
        var poistettavaKäyttäjä = await _context.Käyttäjät.FindAsync(käyttäjäId);
        if (poistettavaKäyttäjä != null)
        {

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            _context.Käyttäjät.Remove(poistettavaKäyttäjä);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index");

        }
        else
        {
            return View("User");
        }
    }
    public IActionResult AccessDenied()
    {
        return View();
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
            var adminRooli = await _context.Roolit.FirstOrDefaultAsync(r => r.RooliNimi == "Admin");
            var userRooli = await _context.Roolit.FirstOrDefaultAsync(r => r.RooliNimi == "User");

            if (userRooli == null || adminRooli == null)
            {
                Console.WriteLine("Roolit ei löydy tietokannasta, mahdollinen ongelma DbInitializer luokassa");
                ViewBag.ErrorMessage = "Rooleja ei löydy. Ota yhteys järjestelmänvalvojaan.";
                return View("Error");
            }
            

            if(_context.Käyttäjät.Count() == 0)
            {
                käyttäjä.RooliId = adminRooli.RooliId;
            }
            else
            {
                käyttäjä.RooliId = userRooli.RooliId;
            }


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



            // tämä voi lähetttää kokonaisen nettisivun
            //WebFetcher webFetcher = new WebFetcher(_httpClient);
            //var emailContent = await webFetcher.GetPageHtmlAsync("https://www.bbc.com/news");

            //TODO
            
            var path = Path.Combine(_env.WebRootPath, "emailpohjat", "TervetuloaEmail.html");

                string emailContent = await System.IO.File.ReadAllTextAsync(path);

                await _emailService.SendEmailAsync(käyttäjä.Käyttäjätunnus, "Rekisteröityminen", emailContent);

                await _context.Käyttäjät.AddAsync(käyttäjä);

                await _context.SaveChangesAsync();

                await AuthorizeUser(käyttäjä); // tämä autorisoi käyttäjän heti rekisteröinnin jälkeen

                return RedirectToAction("Index", "Home"); // mihin mennään kun rekisteröinti onnistuu
            }
        }
        else
        {
            ViewBag.ErrorMessage = "Käyttäjätunnus pitää olla muodossa - email@student.careeria.fi TAI email@careeria.fi";
        }
        return View();
    }
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Authorize(Käyttäjä käyttäjä)
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
            ViewBag.ErrorMessage = "Tyhjä salasana kenttä.";
            return View("Login");
        }


        var result = hasher.VerifyHashedPassword(käyttäjä, loggedUser.Salasana, käyttäjä.Salasana); // tämä varmistaa että salasana on oikein

        if (result == PasswordVerificationResult.Success) // tarkistaa onko salasana verifikaatio success
        {

            if (loggedUser != null)
            {
                return await AuthorizeUser(loggedUser);
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
        else
        {
            ViewBag.ErrorMessage = "Väärä salasana";
        }
        return View("Login");

    }

    private async Task<IActionResult> AuthorizeUser(Käyttäjä loggedUser)
    {
        ViewBag.LoginMessage = "Successful login";
        ViewBag.LoggedStatus = "In";
        ViewBag.LoginError = 0;
        HttpContext.Session.SetString("Käyttäjätunnus", loggedUser.Käyttäjätunnus);
        HttpContext.Session.SetInt32("KäyttäjäId", loggedUser.KäyttäjäId);
        var käyttäjäRooli = await _context.Roolit.FirstOrDefaultAsync(r => r.RooliId == loggedUser.RooliId);
        if (käyttäjäRooli == null)
        {
            ViewBag.ErrorMessage = "Roolia ei löydy";
            return View("Login");
        }
        HttpContext.Session.SetString("Rooli", käyttäjäRooli.RooliNimi); // toivottavasti rooli ei ole null tässä.
        

        var claims = new List<Claim>
                {
                    new Claim("UserName", loggedUser.Käyttäjätunnus),
                    new Claim("UserId", loggedUser.KäyttäjäId.ToString()),
                    new Claim("Role", käyttäjäRooli.RooliNimi)
                    // add roles/permissions here if needed
                };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true, // <-- this makes it persist
            ExpiresUtc = DateTime.UtcNow.AddDays(14) // optional expiry
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        return RedirectToAction("Index", "Home"); // mihin mennään kun login onnistuu
    }
    public IActionResult Email()
    {
        var filePath = Path.Combine(_env.WebRootPath, "emailpohjat", "TervetuloaEmail.html");
        return PhysicalFile(filePath, "text/html");
    }
    [HttpGet]
    public IActionResult SalasananVaihto(int käyttäjäId)
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SalasananVaihto(SalasananVaihtoViewModel salasananVaihtoViewModel)
    {
        var kirjautunutKäyttäjä = _context.Käyttäjät.Include(k => k.Rooli) // hae käyttäjä ja include kaikki navigointi data
            .Include(k => k.Tapahtumat)
                .ThenInclude(k => k.Status)
            .Include(k => k.Tapahtumat)
                .ThenInclude(t => t.Tuote)
                .ThenInclude(t => t.Toimipiste)
            .FirstOrDefault(k => k.KäyttäjäId == HttpContext.Session.GetInt32("KäyttäjäId")); // tämä hakee kirjautuneen käyttäjän id:n sessiosta


        if (kirjautunutKäyttäjä == null)
        {
            salasananVaihtoViewModel.Virheviesti = "Käyttäjää ei löydy.";
            return View("index");
        }


        // salasanan vaihto jutut
        // Authorize
        var hasher = new PasswordHasher<Käyttäjä>();
        if (string.IsNullOrEmpty(kirjautunutKäyttäjä.Salasana))
        {
            ViewBag.ErrorMessage = "Tyhjä salasana kenttä.";
            return View("Login");
        }
        var hashattuVanhaSalasana = hasher.HashPassword(kirjautunutKäyttäjä,salasananVaihtoViewModel.VanhaSalasana); // hashaa tämä ensin että voi verrata tietokantaan

        var result = hasher.VerifyHashedPassword(kirjautunutKäyttäjä, kirjautunutKäyttäjä.Salasana, salasananVaihtoViewModel.VanhaSalasana); // tämä varmistaa että vanha salasana on oikein

        if (result == PasswordVerificationResult.Success) // tarkistaa onko salasana verifikaatio success
        {
            // jos on ok hashaa uusi salasana ja tallennna


            kirjautunutKäyttäjä.Salasana = hasher.HashPassword(kirjautunutKäyttäjä, salasananVaihtoViewModel.UusiSalasana); // tämä hashaa uuden salasanan
            
            _context.SaveChanges(); // tallennetaan muutokset tietokantaan

            ViewBag.SuccessMessage = "Salasana vaihdettu onnistuneesti!";
            return View("user", kirjautunutKäyttäjä);
        }
        else
        {
                
            ViewBag.ErrorMessage = "Väärä salasana!";
            return View("user", kirjautunutKäyttäjä);
        }


    }
}






