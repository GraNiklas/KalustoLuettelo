﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KalustoLuetteloSovellus.Models;

public partial class Tapahtuma
{
    public int TapahtumaId { get; set; }

    public int TuoteId { get; set; }

    [Display(Name = "Luonti pvm")]
    public DateTime AloitusPvm { get; set; }

    [Display(Name = "Palautus pvm")]
    public DateTime? LopetusPvm { get; set; }

    public string? Kommentti { get; set; }

    public int KäyttäjäId { get; set; }

    public int StatusId { get; set; }

    public virtual Käyttäjä? Käyttäjä { get; set; } = null!;

    public virtual Status? Status { get; set; } = null!;

    public virtual Tuote? Tuote { get; set; } = null!;
}
