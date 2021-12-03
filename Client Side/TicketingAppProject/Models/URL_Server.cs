namespace TicketingAppProject.Models
{
    public class URL_Server
    {
        private static string addressLocal = "127.0.0.1";
        private static string addressVirtual = "10.0.2.2";
        
        public static string getURLLogin()
        {
            return $"http://{addressLocal}:5000/users/login";
        }

        public static string getURLUserProfile()
        {
            return $"http://{addressLocal}:5000/users/myprofile";
        }

        public static string getURLUserTickets()
        {
            return $"http://{addressLocal}:5000/users/myprofile/tickets";
        }

        public static string getURLEventList()
        {
            return $"http://{addressLocal}:5000/events";
        }

        public static string getURLEventSeatingChart(int eventID)
        {
            return $"http://{addressLocal}:5000/events/{eventID}/seating";
        }

        public static string getURLPutHoldOnSeats(int eventID)
        {
            return $"http://{addressLocal}:5000/events/{eventID}/seating";
        }

        public static string getURLReserveSeats()
        {
            return $"http://{addressLocal}:5000/users/myprofile/tickets";
        }

    }
}