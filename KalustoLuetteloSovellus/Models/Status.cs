using System;
using System.Collections.Generic;

namespace KalustoLuetteloSovellus.Models;

public partial class Status
{
    public int StatusId { get; set; }

    public string StatusNimi { get; set; } = null!;

    public virtual ICollection<Tuote> Tuotes { get; set; } = new List<Tuote>();
}
