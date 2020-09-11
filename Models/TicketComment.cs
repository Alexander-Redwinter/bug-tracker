using System;
using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public class TicketComment
    {
        [Key]
        public int Id { get; set; }

        public DateTime Created { get; set; }
        public Ticket Ticket { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string Comment { get; set; }

    }
}