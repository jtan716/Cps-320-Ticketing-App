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
            SignupCommand = new Command(OnSignupClicked);
        }
        
        private Login_Client MyLoginClient;
        public Command SignupCommand { get;  }
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

        private string _inputSignupEmail;
        public string InputSignupEmail
        {
            get => _inputSignupEmail;
            set => SetProperty(ref _inputSignupEmail, value);
        }

        private string _inputSignupPassword;
        public string InputSignUpPassword
        {
            get => _inputSignupPassword;
            set => SetProperty(ref _inputSignupPassword, value);
        }

        private string _inputSignupPasswordConfirm;
        public string InputSignupPasswordConfirm
        {
            get => _inputSignupPasswordConfirm;
            set => SetProperty(ref _inputSignupPasswordConfirm, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _loginIsBusy;
        public bool LoginIsBusy
        {
            get => _loginIsBusy;
            set => SetProperty(ref _loginIsBusy, value);
        }

        public bool LoginNotBusy => !LoginIsBusy;

        private bool _signupIsBusy;
        public bool SignupIsBusy
        {
            get => _signupIsBusy;
            set => SetProperty(ref _signupIsBusy, value);
        }

        public bool SignupNotBusy => !SignupIsBusy;

        public string UserEmail => MyLoginClient.loginEmail;
        public string UserPassword => MyLoginClient.loginPassword;

        public string UserID => MyLoginClient.loginUserID;

        public bool ValidateLoginUsernamePasswordFormat()
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
        
        public bool ValidateSignupUsernamePasswordFormat()
        {
            //email username field must be non-empty and have the character '@'
            Message = "";
            bool isValid = true;

            if (String.IsNullOrEmpty(InputSignupEmail))
            {
                Message += "Please enter in an email in the Email Entry Field. \n";
                isValid = false;
            }
            else if (!InputSignupEmail.Contains("@"))
            {
                Message += "Please enter in an email (make sure to include the @ symbol). \n";
                isValid = false;
            }

            if (String.IsNullOrEmpty(InputSignUpPassword))
            {
                Message += "Please enter in a password in the Password Entry Field. \n";
                isValid = false;
            }
            else if (InputSignUpPassword != InputSignupPasswordConfirm)
            {
                Message += "Please make sure that password and confirmation password match.\n";
                isValid = false;
            }
            
            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(UserPassword));
            OnPropertyChanged(nameof(UserID));

            return isValid;
        }

        private async Task<bool> HTTPLoginOrSignupRequest(bool ForLogin)
        {
            Message = "";
            Console.WriteLine("@Debug: Executing HTTPLoginOrSignupRequest().");
            bool success = false;
            try
            {
                if (ForLogin)
                {
                    LoginIsBusy = true;
                    User_Server.loggedinSessionID =
                        await URL_Server.getURLLogin().PostJsonAsync(new {email = InputEmail, password = InputPassword})
                            .ReceiveString();
                    //TODO: figure out better implementation
                    Console.WriteLine("@Debug: received " + User_Server.loggedinSessionID);
                    success = true;
                    Console.WriteLine("@Debug: Successful HTTP Request.");
                    LoginIsBusy = false;
                }
                else
                {
                    SignupIsBusy = true;
                    await URL_Server.getURLSignup()
                        .PostJsonAsync(new {email = InputSignupEmail, password = InputSignUpPassword});
                    success = true;
                    Message = "Successfully created an account!";
                    SignupIsBusy = false;
                }
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
                Console.WriteLine("@Debug: " + ex.GetType() + " at HTTPLoginOrSignupRequest(), " + ex.Message);
            }
            Console.WriteLine("@Debug: HTTPLoginOrSignupRequest() returned "+ success);
            return success;
        }
        
        private async void OnLoginClicked(object obj)
        {
            if (ValidateLoginUsernamePasswordFormat() && await HTTPLoginOrSignupRequest(true))
            {
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(EventListPage)}");
            }
        }

        private async void OnSignupClicked(object obj)
        {
            if (ValidateSignupUsernamePasswordFormat() && await HTTPLoginOrSignupRequest(false))
            {
                //await Shell.Current.GoToAsync($"//{nameof(EventListPage)}");
            }
        }
    }
}