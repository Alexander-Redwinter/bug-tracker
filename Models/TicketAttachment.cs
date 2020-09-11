using System.ComponentModel.DataAnnotations;

namespace BugTracker
{
    public class TicketAttachment
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public Ticket Ticket { get; set; }

        public byte[] Image { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}