using MimeKit.Encodings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalustoLuetteloSovellus.Models;

public partial class Tuote
{
    public int TuoteId { get; set; }

    public string IdNumero { get; set; } = null!;

    public int KategoriaId { get; set; }

    public string? Kuvaus { get; set; }

    public byte[]? Kuva { get; set; }
    [NotMapped] public IFormFile? KuvaFile { get; set; }

    [Display(Name = "Osto pvm")]
    public DateOnly? OstoPvm { get; set; }

    public int? Hinta { get; set; }

    public DateOnly? Takuu { get; set; }

    public bool Aktiivinen { get; set; }

    public int ToimipisteId { get; set; }

    public virtual Kategoria? Kategoria { get; set; } = null!;

    public virtual ICollection<Tapahtuma> Tapahtumat { get; set; } = new List<Tapahtuma>();

    public virtual Toimipiste? Toimipiste { get; set; } = null!;

    [Display(Name = "Viimeisin tapahtuma")]
    public virtual Tapahtuma? ViimeisinTapahtuma => Tapahtumat.OrderByDescending(t => t.AloitusPvm).FirstOrDefault();
    [NotMapped] public virtual Status? Status => ViimeisinTapahtuma?.Status ?? new Status { StatusId = 60001, StatusNimi = "Vapaa"};
}
