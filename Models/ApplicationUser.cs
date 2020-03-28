using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class ApplicationUser : IdentityUser
    {        
        public IList<TicketComment> TicketComments { get; set; }

        public IList<TicketApplicationUser> TicketApplicationUsers{ get; set; }

        public IList<TicketAttachment> TicketAttachments{ get; set; }

        public IList<Ticket> Tickets{ get; set; }

        public IList<TicketHistory> TicketHistories{ get; set; }
        public ICollection<ProjectApplicationUser> ProjectApplicationUsers { get; set; }
    }
}
