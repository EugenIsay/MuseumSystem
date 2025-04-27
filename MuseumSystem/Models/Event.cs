using System;
using System.Collections.Generic;

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

    public virtual ICollection<IncludedItem> IncludedItems { get; set; } = new List<IncludedItem>();

    public virtual User? Organizer { get; set; }

    public virtual EventType? Type { get; set; }
}
