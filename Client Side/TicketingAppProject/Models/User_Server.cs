using System;

namespace TicketingAppProject.Models
{
    public class User_Server
    {
        public string userID { get; set; }
        
        public string userEmail { get; set; }
        
        public string userPassword { get; set; }
        
        public string userCreditCardNumber { get; set; }
        
        public DateTime userCreditCardExpDate { get; set;}
        
        public int userCreditCardCVV { get; set; }

    }
}