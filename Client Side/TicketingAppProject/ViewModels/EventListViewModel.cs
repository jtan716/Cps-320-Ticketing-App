using System;
using System.Collections.Generic;
using Flurl.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using TicketingAppProject.Models;
using TicketingAppProject.Services;
using TicketingAppProject.Views;
using Xamarin.Forms;

namespace TicketingAppProject.ViewModels
{
    public class EventListViewModel: BaseViewModel
    {
        public MappedObservableCollection<Event_Server, EventSeatingViewModel> MappedEventCollection { get; }
        
        public ICommand RefreshCommand { get;  }

        public EventListViewModel()
        {
            MappedEventCollection = new MappedObservableCollection<Event_Server, EventSeatingViewModel>(
                myevent => new EventSeatingViewModel(myevent),
                EventCollection.Instance.Events);
            RefreshCommand = new Command(ExecuteRefreshCommand);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        async void ExecuteRefreshCommand()
        {
            await HTTPGetEventList();
            IsRefreshing = false;
        }

        public async Task HTTPGetEventList()
        {
            try
            {
                List<Event_Server> events = await URL_Server.getURLEventList().GetJsonAsync<List<Event_Server>>();
                EventCollection.Instance.Events.Clear();
                foreach (Event_Server myevent in events)
                {
                    EventCollection.Instance.AddEvent(myevent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"@Debug!: {ex.GetType()} Error at HTTPGetEventList() : {ex.Message}");
            }
        }

    }
}