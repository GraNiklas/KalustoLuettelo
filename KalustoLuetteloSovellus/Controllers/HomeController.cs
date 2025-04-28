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

namespace KalustoLuetteloSovellus.Controllers;

public class HomeController : Controller
{
    private readonly KaluDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, KaluDbContext context, IEmailService emailService)
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.User.Identity.IsAuthenticated)
        {
            // Extract claims from the cookie
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            var userNameClaim = HttpContext.User.FindFirst("UserName")?.Value;
            var userRoleClaim = HttpContext.User.FindFirst("Role")?.Value;


            if (!string.IsNullOrEmpty(userIdClaim) && !string.IsNullOrEmpty(userNameClaim) && !string.IsNullOrEmpty(userRoleClaim))
            {
                // Only set session variables if the claims are valid
                HttpContext.Session.SetString("Käyttäjätunnus", userNameClaim);
                HttpContext.Session.SetInt32("KäyttäjäId", int.Parse(userIdClaim));
                HttpContext.Session.SetInt32("RooliId", int.Parse(userRoleClaim));
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
        

        var tuotteet = await _context.Tuotteet
            .Include(t => t.Kategoria)
            .Include(t => t.Toimipiste)
            .Include(t => t.Tapahtumat)
                .ThenInclude(t => t.Status)
            .ToListAsync();

            var viimeiset10 = tuotteet
            .AsEnumerable()
            .Reverse()
            .Take(10)
            .ToList();

        return View(viimeiset10);
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

                await _emailService.SendEmailAsync(käyttäjä.Käyttäjätunnus, "Hello", "This is a test email");

                await _context.Käyttäjät.AddAsync(käyttäjä);
                await _context.SaveChangesAsync();

                await AuthorizeUser(käyttäjä); // tämä autorisoi käyttäjän heti rekisteröinnin jälkeen

                return RedirectToAction("Index", "Home"); // mihin mennään kun rekisteröinti onnistuu
            }
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
            ViewBag.ErrorMessage = "Tyhjä salasana, arvaa mitä sun pitää täyttää se...";
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
        HttpContext.Session.SetInt32("RooliId", loggedUser.RooliId);


        var claims = new List<Claim>
                {
                    new Claim("UserName", loggedUser.Käyttäjätunnus),
                    new Claim("UserId", loggedUser.KäyttäjäId.ToString()),
                    new Claim("Role", loggedUser.RooliId.ToString())
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
}
