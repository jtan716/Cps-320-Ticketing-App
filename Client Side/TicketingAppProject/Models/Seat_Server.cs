using Newtonsoft.Json;

namespace TicketingAppProject.Models
{
    public class Seat_Server
    {
        [JsonProperty("rowid")]
        public string seatRowID { get; set; }
        
        [JsonProperty("colid")]
        public int seatColID { get; set; }

        [JsonProperty("eventlinkid")]
        public int eventLinkID { get; set; }
        
        [JsonProperty("status_held")]
        public bool status_held { get; set; }
        
        [JsonProperty("status_reserved")]
        public bool status_reserved { get; set; }

        public bool is_selected_by_user { get; set; }
        
    }
}