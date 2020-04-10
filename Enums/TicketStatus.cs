using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Enums
{
    public enum TicketStatus
    {
        Open,
        [Display(Name = "In Progress")]

        InProgress,
        Resolved,
        [Display(Name = "Require Additional Info")]
        RequireAdditionalInfo,
        Unknown
    }
}
