using KalustoLuetteloSovellus.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // T‰m‰ hakee connection stringin appsettings.jsonista
builder.Services.AddDbContext<KaluDbContext>(options => options.UseSqlServer(connectionString)); // Lis‰‰ dbcontextin meid‰n azure sql serveriin i.e yhdist‰‰ sovelluksen azuren databaseen

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Lis‰sin t‰m‰n paketin niin pystyy p‰ivitt‰‰ sivun ilman ett‰ tarvii rebuildata

// Add services to the container.
builder.Services.AddControllersWithViews(); //

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
