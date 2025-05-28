using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuseumSystem.Models
{
    public class EventReview
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        public int? Raiting { get; set; }

        public string? Review { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Event Event { get; set; } = null!;

        public string GoodReviews
        {
            get => new string('★', (int)Raiting);
        }
        public string BadReview
        {
            get => new string('★', 5 - (int)Raiting);
        }
    }
}
