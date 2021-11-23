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
    public partial class EventListPage : ContentPage
    {
        private EventSeatingViewModel _selectedEventSeatingViewModel;

        public EventListPage()
        {
            InitializeComponent();
            BindingContext = new EventListViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as EventListViewModel).HTTPGetEventList();
        }
        
        private async void EventEntry_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            _selectedEventSeatingViewModel = e.Item as EventSeatingViewModel;
            var page = new EventSeatingPage(_selectedEventSeatingViewModel);
            _ = Shell.Current.Navigation.PushAsync(page);
        }
        
    }
}