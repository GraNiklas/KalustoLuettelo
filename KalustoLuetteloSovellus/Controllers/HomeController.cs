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
    public async Task<ActionResult> Register(K�ytt�j� k�ytt�j�)
    {

        if ((k�ytt�j�.K�ytt�j�tunnus.Contains("@student.careeria.fi") ||
            k�ytt�j�.K�ytt�j�tunnus.Contains("@careeria.fi") &&
            k�ytt�j�.K�ytt�j�tunnus != "@student.careeria.fi" &&
            k�ytt�j�.Salasana != ""
        ))
        {
            k�ytt�j�.RooliId = 40001;
            var olemassaolevaK�ytt�j� = await _context.K�ytt�j�t.FirstOrDefaultAsync(k => k.K�ytt�j�tunnus == k�ytt�j�.K�ytt�j�tunnus);
            if (olemassaolevaK�ytt�j� != null)
            {
                // k�ytt�j�tunnus on k�yt�ss�  MITEN ME KERROTAAN K�YTT�J�LLE ETT� K�YTT�J�TUNNUS ON K�YT�SS�
                return View();
            }
            else
            {
                _context.K�ytt�j�t.Add(k�ytt�j�);
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
    public ActionResult Authorize(K�ytt�j� k�ytt�j�)
    {
        
        var crpwd = "";
        //var salt = Hmac.GenerateSalt();
        //var hmac1 = Hmac.ComputeHMAC_SHA256(Encoding.UTF8.GetBytes(k�ytt�j�.Salasana), salt);

        var loggedUser = _context.K�ytt�j�t.SingleOrDefault(x => x.K�ytt�j�tunnus == k�ytt�j�.K�ytt�j�tunnus && x.Salasana == k�ytt�j�.Salasana);
        if (loggedUser != null)
        {
            ViewBag.LoginMessage = "Succesfull login";
            ViewBag.LoggedStatus = "In";
            ViewBag.LoginError = 0;
            HttpContext.Session.SetString("LoggedUser", loggedUser.K�ytt�j�tunnus);
            //Session["LoggedMessage"] = "Logged in as " + loggedUser.K�ytt�j�tunnus;
            //Session["UserName"] = loggedUser.K�ytt�j�tunnus;
            return RedirectToAction("Index", "Home"); // mihin menn��n kun login onnistuu
        }
        else
        {
            ViewBag.LoginMessage = "Login unsuccesfull";
            ViewBag.LoggedStatus = "Out";
            ViewBag.LoginError = 1;
            //k�ytt�j�.LoginErrorMessage = "Tuntematon k�ytt�j�tunnus tai salasana";
            return View("Index", k�ytt�j�);
        }
    }
}
