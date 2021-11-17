using System;
using Flurl.Http;
using Newtonsoft.Json;
using TicketingAppProject.Models;
using Newtonsoft.Json.Linq;

namespace TicketingAppProject.ViewModels
{
    public class EventViewModel:BaseViewModel
    {

        public EventViewModel(Event_Server myEvent)
        {
            MyEvent = myEvent;
        }
        
        private Event_Server MyEvent { get; set; }

        public string EventTitle => MyEvent.eventTitle;

        public string EventDescription => MyEvent.eventDescription;

        public string EventPricePerSeat => "$"+ MyEvent.eventPricePerSeat.ToString("0.00");

        public string EventDurationInHours => MyEvent.eventDurationInHours.ToString("0.00") + " hours.";

        public string EventStartDateTime => MyEvent.eventStartDateAndTime.ToString("dddd, dd MMMM yyyy");

        private bool[][] SeatingChart;

    }

}