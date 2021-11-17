using System;
using System.Collections.Generic;
using Flurl.Http;
using System.Threading.Tasks;
using TicketingAppProject.Models;
using TicketingAppProject.Services;

namespace TicketingAppProject.ViewModels
{
    public class EventListViewModel
    {
        public MappedObservableCollection<Event_Server, EventViewModel> MappedEventCollection { get; }

        public EventListViewModel()
        {
            MappedEventCollection = new MappedObservableCollection<Event_Server, EventViewModel>(
                myevent => new EventViewModel(myevent),
                EventCollection.Instance.Events);
        }

        public async Task HTTPGetEventList()
        {
            try
            {
                List<Event_Server> events = await URL_Server.getURLEventList().GetJsonAsync<List<Event_Server>>();
                EventCollection.Instance.Events.Clear();
                foreach (Event_Server myevent in events)
                {
                    Console.WriteLine($"@Debug!: HTTPGetEventList() called: event -> {myevent.eventTitle}");
                    EventCollection.Instance.AddEvent(myevent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetEventList() : {ex.Message}");
            }
        }
    }
}