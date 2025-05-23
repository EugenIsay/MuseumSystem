using Avalonia.Controls.Documents;
using System;
using System.Collections.Generic;

namespace MuseumSystem.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string? Password { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string FullName
    {
        get => LastName + " " + FirstName + " " + Patronymic;
    }
    public string Color
    {
        get
        {
            if ((bool)IsActive || IsActive == null)
                return "#E0CFBD";
            else
                return "#c19f7b";
        }
    }
    public int GenderId { get; set; }

    public int RoleId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public DateOnly Birthday { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual Gender Gender { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
