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
    public partial class EventSeatingPage : ContentPage
    {
        public EventSeatingPage(EventSeatingViewModel eventSeatingViewModel)
        {
            InitializeComponent();
            BindingContext = eventSeatingViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as EventSeatingViewModel).HTTPGetSeatingList();
            CreateSeatingChartTable();
        }

        private void CreateSeatingChartTable()
        {
            //Create Grid Rows
            for (int i = 0; i < (BindingContext as EventSeatingViewModel).EventNumRows+1; i++)
            {
                RowDefinition rowdef = new RowDefinition();
                EventSeatingGrid.RowDefinitions.Add(rowdef);
            }

            //Create Grid Columns
            for (int j = 0; j < (BindingContext as EventSeatingViewModel).EventNumCols+1; j++)
            {
                ColumnDefinition coldef = new ColumnDefinition();
                EventSeatingGrid.ColumnDefinitions.Add(coldef);
            }
            
            //Populate Grid with Checkboxes
            for (int col = 0; col < (BindingContext as EventSeatingViewModel).EventNumCols+1; col++)
            {
                for (int row = 0; row < (BindingContext as EventSeatingViewModel).EventNumRows+1; row++)
                {
                    if (row == 0 && col == 0)
                    {
                        //do nothing (skip)
                    }
                    else if (col == 0)
                    {
                        //Row Labels 
                        EventSeatingGrid.Children.Add(new Label()
                        {
                            Text = ((char)(row+64)).ToString(),
                        },col,row);
                    }
                    else if (row == 0)
                    {
                        //Column Labels
                        EventSeatingGrid.Children.Add( new Label()
                        {
                            Text = col.ToString(),
                        },col,row);
                    }
                    else
                    {
                        //Create Checkbox
                        CheckBox myCheckBox = new CheckBox();
                        myCheckBox.IsEnabled = !((BindingContext as EventSeatingViewModel).SeatingChart[row - 1, col - 1]
                            .status_reserved || (BindingContext as EventSeatingViewModel).SeatingChart[row - 1, col - 1].status_held );
                        myCheckBox.Color = (myCheckBox.IsEnabled == false) ? Color.Red : Color.Blue;
                        myCheckBox.Opacity = (myCheckBox.IsEnabled == false) ? 0.25 : 1.0;
                        myCheckBox.BindingContext = (BindingContext as EventSeatingViewModel).SeatingChart[row - 1, col - 1];
                        myCheckBox.SetBinding(CheckBox.IsCheckedProperty, "is_selected_by_user");
                        EventSeatingGrid.Children.Add(myCheckBox, col, row);
                    }
                }
            }
            EventSeatingPageLayout.Children.Add(EventSeatingGrid);
        }

        private async void CheckOutButton_OnClicked(object sender, EventArgs e)
        {
            await (BindingContext as EventSeatingViewModel).HTTPPutSeatingReservation();
            /*
            for (int i = 0; i < (BindingContext as EventSeatingViewModel).SeatingChart.GetLength(0); i++)
            {
                for (int j = 0; j < (BindingContext as EventSeatingViewModel).SeatingChart.GetLength(1); j++)
                {
                    Console.WriteLine($"!@Debug!: Seat {i}{j} is {(BindingContext as EventSeatingViewModel).SeatingChart[i,j].is_selected_by_user}");
                }
            }
            */
        }
    }
}