using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BugTracker
{
    public class ApplicationUser : IdentityUser
    {        
        public IList<TicketComment> TicketComments { get; set; }

        public IList<TicketApplicationUser> TicketApplicationUsers{ get; set; }

        public IList<TicketAttachment> TicketAttachments{ get; set; }

        public IList<Ticket> Tickets{ get; set; }

        public IList<TicketHistory> TicketHistories { get; set; } = new List<TicketHistory>();
        public ICollection<ProjectApplicationUser> ProjectApplicationUsers { get; set; } = new List<ProjectApplicationUser>();
    }
}
