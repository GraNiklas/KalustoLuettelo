using KalustoLuetteloSovellus.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;




var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // T‰m‰ hakee connection stringin appsettings.jsonista
builder.Services.AddDbContext<KaluDbContext>(options => options.UseSqlServer(connectionString)); // Lis‰‰ dbcontextin meid‰n azure sql serveriin i.e yhdist‰‰ sovelluksen azuren databaseen

// Add services to the container.

builder.Services.AddScoped<AdminOnlyFilter>(); // Admin filtteri joka ei p‰‰st‰ k‰ytt‰j‰‰ semmosiin n‰kymiin mihin t‰m‰ filtteri on lis‰tty
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Lis‰sin t‰m‰n paketin niin pystyy p‰ivitt‰‰ sivun ilman ett‰ tarvii rebuildata

// Add services to the container.
builder.Services.AddControllersWithViews(); //

builder.Services.AddDistributedMemoryCache();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LoginPath = "/Home/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set how long the cookie is valid
        options.SlidingExpiration = true; // Extends cookie lifetime on activity
        options.Cookie.IsEssential = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KaluDbContext>();
    DbInitializer.SeedDefaults(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

