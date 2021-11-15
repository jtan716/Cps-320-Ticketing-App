using System;
using System.Collections.Generic;
using System.ComponentModel;
using TicketingAppProject.Models;
using TicketingAppProject.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TicketingAppProject.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}