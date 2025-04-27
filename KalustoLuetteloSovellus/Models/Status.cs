using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalustoLuetteloSovellus.Models;

public partial class Status
{
    public int StatusId { get; set; }

    [Display(Name = "Status nimi")]
    public string StatusNimi { get; set; } = null!;

    public virtual ICollection<Tapahtuma> Tapahtumat { get; set; } = new List<Tapahtuma>();
}
