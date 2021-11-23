using System;
using System.Collections.Generic;
using TicketingAppProject.ViewModels;
using TicketingAppProject.Views;
using Xamarin.Forms;

namespace TicketingAppProject
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EventListPage), typeof(EventListPage));
            Routing.RegisterRoute(nameof(UserProfilePage),typeof(UserProfilePage));
            Routing.RegisterRoute(nameof(TicketListPage), typeof(TicketListPage));
            Routing.RegisterRoute(nameof(EventSeatingPage), typeof(EventSeatingPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
