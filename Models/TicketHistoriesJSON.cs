using System;

namespace BugTracker
{
    public class TicketHistoriesJSON
    {
        public string[] Title { get; set; }
        public string[] Description { get; set; }
        public string[] Created { get; set; }
        public string[] LastResolveDate { get; set; }
        public int[] TicketPriority { get; set; }
        public int[] TicketStatus { get; set; }
        public int[] TicketType { get; set; }


    }
}
