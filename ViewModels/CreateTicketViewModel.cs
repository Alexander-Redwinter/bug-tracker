using BugTracker.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public class CreateTicketViewModel
    {


        [Required]
        public string Title { get; set; } = "New Ticket";

        public string Description { get; set; }

        public TicketPriority TicketPriority { get; set; } = TicketPriority.High;
        public TicketStatus TicketStatus { get; set; } = TicketStatus.Open;
        public TicketType TicketType { get; set; } = TicketType.BugError;

        public List<Project> Projects { get; set; }

        public int ProjectId { get; set; }

        public byte[] Image { get; set; }

        public bool returnToProject { get; set; } = false;

    }
}
