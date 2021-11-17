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
    public partial class UserProfilePage : ContentPage
    {
        private User_Server myUserProfile;
        
        public UserProfilePage()
        {
            InitializeComponent();
            if (myUserProfile == null) { myUserProfile = new User_Server(); }
            this.BindingContext = new UserProfileViewModel(myUserProfile);
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as UserProfileViewModel).HTTPGetUser();
        }
    }
}