using System;
using TicketingAppProject.Services;
using TicketingAppProject.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TicketingAppProject
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            //DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
