namespace KalustoLuetteloSovellus.ViewModels
{
    public class StatsViewModel
    {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Used { get; set; }
    }
    public class PieChartViewModel
    {
        public Dictionary<string, float> Data { get; set; } = new Dictionary<string, float>();
    }
}

//nimi, prosentti

