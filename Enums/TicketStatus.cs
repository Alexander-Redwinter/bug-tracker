using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Enums
{
    public enum TicketStatus
    {
        [Display(Name = "Open")]
        Open,
        [Display(Name = "In Progress")]
        InProgress,
        [Display(Name = "Resolved")]
        Resolved,
        [Display(Name = "Require Additional Info")]
        RequireAdditionalInfo,
        [Display(Name = "Unknown")]
        Unknown
    }
}
