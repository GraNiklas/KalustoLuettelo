using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalustoLuetteloSovellus.Models;

public partial class Toimipiste
{
    public int ToimipisteId { get; set; }

    public string Oppilaitos { get; set; } = null!;

    public string Kaupunki { get; set; } = null!;

    [Display(Name = "Toimipiste nimi")]
    public string? ToimipisteNimi { get; set; }

    public virtual ICollection<Tuote> Tuotteet { get; set; } = new List<Tuote>();
    [Display(Name = "Kaupunki ja toimipiste")]
    public string? KaupunkiJaToimipisteNimi => $"{Kaupunki} - {ToimipisteNimi} ";

}
