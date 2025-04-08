using KalustoLuetteloSovellus.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Tämä hakee connection stringin appsettings.jsonista
builder.Services.AddDbContext<KaluDbContext>(options => options.UseSqlServer(connectionString)); // Lisää dbcontextin meidän azure sql serveriin i.e yhdistää sovelluksen azuren databaseen

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Lisäsin tämän paketin niin pystyy päivittää sivun ilman että tarvii rebuildata

// Add services to the container.
builder.Services.AddControllersWithViews(); //

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
