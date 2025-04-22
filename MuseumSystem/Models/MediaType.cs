using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class MediaType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AtachedMedium> AtachedMedia { get; set; } = new List<AtachedMedium>();
}
