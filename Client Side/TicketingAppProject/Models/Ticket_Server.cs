using Newtonsoft.Json;

namespace TicketingAppProject.Models
{
    public class Ticket_Server
    {
        [JsonProperty("id")]
        public string ticketID { get; set; }
        
        [JsonProperty("userlinkid")]
        public string ticketUserIDAssociation { get; set; }
        
        [JsonProperty("eventlinkid")]
        public int ticketEventIDAssociation { get; set; }
        
        [JsonProperty("eventtitle")]
        public string ticketEventTitle { get; set; }
        
        [JsonProperty("seats_reserved")]
        public string ticketSeatsReserved { get; set; }
        
        [JsonProperty("creditcard_used")]
        public string ticketCreditCardNumberUsed { get; set; }
        
        [JsonProperty("total_price")]
        public float ticketTotalPrice { get; set; }
    }
}