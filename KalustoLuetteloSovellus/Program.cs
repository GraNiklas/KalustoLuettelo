using KalustoLuetteloSovellus.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;



// luo builderin joka auttaa applikaation konfiguroinnissa, Dependency injection etc.
var builder = WebApplication.CreateBuilder(args);


// T�m� hakee connection stringin appsettings.jsonista
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Lis�� dbcontextin meid�n azure sql serveriin i.e yhdist�� sovelluksen azuren databaseen
builder.Services.AddDbContext<KaluDbContext>(options => options.UseSqlServer(connectionString)); 



// Admin filtteri joka ei p��st� k�ytt�j�� semmosiin n�kymiin mihin t�m� filtteri on lis�tty
builder.Services.AddScoped<AdminOnlyFilter>(); 


// Lis�sin t�m�n paketin niin pystyy p�ivitt�m��n sivun ja n�hd� muutokset ilman ett� tarvii rebuildata koko sovellus
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// Sessionin ja cookien asetukset, session k�ytt��n tarvitaan t�m� distributed memory cache
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Tallentaa cookien kirjautuessa, jolloin k�ytt�j� voi palata takaisin sovellukseen ilman ett� tarvitsee kirjautua uudestaan sis��n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LoginPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set how long the cookie is valid
        options.SlidingExpiration = true; // Extends cookie lifetime on activity
        options.Cookie.IsEssential = true;
    });


// Konfiguroi smtp asetukset, jotta voidaan l�hett�� s�hk�postia
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Lis�� dependecy injectionissa s�hk�postipalvelun, jotta voidaan l�hett�� s�hk�postia
builder.Services.AddTransient<IEmailService, EmailService>();


// buildaa varsinaisen sovelluksen instanssin konfiguroinnin j�lkeen
var app = builder.Build();


// Luo tietokannan ja alustaa sen sovelluksen tarvitsemilla datalla, jos se ei ole viel� olemassa
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KaluDbContext>();
    DbInitializer.SeedDefaults(context);
}

// Konfiguroi sovelluksen k�ytt�m��n meid�n error sivua kun ei olla developement environmentissa
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // Parantaa turvallisuutta, kun k�ytet��n https:��
    app.UseHsts();
}

// Uudelleenohjaa HTTP-pyynn�t HTTPS:��n (suojattu yhteys)
app.UseHttpsRedirection();

// Palvelee staattisia tiedostoja (esim. CSS, JS, kuvat) wwwroot-kansiosta
app.UseStaticFiles(); 

// Ottaa k�ytt��n reitityksen, jotta pyynt� ohjataan oikealle controllerille ja actionille
app.UseRouting();

// Ottaa k�ytt��n sessionhallinnan (esimerkiksi k�ytt�j�n tilatiedot)
app.UseSession(); 

// Ottaa k�ytt��n todennuksen (k�ytt�j�n kirjautuminen)
app.UseAuthentication();

// Tarkistaa, onko k�ytt�j�ll� oikeudet suoritettaviin toimintoihin
app.UseAuthorization(); 

// M��rittelee oletusreitityksen MVC-kontrollereille
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 


// K�ynnist�� sovelluksen ja alkaa kuunnella HTTP-pyynt�j�
app.Run(); 
