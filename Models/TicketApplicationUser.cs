namespace BugTracker
{
    public class TicketApplicationUser
    {
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}