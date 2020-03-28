using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketComment
    {
        [Key]
        public int Id { get; set; }

        public Ticket Ticket { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}