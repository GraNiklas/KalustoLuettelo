using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalustoLuetteloSovellus.Models;

public partial class Kategoria
{
    public int KategoriaId { get; set; }

    [Display(Name ="Kategoria nimi")]
    public string KategoriaNimi { get; set; } = null!;

    public virtual ICollection<Tuote> Tuotteet { get; set; } = new List<Tuote>();
}
