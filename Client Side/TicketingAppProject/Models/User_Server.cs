using System;
using Newtonsoft.Json;

namespace TicketingAppProject.Models
{
    public class User_Server
    {
        public static string loggedinUserID;
        
        [JsonProperty("id")]
        public string userID { get; set; }
        
        [JsonProperty("email")]
        public string userEmail { get; set; }
        
        [JsonProperty("password")]
        public string userPassword { get; set; }
        
        [JsonProperty("creditcard_number")]
        public string userCreditCardNumber { get; set; }
        
        [JsonProperty("creditcard_expdate")]
        public DateTime userCreditCardExpDate { get; set;}
        
        [JsonProperty("creditcard_cvv")]
        public int userCreditCardCVV { get; set; }

    }
}