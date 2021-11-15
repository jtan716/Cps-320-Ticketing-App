using System.ComponentModel;
using TicketingAppProject.ViewModels;
using Xamarin.Forms;

namespace TicketingAppProject.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}