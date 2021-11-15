using System;
using TicketingAppProject.Models;
using TicketingAppProject.Views;
using Xamarin.Forms;

namespace TicketingAppProject.ViewModels
{
    public class LoginViewModelM: BaseViewModel
    {
        public LoginViewModelM(Login_Client myLoginClient)
        {
            this.MyLoginClient = myLoginClient;
            InputEmail = myLoginClient.loginEmail;
            InputPassword = myLoginClient.loginPassword;
            LoginCommand = new Command(OnLoginClicked);
        }

        private Login_Client MyLoginClient;
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

            return isValid;
        }
        
        private async void OnLoginClicked(object obj)
        {
            if (ValidateUsernamePasswordFormat())
            {
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
            }
        }
    }
}