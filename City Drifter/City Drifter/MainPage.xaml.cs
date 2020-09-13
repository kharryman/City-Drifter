using SQLite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Internals;
using Xamarin.Essentials;
using Plugin.Geolocator;
using System.Threading.Tasks;
using System.Diagnostics;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Markup;
using Position = Xamarin.Forms.Maps.Position;
using System.Collections.Generic;

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

        private double overlayWidth = 0.0;
        private double overlayHeight= 0.0;
        //public CustomMap customMap;
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

        private bool isTabVisible = false;
        private bool isLocationTab = false;
        public bool IsTabVisible
        {
            get => isTabVisible;
            set
            {
                isTabVisible = value;
                OnPropertyChanged();
            }
        }

        private Xamarin.Forms.Rectangle myRect = new Xamarin.Forms.Rectangle(0, 0, 0.5, 1.0);

        public Xamarin.Forms.Rectangle rect
        {
            get => myRect;
            set
            {
                myRect = value;
                OnPropertyChanged();
            }
        }

        CustomPin customPin;


        public MainPage()
        {
            InitializeComponent();
            //Compass.ReadingChanged += (s, e) => PointerImage.RotateTo(e.Reading.HeadingMagneticNorth, 200);
            //Compass.Start(SensorSpeed.UI, applyLowPassFilter: true);
            //LoadApplication(new screensize.App());
            overlay.IsVisible = false;
            rect = new Xamarin.Forms.Rectangle(0, 0, 0.0, 0.0);

            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            overlayWidth = 200;// mainDisplayInfo.Width * 0.25;
            overlayHeight = mainDisplayInfo.Height;
            //var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            //overlayWidth = overlay.Width;
            //overlayHeight = overlay.Height;
            Log.Warning(tag, "overlayWidth=" + overlayWidth, ", overlayHeight=" + overlayHeight);
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

            customPin = new CustomPin
            {
                Type = PinType.Place,
                Position = new Position(37.79752, -122.40183),
                Label = "Xamarin San Francisco Office",
                Address = "394 Pacific Ave, San Francisco CA",
                Name = "Xamarin",
                Url = "http://xamarin.com/about/"
            };
            customMap = new CustomMap();
            customMap.CustomPins = new List<CustomPin> { customPin };

            if (IsLocationAvailable())
            {
                _ = StartListening();
            }
        }

        async private Task loadMap()
        {
            //var locator = CrossGeolocator.Current;
            //TimeSpan getPositionTimespan = new TimeSpan(5000);
            //var position = await locator.GetPositionAsync(getPositionTimespan);

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    var zoomLevel = 9; // between 1 and 18
                    var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));

                    customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(location.Latitude, location.Longitude), Distance.FromMiles(1)));
                    //map.MoveToRegion(new MapSpan(map.VisibleRegion.Center, latlongdegrees, latlongdegrees));
                    
                    var timeoutTask = Task.Delay(3);
                    await timeoutTask;
                    await loadMap();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"Feature not supported");
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Console.WriteLine($"Feature not enabled");
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine($"Permission exception");
            }
            catch (Exception ex)
            {
                // Unable to get location
                Console.WriteLine($"Unable to get location");
                var timeoutTask = Task.Delay(3);
                await timeoutTask;
                await loadMap();
            }

        }

        public bool IsLocationAvailable()
        {
            if (!CrossGeolocator.IsSupported)
                return false;

            return CrossGeolocator.Current.IsGeolocationAvailable;
        }


        async Task StartListening()
        {
            if (CrossGeolocator.Current.IsListening)
                return;


            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true);

            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;
            //var markerImg = BitmapDescriptorFactory.FromResource(Resource.Drawable.dot);


        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {

            //If updating the UI, ensure you invoke on main thread
            var position = e.Position;
            var output = "Full: Lat: " + position.Latitude + " Long: " + position.Longitude;
            output += "\n" + $"Time: {position.Timestamp}";
            output += "\n" + $"Heading: {position.Heading}";
            output += "\n" + $"Speed: {position.Speed}";
            output += "\n" + $"Accuracy: {position.Accuracy}";
            output += "\n" + $"Altitude: {position.Altitude}";
            output += "\n" + $"Altitude Accuracy: {position.AltitudeAccuracy}";
            var newPosition = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(newPosition, Distance.FromMiles(1)));
            customPin.Position = newPosition;
            //map.RotateTo(position.Heading);
            Debug.WriteLine(output);
        }

        private void PositionError(object sender, PositionErrorEventArgs e)

        {
            Debug.WriteLine(e.Error);
            //Handle event here for errors
        }

        //async Task StopListening()
       // {
       //     if (!CrossGeolocator.Current.IsListening)
       //         return;

            //await CrossGeolocator.Current.StopListening();

            //CrossGeolocator.Current.PositionChanged -= PositionChanged;
            //CrossGeolocator.Current.PositionError -= PositionError;
        //}

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //_ = loadMap();
        }
        async public void ShowOptionsTab(object sender, EventArgs e)
        {
            //isTab = !isTab;            
            isLocationTab = !isLocationTab;
            Log.Warning(tag, "ShowOptionsTab called");
            if (isLocationTab)
            {
                //IsTabVisible = true;
                overlay.IsVisible = true;
                Log.Warning(tag, "overlayWidth=" + overlayWidth + ", overlayHeight=" + overlayHeight);
                rect = new Xamarin.Forms.Rectangle(0, 0, 0.0, 0.0);
                //rect = new Xamarin.Forms.Rectangle(0, 0, 0.5, 1.0);
                await overlay.LayoutTo(new Rectangle(0, 0, overlayWidth, overlayHeight), 250, Easing.Linear);
                //await overlay.LayoutTo(new Rectangle(0, 0, 0.5, 1.0), 1000, Easing.Linear);
                Log.Warning(tag, "Setiting rect to half width");                
            }
            else
            {
                Log.Warning(tag, "overlayWidth=" + overlayWidth + ", overlayHeight=" + overlayHeight);
                rect = new Xamarin.Forms.Rectangle(0, 0, 0.5, 1.0);
                //rect = new Xamarin.Forms.Rectangle(0, 0, 0.0, 0.0);
                await overlay.LayoutTo(new Rectangle(0, 0, 0, overlayHeight), 250, Easing.Linear);
                overlay.IsVisible = false;
                //IsTabVisible = false;
                //rect = new Xamarin.Forms.Rectangle(0, 0, 0.0, 0.0);
                Log.Warning(tag, "Setiting rect to 0 width");
            }
        }
    }
}