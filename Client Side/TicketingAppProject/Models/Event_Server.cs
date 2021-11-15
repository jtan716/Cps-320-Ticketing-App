using System;

namespace TicketingAppProject.Models
{
    public class Event_Server
    {
        public int eventID { get; set; }
        
        public string eventTitle { get; set; }
        
        public string eventDescription { get; set; }
        
        public float eventPricePerSeat { get; set; }
        
        public float eventDurationInHours { get; set; }
        
        public DateTime eventStartDateAndTime { get; set; }
        
    }
}