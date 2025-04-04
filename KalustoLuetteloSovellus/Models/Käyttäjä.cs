using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Käyttäjä
{
    public int KäyttäjäId { get; set; }

    public string Käyttäjätunnus { get; set; } = null!;

    public string Salasana { get; set; } = null!;

    public int RooliId { get; set; }

    public virtual Rooli? Rooli { get; set; } = null!;

    public virtual ICollection<Tapahtuma> Tapahtumas { get; set; } = new List<Tapahtuma>();
}
