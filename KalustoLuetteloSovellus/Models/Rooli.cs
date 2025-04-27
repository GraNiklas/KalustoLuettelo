using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalustoLuetteloSovellus.Models;

public partial class Rooli
{
    public int RooliId { get; set; }

    [Display(Name = "Rooli nimi")]
    public string RooliNimi { get; set; } = null!;

    public virtual ICollection<Käyttäjä> Käyttäjät { get; set; } = new List<Käyttäjä>();
}
