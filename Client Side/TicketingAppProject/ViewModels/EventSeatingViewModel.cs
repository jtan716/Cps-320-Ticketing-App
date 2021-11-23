using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using TicketingAppProject.Models;
using Newtonsoft.Json.Linq;
using TicketingAppProject.Services;

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

        public string EventDescription => MyEvent.eventDescription;

        public string EventPricePerSeat => "$"+ MyEvent.eventPricePerSeat.ToString("0.00");

        public string EventDurationInHours => MyEvent.eventDurationInHours.ToString("0.00") + " hours.";

        public string EventStartDateTime => MyEvent.eventStartDateAndTime.ToString("dddd, dd MMMM yyyy");
        
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
                EventSeatList = await URL_Server.getURLEventSeatingChart(MyEvent.eventID).GetJsonAsync<List<Seat_Server>>();
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

        public async Task HTTPPutSeatingReservation()
        {
            HoldRequest newHold = new HoldRequest();
            newHold.userid = User_Server.loggedinUserID;
            newHold.selectedseats = "";
            
            for (int i = 0; i < SeatingChart.GetLength(0); i++)
            {
                for (int j = 0; j < SeatingChart.GetLength(1); j++)
                {
                    if (SeatingChart[i, j].is_selected_by_user == true)
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
                else
                {
                    newHold.selectedseats = newHold.selectedseats.Substring(0, newHold.selectedseats.Length - 1);
                }

                string feedback = await URL_Server.getURLPutHoldOnSeats(MyEvent.eventID).PutJsonAsync(newHold)
                    .ReceiveString();
                Message = feedback;
                
                //TODO Figure out why selected Seat will not be updated
                OnPropertyChanged(nameof(SeatingChart));
                
                Console.WriteLine($"!@Debug: HTTPPutSeatingReservation responded with {feedback}");
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
            }
        }

    }

    internal class HoldRequest
    {
        public string userid { get; set; }
        public string selectedseats { get; set; }
    }

}