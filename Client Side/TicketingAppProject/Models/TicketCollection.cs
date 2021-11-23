using System.Collections.ObjectModel;

namespace TicketingAppProject.Models
{
    public class TicketCollection
    {
        public ObservableCollection<Ticket_Server> Tickets;

        private TicketCollection()
        {
            Tickets = new ObservableCollection<Ticket_Server>();
        }
        
        /*
         * #Static variable 
         * @description: if null, creates new EntryCollection() object and then returns it 
         */
        public static TicketCollection Instance { get; } = new TicketCollection();

        public void RemoveTicket(Ticket_Server myTicket)
        {
            Tickets.Remove(myTicket);
        }
        
        public void AddTicket(Ticket_Server myTicket)
        {
            Tickets.Add(myTicket);
        }
    }
}