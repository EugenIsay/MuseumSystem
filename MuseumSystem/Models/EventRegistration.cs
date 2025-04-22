using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class EventRegistration
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int TicketId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Ticket Ticket { get; set; } = null!;
}
