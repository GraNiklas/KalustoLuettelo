namespace KalustoLuetteloSovellus.ViewModels
{
    public class SalasananVaihtoViewModel
    {
        public string VanhaSalasana { get; set; }
        public string UusiSalasana { get; set; }
        public string Virheviesti { get; set; }
        public SalasananVaihtoViewModel()
        {
            VanhaSalasana = string.Empty;
            UusiSalasana = string.Empty;
            Virheviesti = string.Empty;
        }
    }
}