using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumSystem.Models
{
    public class ExhibitReview
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ExhibitId { get; set; }

        public int? Raiting { get; set; }

        public string? Review { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Exhibit Exhibit { get; set; } = null!;
    }
}
