using System;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;
using TicketingAppProject.Services;

namespace TicketingAppProject.ViewModels
{
    public class SeatingCheckOutViewModel: BaseViewModel
    {
        public SeatingCheckOutViewModel(Event_Server myEvent)
        {
            MyEvent = myEvent;
            TotalPrice = TotalPriceFromServer.ToString();
            SelectedSeats = HeldSeats;
            CreditCardNum = "PLACEHOLDER";
            //CreditCardNum = UserProfileViewModel.UserProfile.userCreditCardNumber;
        }
        public Event_Server MyEvent { get; set; }
        
        public string EventTitle => MyEvent.eventTitle;

        public string EventDescription => MyEvent.eventDescription;

        public string EventPricePerSeat => "$"+ MyEvent.eventPricePerSeat.ToString("0.00");

        public string EventDurationInHours => MyEvent.eventDurationInHours.ToString("0.00") + " hours.";

        public string EventStartDateTime => MyEvent.eventStartDateAndTime.ToString("dddd, dd MMMM yyyy");

        public static float TotalPriceFromServer { get; set; }
        public static string HeldSeats { get; set; }

        private string _selectedSeats;
        public string SelectedSeats
        {
            get => _selectedSeats;
            set => SetProperty(ref _selectedSeats, value);
        }

        private string _totalPrice;
        public string TotalPrice
        {
            get => "$" + _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        private string _creditCardNum;
        public string CreditCardNum
        {
            get => _creditCardNum;
            set => SetProperty(ref _creditCardNum, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        
        public async Task<bool> HTTPReserveSeatsRequest()
        {
            Console.WriteLine("@Debug: Executing HTTPReserveSeatsRequest().");
            Console.WriteLine($"eventID: {MyEvent.eventID}");
            Console.WriteLine($"seats: {SelectedSeats}");
            Console.WriteLine($"creditcardnumber: {CreditCardNum}");
            
            Message = "";
            bool success = false;
            try
            {
                ReserveRequest myReserveRequest = new ReserveRequest();
                myReserveRequest.eventid = MyEvent.eventID;
                myReserveRequest.seats = SelectedSeats;
                myReserveRequest.creditcardnum = CreditCardNum;
                Ticket_Server myTicket = new Ticket_Server();
                myTicket =
                    await URL_Server.getURLReserveSeats().WithCookie("loginsession",User_Server.loggedinSessionID).PostJsonAsync(myReserveRequest)
                        .ReceiveJson<Ticket_Server>();
                success = true;
                Message = "Successfully reserved seats!";
            }
            catch (FlurlHttpException httpex)
            {
                string connectionError = httpex.Message;
                Console.WriteLine($"@Debug: FlurlHTTPException message: {connectionError}");
                
                //For iOS 
                if (connectionError.Contains("Connection refused"))
                {
                    Message = "Connection Error: could not find a connection. Please try again later.";
                }
                else
                {
                    var apiError = await httpex.GetResponseJsonAsync<APIError>();
                    Message = apiError.Error;
                }
            }

            catch (Exception ex)
            {
                Message = ex.Message;
                Console.WriteLine("@Debug: " + ex.GetType() + " at HTTPReserveSeatsRequest(), " + ex.Message);
            }

            return success;
        }

    }
    
    internal class ReserveRequest
    {
        public int eventid { get; set; }
        public string seats { get; set; }
        public string creditcardnum { get; set; }
    }

}