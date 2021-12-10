using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Flurl.Http;
using Newtonsoft.Json;
using TicketingAppProject.Models;
using Newtonsoft.Json.Linq;
using TicketingAppProject.Services;
using TicketingAppProject.Views;
using Xamarin.Forms;

namespace TicketingAppProject.ViewModels
{
    public class EventSeatingViewModel:BaseViewModel
    {

        public EventSeatingViewModel(Event_Server myEvent)
        {
            MyEvent = myEvent;
            Title = "Event View";
        }

        public Event_Server MyEvent { get; set; }

        public string EventTitle => MyEvent.eventTitle;

        public string EventPricePerSeat => "Price per seat: $"+ MyEvent.eventPricePerSeat.ToString("0.00");

        public string EventDurationInHours => "Duration: " + MyEvent.eventDurationInHours.ToString("0.00") + " hours.";

        public string EventStartDateTime => "Duration Start Time: " +  MyEvent.eventStartDateAndTime.ToString("dddd, dd MMMM yyyy");
        
        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }


        public int EventNumRows { get; set; }
        
        public int EventNumCols { get; set; }

        private List<Seat_Server> EventSeatList;
        
        public Seat_Server[,] SeatingChart { get; set; }

        public async Task HTTPGetSeatingList()
        {
            try
            {
                EventSeatList = await URL_Server.getURLEventSeatingChart(MyEvent.eventID).WithCookie("loginsession",User_Server.loggedinSessionID).GetJsonAsync<List<Seat_Server>>();
                //TODO MOD UNFINISHED CODE BELOW
                int maxRowValue = 65; //Unicode value for starting row character 'A'
                int maxColValue = 1; //Min value for starting col number 1
                foreach (Seat_Server seat in EventSeatList)
                {
                    if ((int)seat.seatRowID.ToCharArray()[0] > maxRowValue)
                    {
                        maxRowValue = (int) seat.seatRowID.ToCharArray()[0];
                    }
                    if (seat.seatColID > maxColValue)
                    {
                        maxColValue = seat.seatColID;
                    }
                }
                EventNumRows = maxRowValue - 65 + 1;
                EventNumCols = maxColValue - 1 + 1;

                SeatingChart = new Seat_Server[EventNumRows, EventNumCols];

                foreach (Seat_Server seat in EventSeatList)
                {
                    SeatingChart[((int) seat.seatRowID.ToCharArray()[0])-65, seat.seatColID-1] = seat;
                }
                
                OnPropertyChanged(nameof(SeatingChart));
                Console.WriteLine($"@Debug!: HTTPGetSeatingList() calculated the num rows to be {EventNumRows}");
                Console.WriteLine($"@Debug!: HTTPGetSeatingList() calculated the num cols to be {EventNumCols}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetEventList() : {ex.Message}");
            }
        }

        public async Task<bool> HTTPPutSeatingReservation()
        {
            Message = "";
            bool success = false;
            HoldRequest newHold = new HoldRequest();
            newHold.userid = User_Server.loggedinSessionID;
            newHold.selectedseats = "";
            
            //Get selected seats from the grid one by one
            for (int i = 0; i < SeatingChart.GetLength(0); i++)
            {
                for (int j = 0; j < SeatingChart.GetLength(1); j++)
                {
                    if (SeatingChart[i, j]!=null && SeatingChart[i, j].is_selected_by_user == true)
                    {
                        string selectedSeatID = SeatingChart[i, j].seatRowID + SeatingChart[i, j].seatColID;
                        newHold.selectedseats += selectedSeatID + ",";
                    }
                }
            }

            try
            {
                if (String.IsNullOrEmpty(newHold.selectedseats))
                {
                    //TODO Implement error dialogue 
                    Console.WriteLine("!@Debug!: HTTPPutSeatingReservation() Please select seats to hold!");
                }
                //Trim off the comma at the end
                else
                {
                    newHold.selectedseats = newHold.selectedseats.Substring(0, newHold.selectedseats.Length - 1);
                }

                //HTTP Request
                FeedbackDictionary feedback = await URL_Server.getURLPutHoldOnSeats(MyEvent.eventID)
                    .WithCookie("loginsession", User_Server.loggedinSessionID).PutJsonAsync(newHold)
                    .ReceiveJson<FeedbackDictionary>();
                SeatingCheckOutViewModel.TotalPriceFromServer = feedback.return_price;
                SeatingCheckOutViewModel.CreditCardNumberRegistered = feedback.return_creditcard;
                SeatingCheckOutViewModel.HeldSeats = newHold.selectedseats;
                success = true;
                
                //TODO Figure out why selected Seat will not be updated
                OnPropertyChanged(nameof(SeatingChart));
                Console.WriteLine($"!@Debug: HTTPPutSeatingReservation responded with {feedback}");
            }
            catch (FlurlHttpException httpex)
            {
                success = false;
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
                success = false;
                Message = ex.Message;
            }
            return success;
        }

        public async Task TestConcurrencyHoldFunction()
        {
            HoldRequest newHold = new HoldRequest();
            newHold.userid = User_Server.loggedinSessionID;
            newHold.selectedseats = "";
            
            //Get selected seats from the grid one by one
            for (int i = 0; i < SeatingChart.GetLength(0); i++)
            {
                for (int j = 0; j < SeatingChart.GetLength(1); j++)
                {
                    if (SeatingChart[i, j]!=null && SeatingChart[i, j].is_selected_by_user == true)
                    {
                        string selectedSeatID = SeatingChart[i, j].seatRowID + SeatingChart[i, j].seatColID;
                        newHold.selectedseats += selectedSeatID + ",";
                    }
                }
            }
            
            if (String.IsNullOrEmpty(newHold.selectedseats))
            {
                //TODO Implement error dialogue 
                Console.WriteLine("!@Debug!: HTTPPutSeatingReservation() Please select seats to hold!");
            }
            //Trim off the comma at the end
            else
            {
                newHold.selectedseats = newHold.selectedseats.Substring(0, newHold.selectedseats.Length - 1);
            }
            
            Task t1 = Task.Run(() =>
                URL_Server.getURLPutHoldOnSeats(MyEvent.eventID)
                    .WithCookie("loginsession", User_Server.loggedinSessionID).PutJsonAsync(newHold));
            Task t2 = Task.Run(() =>
                URL_Server.getURLPutHoldOnSeats(MyEvent.eventID)
                    .WithCookie("loginsession", User_Server.loggedinSessionID).PutJsonAsync(newHold));
        }
        
        public void NotifyAll()
        {
            OnPropertyChanged("");
        }

    }
    

    internal class HoldRequest
    {
        public string userid { get; set; }
        public string selectedseats { get; set; }
    }

    internal class FeedbackDictionary
    {
        [JsonProperty("price")]
        public float return_price { get; set; }

        [JsonProperty("user_creditcard")]
        public string return_creditcard { get; set; }
    }

}