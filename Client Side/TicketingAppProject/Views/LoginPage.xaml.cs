using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingAppProject.Models;
using TicketingAppProject.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TicketingAppProject.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private Login_Client myLoginClient;
        public LoginPage()
        {
            InitializeComponent();
            if (myLoginClient == null) { myLoginClient = new Login_Client(); }
            this.BindingContext = new LoginViewModelM(myLoginClient);
            SignupContent.IsVisible = false;
            BtnLoginClicked.Opacity = 0.25;
        }

        private void BtnLoginClicked_OnClicked(object sender, EventArgs e)
        {
            SignupContent.IsVisible = false;
            LoginContent.IsVisible = true;
            BtnLoginClicked.Opacity = 1.0;
            BtnSignupClicked.Opacity = 0.5;
            (BindingContext as LoginViewModelM).Message = "";
        }

        private void BtnSignupClicked_OnClicked(object sender, EventArgs e)
        {
            LoginContent.IsVisible = false;
            SignupContent.IsVisible = true;
            BtnSignupClicked.Opacity = 1.0;
            BtnLoginClicked.Opacity = 0.5;
            (BindingContext as LoginViewModelM).Message = "";
        }
    }
}