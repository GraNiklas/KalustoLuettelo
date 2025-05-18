using KalustoLuetteloSovellus.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;



// luo builderin joka auttaa applikaation konfiguroinnissa, Dependency injection etc.
var builder = WebApplication.CreateBuilder(args);


// Tämä hakee connection stringin appsettings.jsonista
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Lisää dbcontextin meidän azure sql serveriin i.e yhdistää sovelluksen azuren databaseen
builder.Services.AddDbContext<KaluDbContext>(options => options.UseSqlServer(connectionString)); 



// Admin filtteri joka ei päästä käyttäjää semmosiin näkymiin mihin tämä filtteri on lisätty
builder.Services.AddScoped<AdminOnlyFilter>(); 


// Lisäsin tämän paketin niin pystyy päivittämään sivun ja nähdä muutokset ilman että tarvii rebuildata koko sovellus
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// Sessionin ja cookien asetukset, session käyttöön tarvitaan tämä distributed memory cache
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Tallentaa cookien kirjautuessa, jolloin käyttäjä voi palata takaisin sovellukseen ilman että tarvitsee kirjautua uudestaan sisään
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LoginPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set how long the cookie is valid
        options.SlidingExpiration = true; // Extends cookie lifetime on activity
        options.Cookie.IsEssential = true;
    });


// Konfiguroi smtp asetukset, jotta voidaan lähettää sähköpostia
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Lisää dependecy injectionissa sähköpostipalvelun, jotta voidaan lähettää sähköpostia
builder.Services.AddTransient<IEmailService, EmailService>();


// buildaa varsinaisen sovelluksen instanssin konfiguroinnin jälkeen
var app = builder.Build();


// Luo tietokannan ja alustaa sen sovelluksen tarvitsemilla datalla, jos se ei ole vielä olemassa
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KaluDbContext>();
    DbInitializer.SeedDefaults(context);
}

// Konfiguroi sovelluksen käyttämään meidän error sivua kun ei olla developement environmentissa
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // Parantaa turvallisuutta, kun käytetään https:ää
    app.UseHsts();
}

// Uudelleenohjaa HTTP-pyynnöt HTTPS:ään (suojattu yhteys)
app.UseHttpsRedirection();

// Palvelee staattisia tiedostoja (esim. CSS, JS, kuvat) wwwroot-kansiosta
app.UseStaticFiles(); 

// Ottaa käyttöön reitityksen, jotta pyyntö ohjataan oikealle controllerille ja actionille
app.UseRouting();

// Ottaa käyttöön sessionhallinnan (esimerkiksi käyttäjän tilatiedot)
app.UseSession(); 

// Ottaa käyttöön todennuksen (käyttäjän kirjautuminen)
app.UseAuthentication();

// Tarkistaa, onko käyttäjällä oikeudet suoritettaviin toimintoihin
app.UseAuthorization(); 

// Määrittelee oletusreitityksen MVC-kontrollereille
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 


// Käynnistää sovelluksen ja alkaa kuunnella HTTP-pyyntöjä
app.Run(); 
