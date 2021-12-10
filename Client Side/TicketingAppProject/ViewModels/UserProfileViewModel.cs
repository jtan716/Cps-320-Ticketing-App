using System;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;
using TicketingAppProject.Services;

namespace TicketingAppProject.ViewModels
{
    //TODO: Handle Null/None values returned from server api call
    public class UserProfileViewModel:BaseViewModel
    {
        public UserProfileViewModel(User_Server userProfile)
        {
            UserProfile = userProfile;
            //UserCreditCardExpDate = userProfile.userCreditCardExpDate;
            //UserCreditCardCVV = userProfile.userCreditCardCVV;
        }

        public static User_Server UserProfile;

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        
        public string UserEmail => UserProfile.userEmail;
        public string UserPassword => UserProfile.userPassword;

        private string _userCreditCardNumber;
        public string UserCreditCardNumber
        {
            get => _userCreditCardNumber;
            set
            {
                SetProperty(ref _userCreditCardNumber, value);
                OnPropertyChanged(nameof(IsProfileChanged));
            }
        }

        private DateTime _userCreditCardExpDate;
        public DateTime UserCreditCardExpDate
        {
            get => _userCreditCardExpDate;
            set
            {
                SetProperty(ref _userCreditCardExpDate, value);
                OnPropertyChanged(nameof(IsProfileChanged));
            }
        }

        private string _userCreditCardCVV;
        public string UserCreditCardCVV
        {
            get => _userCreditCardCVV;
            set
            {
                SetProperty(ref _userCreditCardCVV, value);
                OnPropertyChanged(nameof(IsProfileChanged));
            }
        }

        public async Task HTTPGetUser()
        {
            try
            {
                User_Server qUser = await URL_Server.getURLUserProfile().WithCookie("loginsession",User_Server.loggedinSessionID).GetJsonAsync<User_Server>();
                UserProfile = qUser;
                setCreditCardInfo();
                updateDisplayValues();
                Console.WriteLine($"@Debug!: HTTPGetUser() returned with values {qUser.userEmail} and {qUser.userPassword}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetUser() : {ex.Message}");
            }
        }

        public bool IsProfileChanged => UserCreditCardNumber!=UserProfile.userCreditCardNumber || UserCreditCardExpDate!=UserProfile.userCreditCardExpDate || UserCreditCardCVV!=UserProfile.userCreditCardCVV.ToString();

        public async Task HTTPUpdateUserProfile()
        {
            Message = "";
            IsBusy = true;
            try
            {
                await URL_Server.getURLUserProfile().WithCookie("loginsession", User_Server.loggedinSessionID)
                    .PatchJsonAsync(new
                    {
                        creditcard_num = UserCreditCardNumber, 
                        creditcard_exp = UserCreditCardExpDate.ToString("MM/dd/yyyy"),
                        creditcard_cvv = int.Parse(UserCreditCardCVV)
                    });
                Message = "Successfully updated profile!";
                updateDisplayValues();
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
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPUpdateUserProfile() : {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(Message));
            }
        }

        private void setCreditCardInfo()
        {
            UserCreditCardNumber = UserProfile.userCreditCardNumber;
            UserCreditCardExpDate = UserProfile.userCreditCardExpDate;
            _userCreditCardCVV = UserProfile.userCreditCardCVV.ToString();
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