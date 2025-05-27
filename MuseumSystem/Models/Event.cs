using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MuseumSystem.Models;

public partial class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? TypeId { get; set; }

    public string? Description { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime? EndDatetime { get; set; }

    public string? Addres { get; set; }

    public int? MaxAttendees { get; set; }

    public decimal? Price { get; set; }

    public bool? IsActive { get; set; }

    public int? OrganizerId { get; set; }

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public int RegistrationCount
    {
        get => EventRegistrations.Count;
    }

    public bool IsOld
    {
        get => StartDatetime < DateTime.Now.Date;
    }
    public string Color
    {
        get
        {
            if (IsOld)
                return "#c19f7b";
            else
                return "#E0CFBD";
        }
    }

    public string? ImageName { get; set; }
    public Bitmap MainImageBitmap
    {
        get
        {
            try
            {
                return new Bitmap(Environment.CurrentDirectory + "/Pictures/" + ImageName);
            }
            catch
            {
                return new Bitmap(Environment.CurrentDirectory + "/no_image_available.jpg");
            }
        }
    }

    public virtual ICollection<IncludedItem> IncludedItems { get; set; } = new List<IncludedItem>();

    public List<int> IncludedExhibits
    {
        get => IncludedItems.Select(e => e.ExhibitId).ToList();
    }

    public virtual User? Organizer { get; set; }

    public virtual EventType? Type { get; set; }

    public virtual ICollection<EventReview> EventReviews { get; set; } = new List<EventReview>();
}
