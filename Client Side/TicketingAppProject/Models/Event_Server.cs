using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TicketingAppProject.Models
{
    public class Event_Server
    {
        [JsonProperty("id")]
        public int eventID { get; set; }
        
        [JsonProperty("title")]
        public string eventTitle { get; set; }
        
        [JsonProperty("description")]
        public string eventDescription { get; set; }
        
        [JsonProperty("priceperseat")]
        public float eventPricePerSeat { get; set; }
        
        [JsonProperty("durationhours")]
        public float eventDurationInHours { get; set; }
        
        [JsonProperty("dateandtime")]
        public DateTime eventStartDateAndTime { get; set; }

        public bool is_selected_by_user { get; set; }
    }
}