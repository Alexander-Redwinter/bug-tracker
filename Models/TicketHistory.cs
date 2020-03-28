using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        [Key]
        public int Id { get; set; }
        public Ticket Ticket { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}