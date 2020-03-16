using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BugTracker.Models
{
    public class Ticket
    {

        [Key]
        public int Id { get; set; }
         
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Created { get; set; }

    }
}
