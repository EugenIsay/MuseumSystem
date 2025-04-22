using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class AtachedMedium
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public int ExhibitId { get; set; }

    public string Path { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;

    public virtual MediaType Type { get; set; } = null!;
}
