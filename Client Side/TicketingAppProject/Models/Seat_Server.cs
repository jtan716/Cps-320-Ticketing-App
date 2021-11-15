namespace TicketingAppProject.Models
{
    public class Seat_Server
    {
        
        public string seatRowID { get; set; }
        
        public int seatColID { get; set; }

        public int eventLinkID { get; set; }
        
        public bool status_held { get; set; }
        
        public bool status_reserved { get; set; }
        
    }
}