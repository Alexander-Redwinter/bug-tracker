using BugTracker.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Ticket
    {


        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public DateTime LastResolveDate { get; set; }


        public TicketPriority TicketPriority { get; set; } = TicketPriority.High;
        public TicketStatus TicketStatus { get; set; } = TicketStatus.Open;
        public TicketType TicketType { get; set; } = TicketType.BugError;


        public Project Project { get; set; }

        public IList<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
        public IList<TicketAttachment> TicketAttachments { get; set; } = new List<TicketAttachment>();
        public IList<TicketHistory> TicketHistories { get; set; } = new List<TicketHistory>();

        //assigned users
        public IList<TicketApplicationUser> TicketApplicationUsers { get; set; } = new List<TicketApplicationUser> ();

        //submitter
        public ApplicationUser ApplicationUser { get; set; }

    }
}
