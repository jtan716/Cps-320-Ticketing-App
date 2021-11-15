namespace TicketingAppProject.Models
{
    public class Ticket_Server
    {
        public int ticketID { get; set; }
        
        public string ticketUserIDAssociation { get; set; }
        
        public int ticketEventIDAssociation { get; set; }
        
        public string ticketSeatsReserved { get; set; }
        
        public string ticketCreditCardNumberUsed { get; set; }
        
        public int ticketTotalPrice { get; set; }
    }
}