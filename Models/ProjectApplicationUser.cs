namespace BugTracker
{
    public class ProjectApplicationUser
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

    }
}
