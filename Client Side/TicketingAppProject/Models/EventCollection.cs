using System.Collections.ObjectModel;

namespace TicketingAppProject.Models
{
    public class EventCollection
    {
        public ObservableCollection<Event_Server> Events;

        private EventCollection()
        {
            Events = new ObservableCollection<Event_Server>();
        }
        
        /*
         * #Static variable 
         * @description: if null, creates new EntryCollection() object and then returns it 
         */
        public static EventCollection Instance { get; } = new EventCollection();

        public void RemoveEvent(Event_Server myEvent)
        {
            Events.Remove(myEvent);
        }
        
        public void AddEvent(Event_Server myEvent)
        {
            Events.Add(myEvent);
        }
    }
}