using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public enum TicketPriority
    {
        [Display(Name = "None")]
        None,
        [Display(Name = "Low")]
        Low,
        [Display(Name = "Medium")]
        Medium,
        [Display(Name = "High")]
        High
    }
}
