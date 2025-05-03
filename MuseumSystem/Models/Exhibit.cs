using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseumSystem.Models;

public partial class Exhibit
{
    public int Id { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? AddDate { get; set; }

    public string? Condition { get; set; }

    public decimal? ApproximateCost { get; set; }

    public string PermanentlyLocated { get; set; } = null!;

    public string? MainImage { get; set; }

    public Bitmap MainImageBitmap
    {
        get
        {
            try
            {
                return new Bitmap(Environment.CurrentDirectory + "/Pictures/" + MainImage);
            }
            catch
            {
                return new Bitmap(Environment.CurrentDirectory + "/no_image_available.jpg");
            }
        }
    }

    public virtual ICollection<AtachedMedium> AtachedMedia { get; set; } = new List<AtachedMedium>();

    public List<AtachedMedium> SpecificMdeia(int TypeId)
    {
        return AtachedMedia.Where(media => media.TypeId == TypeId).ToList();
    }

    public virtual Category? Category { get; set; }

    public virtual ICollection<IncludedItem> IncludedItems { get; set; } = new List<IncludedItem>();
}
