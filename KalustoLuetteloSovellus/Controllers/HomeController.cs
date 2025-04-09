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

    public IActionResult Index()
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
    public async Task<ActionResult> Register(K�ytt�j� k�ytt�j�)
    {

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

                await _context.K�ytt�j�t.AddAsync(k�ytt�j�);
                await _context.SaveChangesAsync();
                
                return RedirectToAction("Index");

                //return RedirectToAction("Authorize",k�ytt�j�); // koitin autorisoida, registerin j�lkeen mutta t�m� ei toiminnut 
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
    public ActionResult Authorize(K�ytt�j� k�ytt�j�)
    {

        var loggedUser = _context.K�ytt�j�t.SingleOrDefault(x => x.K�ytt�j�tunnus == k�ytt�j�.K�ytt�j�tunnus);
        if (loggedUser == null) 
        {
            return View();  // palataan jos ei l�ydy k�ytt�j�� k�ytt�j�tunnuksella.
        }

        var hasher = new PasswordHasher<K�ytt�j�>();

        var result = hasher.VerifyHashedPassword(k�ytt�j�, loggedUser.Salasana, k�ytt�j�.Salasana); // t�m� varmistaa ett� salasana on oikein

        if (result == PasswordVerificationResult.Success) // tarkistaa onko salasana verifikaatio success
        {

            if (loggedUser != null)
            {
                ViewBag.LoginMessage = "Successful login";
                ViewBag.LoggedStatus = "In";
                ViewBag.LoginError = 0;
                HttpContext.Session.SetString("K�ytt�j�tunnus", loggedUser.K�ytt�j�tunnus);
                HttpContext.Session.SetInt32("RooliId", loggedUser.RooliId);
                return RedirectToAction("Index", "Home"); // mihin menn��n kun login onnistuu
            }
            else
            {
                ViewBag.LoginMessage = "Login unsuccessful";
                ViewBag.LoggedStatus = "Out";
                ViewBag.LoginError = 1;
                //k�ytt�j�.LoginErrorMessage = "Tuntematon k�ytt�j�tunnus tai salasana";
                return View("Index", k�ytt�j�);
            }
        }
        return View("Index", k�ytt�j�);

    }
}
