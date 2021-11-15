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
        }
    }
}