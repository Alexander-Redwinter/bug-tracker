using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Enums
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
