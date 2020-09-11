using System.ComponentModel.DataAnnotations;

namespace BugTracker
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
