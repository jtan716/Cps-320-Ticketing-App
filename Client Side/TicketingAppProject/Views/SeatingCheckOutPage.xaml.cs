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
    public partial class SeatingCheckOutPage : ContentPage
    {
        public SeatingCheckOutPage(SeatingCheckOutViewModel seatingCheckOutViewModel) 
        {
            InitializeComponent();
            BindingContext = seatingCheckOutViewModel;
        }

        private async void PurchaseButton_OnClicked(object sender, EventArgs e)
        {
            bool success = await (BindingContext as SeatingCheckOutViewModel).HTTPReserveSeatsRequest();
            if (success)
            {
                await Shell.Current.Navigation.PopToRootAsync();
                await Shell.Current.GoToAsync($"//{nameof(TicketListPage)}");
            }
        }
    }
}