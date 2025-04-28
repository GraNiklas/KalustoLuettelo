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
                HttpContext.Session.SetString("K�ytt�j�tunnus", userNameClaim);
                HttpContext.Session.SetInt32("K�ytt�j�Id", int.Parse(userIdClaim));
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
        // T�m� ottaa nyt vaa 10 ensimm�ist� tuotetta tietokannasta, voidaan keksi� joku juttu miss� se hakee tapahtumista viimeisimm�t 10 tapahtumaa ja listaa ne tuotteet, en tied�.
        //var t = _context.Tapahtumat.OrderByDescending(t=>t.AloitusPvm).Take(10);  // t�m� on nyt vain esimerkki viimeisimmist� tapahtumista
        

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
        var k�ytt�j�Id = HttpContext.Session.GetInt32("K�ytt�j�Id");
        var k�ytt�j� = await _context.K�ytt�j�t
            .Include(k => k.Rooli)
            .Include(k => k.Tapahtumat)
                .ThenInclude(k => k.Status)
            .Include(k => k.Tapahtumat)
                .ThenInclude(t => t.Tuote)
                .ThenInclude(t => t.Toimipiste)
            .FirstOrDefaultAsync(k => k.K�ytt�j�Id == k�ytt�j�Id);
        return View(k�ytt�j�);
    }
    [HttpGet]
    public async Task<IActionResult> DeleteAccount(int k�ytt�j�Id)
    {
        var poistettavaK�ytt�j� = await _context.K�ytt�j�t.FindAsync(k�ytt�j�Id);
        if (poistettavaK�ytt�j� != null)
        {

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            _context.K�ytt�j�t.Remove(poistettavaK�ytt�j�);
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
    public async Task<ActionResult> Register(K�ytt�j� k�ytt�j�)
    {
        //if (!ModelState.IsValid)
        //{
        //    return View(k�ytt�j�);
        //}

        if ((k�ytt�j�.K�ytt�j�tunnus.Contains("@student.careeria.fi") ||
             k�ytt�j�.K�ytt�j�tunnus.Contains("@careeria.fi")) &&
            k�ytt�j�.K�ytt�j�tunnus != "@student.careeria.fi" &&
            k�ytt�j�.Salasana != "")
        {
            k�ytt�j�.RooliId = 40001;
            var olemassaolevaK�ytt�j� = await _context.K�ytt�j�t.FirstOrDefaultAsync(k => k.K�ytt�j�tunnus == k�ytt�j�.K�ytt�j�tunnus);
            if (olemassaolevaK�ytt�j� != null)
            {
                // k�ytt�j�tunnus on k�yt�ss�  MITEN ME KERROTAAN K�YTT�J�LLE ETT� K�YTT�J�TUNNUS ON K�YT�SS�
                ViewBag.ErrorMessage = "K�ytt�j�tunnus k�yt�ss�";
                return View();
            }
            else
            {
                var hasher = new PasswordHasher<K�ytt�j�>();

                k�ytt�j�.Salasana = hasher.HashPassword(k�ytt�j�, k�ytt�j�.Salasana); // t�m� hashaa salasanan

                await _emailService.SendEmailAsync(k�ytt�j�.K�ytt�j�tunnus, "Hello", "This is a test email");

                await _context.K�ytt�j�t.AddAsync(k�ytt�j�);
                await _context.SaveChangesAsync();

                await AuthorizeUser(k�ytt�j�); // t�m� autorisoi k�ytt�j�n heti rekister�innin j�lkeen

                return RedirectToAction("Index", "Home"); // mihin menn��n kun rekister�inti onnistuu
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
    public async Task<IActionResult> Authorize(K�ytt�j� k�ytt�j�)
    {

        var loggedUser = _context.K�ytt�j�t.SingleOrDefault(x => x.K�ytt�j�tunnus == k�ytt�j�.K�ytt�j�tunnus);
        if (loggedUser == null)
        {
            ViewBag.ErrorMessage = "Tuntematon k�ytt�j�tunnus";
            return View("Login");
        } 

        var hasher = new PasswordHasher<K�ytt�j�>();
        if (string.IsNullOrEmpty(k�ytt�j�.Salasana))
        {
            ViewBag.ErrorMessage = "Tyhj� salasana, arvaa mit� sun pit�� t�ytt�� se...";
            return View("Login");
        }


        var result = hasher.VerifyHashedPassword(k�ytt�j�, loggedUser.Salasana, k�ytt�j�.Salasana); // t�m� varmistaa ett� salasana on oikein

        if (result == PasswordVerificationResult.Success) // tarkistaa onko salasana verifikaatio success
        {

            if (loggedUser != null)
            {
                return await AuthorizeUser(loggedUser);
            }
            else
            {
                ViewBag.LoginMessage = "V��r� salasana";
                ViewBag.LoggedStatus = "Out";
                ViewBag.LoginError = 1;
                //k�ytt�j�.LoginErrorMessage = "Tuntematon k�ytt�j�tunnus tai salasana";
                return View("Login");
            }
        }
        else
        {
            ViewBag.ErrorMessage = "V��r� salasana";
        }
        return View("Login");

    }

    private async Task<IActionResult> AuthorizeUser(K�ytt�j� loggedUser)
    {
        ViewBag.LoginMessage = "Successful login";
        ViewBag.LoggedStatus = "In";
        ViewBag.LoginError = 0;
        HttpContext.Session.SetString("K�ytt�j�tunnus", loggedUser.K�ytt�j�tunnus);
        HttpContext.Session.SetInt32("K�ytt�j�Id", loggedUser.K�ytt�j�Id);
        HttpContext.Session.SetInt32("RooliId", loggedUser.RooliId);


        var claims = new List<Claim>
                {
                    new Claim("UserName", loggedUser.K�ytt�j�tunnus),
                    new Claim("UserId", loggedUser.K�ytt�j�Id.ToString()),
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

        return RedirectToAction("Index", "Home"); // mihin menn��n kun login onnistuu
    }
}
