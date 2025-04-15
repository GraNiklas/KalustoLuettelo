using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Toimipiste
{
    public int ToimipisteId { get; set; }

    public string Oppilaitos { get; set; } = null!;

    public string Kaupunki { get; set; } = null!;

    public string? ToimipisteNimi { get; set; }

    public virtual ICollection<Tuote> Tuotes { get; set; } = new List<Tuote>();
    public string? KaupunkiJaToimipisteNimi => $"{Kaupunki} - {ToimipisteNimi} ";

}
