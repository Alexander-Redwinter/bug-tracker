namespace BugTracker.Models
{
    public class ProjectApplicationUser
    {
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

    }
}
