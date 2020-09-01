using SQLite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Internals;

namespace City_Drifter
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]


    public class Location
    {
        public string DisplayName { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public string tag = "MainPage";
        public Boolean isTab = false;
        ObservableCollection<Location> locations = new ObservableCollection<Location>();
        public ObservableCollection<Location> Locations{ get { return locations; } }

        static LocationDatabase database;
        public static LocationDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new LocationDatabase();
                }
                return database;
            }
        }

        private int _colWidth= 0;

        public int ColWidth
        {
            get => _colWidth;
            set
            {
                _colWidth= value;
                OnPropertyChanged();
            }
        }


        public MainPage()
        {
            InitializeComponent();            
            BindingContext = this;
            //Xamarin.FormsMaps.Init("INSERT_AUTHENTICATION_TOKEN_HERE");
            LocationView.ItemsSource = locations;

            //selectionView.ItemSource = new[]
            //{
            //   "Walking","Driving"
            //};

            // ObservableCollection allows items to be added after ItemsSource
            // is set and the UI will react to changes
            locations.Add(new Location{ DisplayName = "Madera" });
            locations.Add(new Location { DisplayName = "Sanger" });
            locations.Add(new Location { DisplayName = "Reedley" });
            locations.Add(new Location { DisplayName = "Merced" });
            locations.Add(new Location { DisplayName = "Kingsburg" });
            locations.Add(new Location { DisplayName = "Hanford" });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }
        public void ShowOptionsTab(object sender, EventArgs e)
        {
            isTab = !isTab;
            Log.Warning(tag, "ShowOptionsTab called, isTab = " + isTab);
            if (isTab)
            {                
                ColWidth = 200;
            }
            else
            {
                ColWidth = 1;
            }
            Log.Warning(tag, "ShowOptionsTab called, SET ColWidth = " + ColWidth);
        }
    }
}