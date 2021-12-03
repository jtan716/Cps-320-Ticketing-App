using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using TicketingAppProject.Models;
using TicketingAppProject.Services;
using TicketingAppProject.Views;
using Xamarin.Forms;

namespace TicketingAppProject.ViewModels
{
    public class LoginViewModelM: BaseViewModel
    {
        public LoginViewModelM(Login_Client myLoginClient)
        {
            this.MyLoginClient = myLoginClient;
            
            //TODO REMOVE ONCE FINISHED
            myLoginClient.loginEmail = "jack@bju.edu";
            myLoginClient.loginPassword = "123456";
            
            InputEmail = myLoginClient.loginEmail;
            InputPassword = myLoginClient.loginPassword;
            LoginCommand = new Command(OnLoginClicked);
        }
        
        private Login_Client MyLoginClient;
        private CancellationTokenSource cancelSource;
        public Command LoginCommand { get; }

        private string _inputEmail;
        public string InputEmail
        {
            get => _inputEmail;
            set => SetProperty(ref _inputEmail, value);
        }

        private string _inputPassword;
        public string InputPassword
        {
            get => _inputPassword;
            set => SetProperty(ref _inputPassword, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string UserEmail => MyLoginClient.loginEmail;
        public string UserPassword => MyLoginClient.loginPassword;

        public string UserID => MyLoginClient.loginUserID;

        public bool ValidateUsernamePasswordFormat()
        {
            //email username field must be non-empty and have the character '@'
            Message = "";
            bool isValid = true;

            if (String.IsNullOrEmpty(InputEmail))
            {
                Message += "Please enter in an email in the Email Entry Field. \n";
                isValid = false;
            }
            else if (!InputEmail.Contains("@"))
            {
                Message += "Please enter in an email (make sure to include the @ symbol). \n";
                isValid = false;
            }

            if (String.IsNullOrEmpty(InputPassword))
            {
                Message += "Please enter in a password in the Password Entry Field. \n";
                isValid = false;
            }
            
            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(UserPassword));
            OnPropertyChanged(nameof(UserID));

            return isValid;
        }

        private async Task<bool> HTTPLoginRequest()
        {
            Message = "";
            Console.WriteLine("@Debug: Executing HTTPLoginRequest().");
            bool success = false;
            try
            {
                User_Server.loggedinSessionID =
                    await URL_Server.getURLLogin().PostJsonAsync(new {email = InputEmail, password = InputPassword})
                        .ReceiveString();
                //TODO: figure out better implementation
                Console.WriteLine("@Debug: received " + User_Server.loggedinSessionID);
                success = true;
                Console.WriteLine("@Debug: Successful HTTP Request.");
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
                success = false;
                Message = ex.Message;
                Console.WriteLine("@Debug: " + ex.GetType() + " at HTTPLoginRequest(), " + ex.Message);
            }
            Console.WriteLine("@Debug: HTTPLoginRequest() returned "+ success);
            return success;
        }
        
        private async void OnLoginClicked(object obj)
        {
            if (ValidateUsernamePasswordFormat() && await HTTPLoginRequest())
            {
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(EventListPage)}");
            }
        }
    }
}