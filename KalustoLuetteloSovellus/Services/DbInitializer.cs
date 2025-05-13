//Initialisoi Db:n (statukset ja roolit)

using KalustoLuetteloSovellus.Models;

public static class DbInitializer
{
    public static void SeedDefaults(KaluDbContext _context)
    {
        var defaultStatusNames = new[]
        {
            "Varattu", "Vapaa", "Huollossa", "Kadonnut", "Poistettu"
        };

        foreach (var name in defaultStatusNames)
        {
            if (!_context.Statukset.Any(s => s.StatusNimi == name))
            {
                _context.Statukset.Add(new Status { StatusNimi = name });
            }
        }

        var defaultRoolit = new[] { "Admin", "User" };

        foreach (var nimi in defaultRoolit)
        {
            if (!_context.Roolit.Any(r => r.RooliNimi == nimi))
            {
                _context.Roolit.Add(new Rooli { RooliNimi = nimi });
            }
        }

        _context.SaveChanges();
    }
}
