using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Project
    {






        public ICollection<Ticket> Tickets{ get; set; }
    }
}
