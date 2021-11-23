using TicketingAppProject.Models;

namespace TicketingAppProject.ViewModels
{
    public class TicketViewModel: BaseViewModel
    {
        public TicketViewModel(Ticket_Server myTicket)
        {
            MyTicket = myTicket;
        }
        
        private Ticket_Server MyTicket { get; set; }

        public string TicketID => MyTicket.ticketID;

        public string TicketEventTitle => MyTicket.ticketEventTitle;

        public string TicketSeatsReserved => MyTicket.ticketSeatsReserved;

        public string TicketCreditCardUsed => MyTicket.ticketCreditCardNumberUsed;

        public string TicketTotalPrice => "$"+MyTicket.ticketTotalPrice.ToString("0.00");

    }
}