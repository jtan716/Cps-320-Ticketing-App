using System;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;

namespace TicketingAppProject.ViewModels
{
    public class UserProfileViewModel:BaseViewModel
    {
        public UserProfileViewModel(User_Server userProfile)
        {
            UserProfile = userProfile;
        }

        private User_Server UserProfile;
        
        public string UserEmail => UserProfile.userEmail;
        public string UserPassword => UserProfile.userPassword;
        public string UserCreditCardNumber => UserProfile.userCreditCardNumber;
        public string UserCreditCardExpDate => UserProfile.userCreditCardExpDate.ToString("dddd, dd MMMM yyyy");
        public string UserCreditCardCVV => UserProfile.userCreditCardCVV.ToString();

        public async Task HTTPGetUser()
        {
            try
            {
                User_Server qUser = await URL_Server.getURLUserProfile(User_Server.loggedinUserID).GetJsonAsync<User_Server>();
                UserProfile = qUser;
                Console.WriteLine($"@Debug!: HTTPGetUser() returned with values {qUser.userEmail} and {qUser.userPassword}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetUser() : {ex.Message}");
            }
        }

    }
}