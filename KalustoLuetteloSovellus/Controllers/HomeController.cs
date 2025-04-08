using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KalustoLuetteloSovellus.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;




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
    public async Task<ActionResult> Login()
    {

        return View();

    }
    [HttpGet]
    public async Task<ActionResult> Register()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Register(Käyttäjä käyttäjä)
    {

        if ((käyttäjä.Käyttäjätunnus.Contains("@student.careeria.fi") ||
            käyttäjä.Käyttäjätunnus.Contains("@careeria.fi") &&
            käyttäjä.Käyttäjätunnus != "@student.careeria.fi" &&
            käyttäjä.Salasana != ""
        ))
        {
            käyttäjä.RooliId = 40001;
            var olemassaolevaKäyttäjä = await _context.Käyttäjät.FirstOrDefaultAsync(k => k.Käyttäjätunnus == käyttäjä.Käyttäjätunnus);
            if (olemassaolevaKäyttäjä != null)
            {
                // käyttäjätunnus on käytössä  MITEN ME KERROTAAN KÄYTTÄJÄLLE ETTÄ KÄYTTÄJÄTUNNUS ON KÄYTÖSSÄ
                return View();
            }
            else
            {
                _context.Käyttäjät.Add(käyttäjä);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        return View();
    }
    public async Task<ActionResult> Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }


    [HttpPost]
    public ActionResult Authorize(Käyttäjä käyttäjä)
    {
        
        var crpwd = "";
        //var salt = Hmac.GenerateSalt();
        //var hmac1 = Hmac.ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(käyttäjä.Salasana), salt);

        var loggedUser = _context.Käyttäjät.SingleOrDefault(x => x.Käyttäjätunnus == käyttäjä.Käyttäjätunnus && x.Salasana == käyttäjä.Salasana);
        if (loggedUser != null)
        {
            ViewBag.LoginMessage = "Succesfull login";
            ViewBag.LoggedStatus = "In";
            ViewBag.LoginError = 0;
            HttpContext.Session.SetString("LoggedUser", loggedUser.Käyttäjätunnus);
            //Session["LoggedMessage"] = "Logged in as " + loggedUser.Käyttäjätunnus;
            //Session["UserName"] = loggedUser.Käyttäjätunnus;
            return RedirectToAction("Index", "Home"); // mihin mennään kun login onnistuu
        }
        else
        {
            ViewBag.LoginMessage = "Login unsuccesfull";
            ViewBag.LoggedStatus = "Out";
            ViewBag.LoginError = 1;
            //käyttäjä.LoginErrorMessage = "Tuntematon käyttäjätunnus tai salasana";
            return View("Index", käyttäjä);
        }
    }
}
