using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public class Project
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsOpen { get; set; }

        public ICollection<Ticket> Tickets{ get; set; }

        public ICollection<ProjectApplicationUser> ProjectApplicationUsers { get; set; } = new List<ProjectApplicationUser>();
    }
}
