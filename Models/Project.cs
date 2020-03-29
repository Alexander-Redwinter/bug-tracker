using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
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

        public ICollection<ProjectApplicationUser> ProjectApplicationUsers{ get; set; }
    }
}
