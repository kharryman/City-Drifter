using SQLite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Essentials;
using Plugin.Geolocator;
using System.Threading.Tasks;
using System.Diagnostics;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Markup;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps;
using Position = Xamarin.Forms.GoogleMaps.Position;
using System.Linq;
using TouchTracking;

namespace City_Drifter
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]


    public class MyLocation
    {
        public string DisplayName { get; set; }
    }



    public partial class MainPage : ContentPage
    {
        public string tag = "MainPage";
        public Boolean isTab = false;
        ObservableCollection<MyLocation> locations = new ObservableCollection<MyLocation>();
        public ObservableCollection<MyLocation> Locations { get { return locations; } }

        static LocationDatabase database;

        private double overlayWidth = 0.0;
        private double overlayHeight = 0.0;
        Pin mySelf;

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

        public bool isSetIcon { get; set; }
        public bool isGettingAddress { get; set; }

        public string currentCountry{ get; set; }
        public string currentState { get; set; }
        public string currentCity{ get; set; }
        public Plugin.Geolocator.Abstractions.Position oldPosition {get; set;}

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

        bool isBeingDragged;
        long touchId;
        Point pressPoint;


        public MainPage()
        {
            InitializeComponent();
            //Compass.ReadingChanged += (s, e) => PointerImage.RotateTo(e.Reading.HeadingMagneticNorth, 200);
            //Compass.Start(SensorSpeed.UI, applyLowPassFilter: true);
            //LoadApplication(new screensize.App());
            isSetIcon = false;
            isGettingAddress = false;
            overlay.IsVisible = false;
            oldPosition = null;
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
            locations.Add(new MyLocation { DisplayName = "Madera" });
            locations.Add(new MyLocation { DisplayName = "Sanger" });
            locations.Add(new MyLocation { DisplayName = "Reedley" });
            locations.Add(new MyLocation { DisplayName = "Merced" });
            locations.Add(new MyLocation { DisplayName = "Kingsburg" });
            locations.Add(new MyLocation { DisplayName = "Hanford" });


            if (IsLocationAvailable())
            {
                mySelf = new Pin()
                {
                    Label = "ME!"
                };
                map.Pins.Add(mySelf);
                _ = StartListening();
                TouchEffect touchEffect = new TouchEffect();
                touchEffect.TouchAction += OnTouchEffectAction;
                map.Effects.Add(touchEffect);
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


            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(10), 10, true);

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
            //Debug.WriteLine(output);
            mySelf.Position = new Position(position.Latitude, position.Longitude);
            mySelf.Rotation = (float)position.Heading;
            if (!isSetIcon)
            {
                mySelf.Icon = BitmapDescriptorFactory.FromView(new BindingPinView(""));
                isSetIcon = true;
            }
            map.MoveToRegion(MapSpan.FromCenterAndRadius(mySelf.Position, Distance.FromMiles(1)));
            if (oldPosition == null)
            {
                oldPosition = position;
            }
            else
            {
                if (position != null)
                {
                    Console.WriteLine($"CHECKING DISTANCE !!!");
                    Location sourceCoordinates = new Location(oldPosition.Latitude, oldPosition.Longitude);
                    Location destinationCoordinates = new Location(position.Latitude, position.Longitude);
                    double distance = Location.CalculateDistance(sourceCoordinates, destinationCoordinates, DistanceUnits.Kilometers);
                    double distanceThreshold = walkingSwitch.IsToggled == false ? 0.005 : 0.100;
                    Console.WriteLine($"Distance = " + distance);
                    Random rg = new Random();
                    int myR = rg.Next(0, 10);
                    if (distance >= distanceThreshold || myR > 7)//IF MORE THAN 250 METERS: SAVE TO DATABASE:
                    {
                        Console.WriteLine($"ADDING POLYLINE !!!");
                        var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                        polyline.StrokeColor = Color.Blue;
                        polyline.StrokeWidth = 2f;
                        // APP_DEBUG X : 
                        var myRLat = .005 + ((rg.Next(-5, 5)) * 0.01);
                        var myRLng = .005 + ((rg.Next(-5, 5)) * 0.01);
                        position.Latitude += myRLat;
                        position.Longitude += myRLng;
                        // 
                        polyline.Positions.Add(new Position(oldPosition.Latitude , oldPosition.Longitude));
                        polyline.Positions.Add(new Position(position.Latitude, position.Longitude));
                        map.Polylines.Add(polyline);
                        Console.WriteLine($"ADDED POLYLINE !!!");
                        oldPosition = position;
                        if (!isGettingAddress)
                        {
                            isGettingAddress = true;
                            getAddress(position);
                        }
                        Console.WriteLine("Saving LocationItem into database");
                    }
                }
                else
                {
                    Console.WriteLine("oldPosition IS NULL");
                }
            }            
            
        }

        async private void getAddress(Plugin.Geolocator.Abstractions.Position position)
        {
            try
            {
                var lat = position.Latitude;
                var lon = position.Longitude;

                var placemarks = await Geocoding.GetPlacemarksAsync(lat, lon);

                var placemark = placemarks?.FirstOrDefault();
                if (placemark != null)
                {
                    var geocodeAddress =
                        $"FeatureName:     {placemark.FeatureName}, " +
                        $"Locality:        {placemark.Locality}, " +
                        $"AdminArea:       {placemark.AdminArea}, " +
                        $"CountryName:     {placemark.CountryName}.";
                    //$"CountryCode:     {placemark.CountryCode}\n" +                        


                    //$"PostalCode:      {placemark.PostalCode}\n" +
                    //$"SubAdminArea:    {placemark.SubAdminArea}\n" +
                    //$"SubLocality:     {placemark.SubLocality}\n" +
                    //$"SubThoroughfare: {placemark.SubThoroughfare}\n" +
                    //$"Thoroughfare:    {placemark.Thoroughfare}\n";

                    Console.WriteLine(geocodeAddress);
                    String travelMode = walkingSwitch.IsToggled == false ? "WALKING" : "DRIVING";
                    Console.WriteLine($"travelMode = " + travelMode);

                    //GET AND PLOT POINTS IF COUNTRY, STATE, AND CITY CHANGE:
                    if (position != null && currentCountry != placemark.CountryName && currentState != placemark.AdminArea && currentCity != placemark.Locality)
                    {
                        currentCountry = placemark.CountryName;
                        currentState = placemark.AdminArea;
                        currentCity = placemark.Locality;
                        oldPosition = position;
                        Console.WriteLine($"SET oldPosition = " + oldPosition.Latitude + ", " + oldPosition.Longitude);
                        addDatabasePolylines(placemark.CountryName, placemark.AdminArea, placemark.Locality, travelMode);
                    }
                    isGettingAddress = false;
                    var myDatabaseLocation = new LocationItem { Travel_Mode = travelMode, Country = placemark.CountryName, State = placemark.AdminArea, City = placemark.Locality, Latitude = position.Latitude, Longitude = position.Longitude };
                    Database.SaveItemAsync(myDatabaseLocation);
                }//PLACEMARK IS NULL!:
                else
                {
                    isGettingAddress = false;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
                isGettingAddress = false;
            }
            catch (Exception ex)
            {
                // Handle exception that may have occurred in geocoding
                isGettingAddress = false;
            }
        }

        void OnTravelModeToggled(object sender, ToggledEventArgs e)
        {
            String travelMode = walkingSwitch.IsToggled == false ? "WALKING" : "DRIVING";
            Console.WriteLine($"OnTravelModeToggled travelMode = " + travelMode);
            //GET AND PLOT POINTS IF COUNTRY, STATE, AND CITY CHANGE:
            if (currentCountry != null && currentState != null && currentCity != null)
            {
                Console.WriteLine($"OnTravelModeToggled calling addDatabasePolylines !!!");
                map.Polylines.Clear();
                addDatabasePolylines(currentCountry, currentState, currentCity, travelMode);
            }
        }


        private void addDatabasePolylines(String country, String state, String city, String travelMode)
        {
            var travelledLocations = Database.GetRoadsDone(country, state, city, travelMode);
            Console.WriteLine($"addDatabasePolylines FOUND " + travelledLocations.Result.Count + " LINES MOVED.");
            if (travelledLocations.Result.Count > 0)
            {
                var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                polyline.StrokeColor = Color.Blue;
                polyline.StrokeWidth = 2f;
                for (var i = 0; i < travelledLocations.Result.Count; i++)
                {
                    Console.WriteLine($"ADDING POLYLINE LATITUDE = " + travelledLocations.Result[i].Latitude + $", LONGITUDE = " + travelledLocations.Result[i].Longitude);
                    polyline.Positions.Add(new Position(travelledLocations.Result[i].Latitude, travelledLocations.Result[i].Longitude));
                }
                map.Polylines.Add(polyline);
                Console.WriteLine($"addDatabasePolylines ADDED DATABASE POLYLINES!!!");
            }
        }

        private void PositionError(object sender, PositionErrorEventArgs e)

        {
            Debug.WriteLine(e.Error);
            //Handle event here for errors
        }

       async Task StopListening()
       {
            if (!CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StopListeningAsync();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
        }

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
                //TODO: GET LOCATIONS(COUNTRY, STATE, CITY)
                var locationsVistited = Database.GetLocationsVisited();
                locations.Clear();
                Console.WriteLine("locationsVisited LENGTH = " + locationsVistited.Result.Count);
                for (var i=0;i< locationsVistited.Result.Count; i++)
                {
                    locations.Add(new MyLocation { DisplayName = "Country: " + locationsVistited.Result[i].Country + ", Stagte: " + locationsVistited.Result[i].State + ", City: " + locationsVistited.Result[i].City });
                }
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

        async void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            Console.WriteLine("OnTouchEffectAction CALLED!!");
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    //if (!isBeingDragged)
                    //{
                    //    isBeingDragged = true;
                    //    touchId = args.Id;
                        //pressPoint = args.Location;                        
                    //}
                    Console.WriteLine("Pressed ACTION");
                    //await StopListening();
                    break;

                case TouchActionType.Moved:
                    //if (isBeingDragged && touchId == args.Id)
                    //{
                    //    TranslationX += args.Location.X - pressPoint.X;
                    //    TranslationY += args.Location.Y - pressPoint.Y;
                    //}
                    Console.WriteLine("Moved ACTION");
                    break;

                case TouchActionType.Released:
                    //if (isBeingDragged && touchId == args.Id)
                    //{
                    //    isBeingDragged = false;
                    // }                    
                    Console.WriteLine("Released ACTION");
                    //await StartListening();
                    break;
            }
        }
    }

}