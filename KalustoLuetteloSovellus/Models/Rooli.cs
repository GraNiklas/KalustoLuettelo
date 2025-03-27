using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Rooli
{
    public int RooliId { get; set; }

    public string RooliNimi { get; set; } = null!;

    public virtual ICollection<Käyttäjä> Käyttäjäs { get; set; } = new List<Käyttäjä>();
}
