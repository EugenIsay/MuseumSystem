using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class AtachedMedium
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public int ExhibitId { get; set; }

    public string Path { get; set; } = null!;

    public string? TempPath;

    public Bitmap ImageBitmap
    {
        get
        {
            if (TypeId == 1)
            {
                try
                {
                    if (TempPath == null)
                        return new Bitmap(Environment.CurrentDirectory + "/Pictures/" + Path);
                    else
                        return new Bitmap(TempPath);
                }
                catch
                {
                    return new Bitmap(Environment.CurrentDirectory + "/no_image_available.jpg");
                }
            }
            else
                return null;
        }
    }

    public string? Description { get; set; }

    public bool HasDescription
    {
        get => !string.IsNullOrEmpty(Description);
    }

    public virtual Exhibit Exhibit { get; set; } = null!;

    public virtual MediaType Type { get; set; } = null!;
}
