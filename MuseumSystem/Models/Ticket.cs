using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public int? TypeId { get; set; }

    public int UserId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public DateOnly ValidFrom { get; set; }

    public DateOnly ValidTo { get; set; }

    public decimal Price { get; set; }

    public bool? IsUsed { get; set; }

    public virtual ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();

    public virtual TicketType? Type { get; set; }

    public virtual User User { get; set; } = null!;
}
