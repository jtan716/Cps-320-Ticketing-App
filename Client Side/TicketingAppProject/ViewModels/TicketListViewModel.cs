using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;
using TicketingAppProject.Services;

namespace TicketingAppProject.ViewModels
{
    public class TicketListViewModel: BaseViewModel
    {
        public MappedObservableCollection<Ticket_Server, TicketViewModel> MappedTicketCollection { get; }

        public TicketListViewModel()
        {
            MappedTicketCollection =
                new MappedObservableCollection<Ticket_Server, TicketViewModel>(
                    myticket => new TicketViewModel(myticket), TicketCollection.Instance.Tickets);
        }

        public async Task HTTPGetUserTickets()
        {
            try
            {
                List<Ticket_Server> tickets = await URL_Server.getURLUserTickets().WithCookie("loginsession",User_Server.loggedinSessionID).GetJsonAsync<List<Ticket_Server>>();
                TicketCollection.Instance.Tickets.Clear();
                foreach (Ticket_Server myticket in tickets)
                {
                    Console.WriteLine($"@Debug!: HTTPGetUserTickets() called: ticket -> {myticket.ticketID}");
                    TicketCollection.Instance.AddTicket(myticket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetUserTickets() : {ex.Message}");
            }
        }
    }
}