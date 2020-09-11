using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public enum TicketType
    {
        [Display(Name = "Bug/Error")]
        BugError,
        [Display(Name = "Feature Request")]
        FeatureRequest,
        [Display(Name = "Assistance Request")]
        AssistanceRequest,
        [Display(Name = "Other")]
        Other
    }
}
