using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingAppProject.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TicketingAppProject.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TicketListPage : ContentPage
    {
        public TicketListPage()
        {
            InitializeComponent();
            BindingContext = new TicketListViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as TicketListViewModel).HTTPGetUserTickets();
        }
    }
}