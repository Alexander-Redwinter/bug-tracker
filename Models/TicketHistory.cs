using System;
using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public class TicketHistory
    {
        [Key]
        public int Id { get; set; }
        public Ticket Ticket { get; set; }
        public String ApplicationUser { get; set; }
        public String Project { get; set; }
        public System.DateTime When { get; set; }
        public String Action { get; set; }
        public String KeyValue { get; set; }
        public String OldValue { get; set; }
        public String NewValue { get; set; }

    }
}