using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Tuote
{
    public int TuoteId { get; set; }

    public string IdNumero { get; set; } = null!;

    public int KategoriaId { get; set; }

    public int StatusId { get; set; }

    public string? Kuvaus { get; set; }

    public byte[]? Kuva { get; set; }

    public DateOnly? OstoPvm { get; set; }

    public int? Hinta { get; set; }

    public DateOnly? Takuu { get; set; }

    public bool Aktiivinen { get; set; }

    public int ToimipisteId { get; set; }

    public virtual Kategorium Kategoria { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<Tapahtuma> Tapahtumas { get; set; } = new List<Tapahtuma>();

    public virtual Toimipiste Toimipiste { get; set; } = null!;
}
