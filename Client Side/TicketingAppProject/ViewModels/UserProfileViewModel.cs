using System;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;

namespace TicketingAppProject.ViewModels
{
    //TODO: Handle Null/None values returned from server api call
    public class UserProfileViewModel:BaseViewModel
    {
        public UserProfileViewModel(User_Server userProfile)
        {
            UserProfile = userProfile;
        }

        private User_Server UserProfile;
        
        public string UserEmail => $"Email: {UserProfile.userEmail} ";
        public string UserPassword => $"Password: {UserProfile.userPassword}";
        public string UserCreditCardNumber => $"Registered Credit Card No: {UserProfile.userCreditCardNumber}";
        public string UserCreditCardExpDate => $"Credit Card Exp Date: {UserProfile.userCreditCardExpDate:dddd, dd MMMM yyyy}";
        public string UserCreditCardCVV => $"Credit Card CVV: {UserProfile.userCreditCardCVV}";

        public async Task HTTPGetUser()
        {
            try
            {
                User_Server qUser = await URL_Server.getURLUserProfile(User_Server.loggedinUserID).GetJsonAsync<User_Server>();
                UserProfile = qUser;
                updateDisplayValues();
                Console.WriteLine($"@Debug!: HTTPGetUser() returned with values {qUser.userEmail} and {qUser.userPassword}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetUser() : {ex.Message}");
            }
        }

        private void updateDisplayValues()
        {
            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(UserPassword));
            OnPropertyChanged(nameof(UserCreditCardNumber));
            OnPropertyChanged(nameof(UserCreditCardExpDate));
            OnPropertyChanged(nameof(UserCreditCardCVV));
        }

    }
}