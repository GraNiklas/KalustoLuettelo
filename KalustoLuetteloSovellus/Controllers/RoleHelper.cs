namespace KalustoLuetteloSovellus
{
    public static class RoleHelper
    {
        public static bool IsAdmin(HttpContext context)
        {
            return context.Session.GetString("Rooli") == "Admin";
        }
        public static bool IsUser(int käyttäjäId,HttpContext context)
        {
            return context.Session.GetInt32("KäyttäjäId") == käyttäjäId;
        }
    }
}
