using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Kategoria
{
    public int KategoriaId { get; set; }

    public string KategoriaNimi { get; set; } = null!;

    public virtual ICollection<Tuote> Tuotteet { get; set; } = new List<Tuote>();
}
