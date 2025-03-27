using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Kategorium
{
    public int KategoriaId { get; set; }

    public string KategoriaNimi { get; set; } = null!;

    public virtual ICollection<Tuote> Tuotes { get; set; } = new List<Tuote>();
}
