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
using System.Globalization;

namespace City_Drifter
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]

    public class ListviewDropdown
    {
        public string displayText { get; set; } // Text to display in ListView
        public bool isSelected { get; set; } // Flag, if item is selected or not
        public int iIndex { get; set; }  // Own index to handle the entrys -> see code
        public string selectedIcon { get; set; } // Name of the Icon
    }

    public class ListviewLocationDropdown
    {
        public int ID { get; set; } // Row ID
        public string displayText { get; set; } // Text to display in ListView
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public bool isSelected { get; set; } // Flag, if item is selected or not
        public int iIndex { get; set; }  // Own index to handle the entrys -> see code
        public string selectedIcon { get; set; } // Name of the Icon
    }

    class ListViewCell : ViewCell
    {
        public ListViewCell() // Spezial-definition 
        {
            var myLabel = new Label();
            myLabel.SetBinding(Label.TextProperty, "displayText");
            myLabel.VerticalOptions = LayoutOptions.Center;
            myLabel.BindingContextChanged += (sender, e) =>
            {
            };
            var myIcon = new Image();
            myIcon.SetBinding(Image.SourceProperty, new Binding("selectedIcon", BindingMode.OneWay, new StringToImageConverter()));
            //                 
            // pattform-specific settings -> depends on Icon -> not yet finished
            switch (Device.RuntimePlatform)
            {
                case "WinPhone":
                    myIcon.VerticalOptions = LayoutOptions.Center;
                    myLabel.Font = Font.SystemFontOfSize(30);
                    break;
                case "iOS":
                    myIcon.HeightRequest = 15;
                    myIcon.VerticalOptions = LayoutOptions.Center;
                    break;
                case "Android":
                    myIcon.HeightRequest = 15;
                    myIcon.VerticalOptions = LayoutOptions.Center;
                    break;

            }
            var s = new StackLayout();
            s.Orientation = StackOrientation.Horizontal; // Element horizontal anordnen
                                                         //var s = new TableView();
            s.Children.Add(myIcon);
            s.Children.Add(myLabel);
            this.View = s;
        }
    }

    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var filename = (string)value;
            return ImageSource.FromFile(filename);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public partial class MainPage : ContentPage
    {
        public string tag = "MainPage";
        public Boolean isTab = false;
        //ObservableCollection<String> locations = new ObservableCollection<String>();
        //public ObservableCollection<String> Locations { get { return locations; } }

        ObservableCollection<String> provinces = new ObservableCollection<String>();
        //public ObservableCollection<String> Provinces { get { return provinces; } }

        List<City_Drifter.ListviewDropdown> statesDropdownlist;
        List<City_Drifter.ListviewLocationDropdown> locationsDropdownlist;

        City_Drifter.ListviewLocationDropdown selectedShowLocation;

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

        public int mySelectedLocationID { get; set; }

        //public strin
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

        public bool isRandomMovements = false;


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
            mySelectedLocationID = -1;
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
            //LocationView.ItemsSource = locations;            

            //selectionView.ItemSource = new[]
            //{
            //   "Walking","Driving"
            //};

            // ObservableCollection allows items to be added after ItemsSource
            // is set and the UI will react to changes

            //ProvinceView.ItemsSource = provinces;
            //provinces.Add("California");
            //provinces.Add("Nevada");
            //provinces.Add("Oregon");

            statesDropdownlist = new List<City_Drifter.ListviewDropdown>();
            locationsDropdownlist = new List<City_Drifter.ListviewLocationDropdown>();
            //
            SelectedProvinceButton.Text = "California ▼";
            var province = new City_Drifter.ListviewDropdown() { displayText = "California", iIndex = 0, isSelected = true };
            statesDropdownlist.Add(province);
            province = new City_Drifter.ListviewDropdown() { displayText = "Nevada", iIndex = 1, isSelected = false };
            statesDropdownlist.Add(province);
            province = new City_Drifter.ListviewDropdown() { displayText = "Oregon", iIndex = 2, isSelected = false };
            statesDropdownlist.Add(province);



            if (IsLocationAvailable())
            {
                mySelf = new Pin()
                {
                    Label = "ME!"
                };
                map.Pins.Add(mySelf);
                //ADD LOCATIONS:                

                _ = StartListening();
                TouchEffect touchEffect = new TouchEffect();
                touchEffect.TouchAction += OnTouchEffectAction;
                map.Effects.Add(touchEffect);
            }

            //ProvinceView.ItemsSource.
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
                    double distanceThreshold = walkingSwitch.IsToggled == false ? 0.020 : 0.100;
                    Console.WriteLine($"Distance = " + distance);
                    // APP_DEBUG -----------:
                    int myR = 10;
                    if (isRandomMovements == true)
                    {
                        Random rg = new Random();
                        myR = rg.Next(0, 10);
                    }
                    Console.WriteLine($"myR = " + myR);
                    //-------------------------
                    if (distance >= distanceThreshold || myR>7)//IF MORE THAN 250 METERS: SAVE TO DATABASE:
                    {
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
                    Console.WriteLine($"ADDING POLYLINE !!!");
                    var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                    polyline.StrokeColor = Color.Blue;
                    polyline.StrokeWidth = 2f;
                    if (isRandomMovements == true)
                    {
                        Random rg = new Random();
                        var myRLat = .005 + ((rg.Next(-5, 5)) * 0.01);
                        var myRLng = .005 + ((rg.Next(-5, 5)) * 0.01);
                        position.Latitude += myRLat;
                        position.Longitude += myRLng;
                    }
                    // 
                    polyline.Positions.Add(new Position(oldPosition.Latitude, oldPosition.Longitude));
                    polyline.Positions.Add(new Position(position.Latitude, position.Longitude));
                    map.Polylines.Add(polyline);
                    Console.WriteLine($"ADDED POLYLINE !!!");
                    oldPosition = position;
                    var myDatabaseLocation = new LocationItem { Travel_Mode = travelMode, Country = placemark.CountryName, State = placemark.AdminArea, City = placemark.Locality, Latitude = position.Latitude, Longitude = position.Longitude };
                    await Database.SaveItemAsync(myDatabaseLocation);
                    isGettingAddress = false;
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
                var lastPosition = new Position(travelledLocations.Result[0].Latitude, travelledLocations.Result[0].Longitude);
                var newPosition = new Position();
                for (var i = 1; i < travelledLocations.Result.Count; i++)
                {
                    polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                    polyline.StrokeColor = Color.Blue;
                    polyline.StrokeWidth = 2f;
                    polyline.Positions.Add(lastPosition);
                    newPosition = new Position(travelledLocations.Result[i].Latitude, travelledLocations.Result[i].Longitude);
                    Console.WriteLine($"ADDING POLYLINE LATITUDE = " + travelledLocations.Result[i].Latitude + $", LONGITUDE = " + travelledLocations.Result[i].Longitude);
                    polyline.Positions.Add(newPosition);
                    lastPosition = newPosition;
                    map.Polylines.Add(polyline);
                }                
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



        public void showDownloadDropdown(object sender, EventArgs e)
        {
            Console.WriteLine("showDownloadDropdown called");
            downloadListviewLayout.IsVisible = true;
            downloadListview.ItemsSource = statesDropdownlist;
            downloadListview.ItemTemplate = new DataTemplate(typeof(ListViewCell)); // Update page
            downloadListview.ItemTapped += async (listviewSender, listviewEventArgs) =>
            {
                var LVElement = (City_Drifter.ListviewDropdown)listviewEventArgs.Item;
                if (LVElement.isSelected) // Item is selected already
                {
                    statesDropdownlist[LVElement.iIndex].isSelected = false;
                }
                else
                {
                    statesDropdownlist[LVElement.iIndex].isSelected = true;
                    SelectedProvinceButton.Text = LVElement.displayText + " ▼";
                    // For iOS -> black check-icon, for Android and WP -> white check-icon
                    if (Device.RuntimePlatform.Equals("iOS")) { statesDropdownlist[LVElement.iIndex].selectedIcon = "CheckBlack.png"; } else { statesDropdownlist[LVElement.iIndex].selectedIcon = "CheckWhite.png"; }
                }
                foreach (City_Drifter.ListviewDropdown dropdownItemLooped in statesDropdownlist)
                {
                    if (dropdownItemLooped.iIndex != LVElement.iIndex)
                    {
                        dropdownItemLooped.isSelected = false;
                        dropdownItemLooped.selectedIcon = "";
                    }
                }
                downloadListview.ItemTemplate = new DataTemplate(typeof(ListViewCell)); // Update Page
                await Task.Delay(500);
                downloadListviewLayout.IsVisible = false;
                //await Task.Delay(200);
                isLocationTab = false;
                toggleShowOptionsTab();
                //overlay.IsVisible = true;
            };
        }

        public void showLocationsDropdown(object sender, EventArgs e)
        {
            Console.WriteLine("showLocationsDropdowncalled");
            locationsListviewLayout.IsVisible = true;
            locationsListview.ItemsSource = locationsDropdownlist;
            locationsListview.ItemTemplate = new DataTemplate(typeof(ListViewCell)); // Update page
            locationsListview.ItemTapped += async (listviewSender, listviewEventArgs) =>
            {
                var LVElement = (City_Drifter.ListviewLocationDropdown)listviewEventArgs.Item;
                if (LVElement.isSelected) // Item is selected already
                {
                    locationsDropdownlist[LVElement.iIndex].isSelected = false;
                }
                else
                {
                    selectedShowLocation = locationsDropdownlist[LVElement.iIndex];
                    locationsDropdownlist[LVElement.iIndex].isSelected = true;
                    mySelectedLocationID = LVElement.ID;
                    Console.WriteLine($"mySelectedLocationID = " + mySelectedLocationID);
                    SelectedLocationButton.Text = LVElement.displayText + " ▼";
                    // For iOS -> black check-icon, for Android and WP -> white check-icon
                    if (Device.RuntimePlatform.Equals("iOS")) { locationsDropdownlist[LVElement.iIndex].selectedIcon = "CheckBlack.png"; } else { locationsDropdownlist[LVElement.iIndex].selectedIcon = "CheckWhite.png"; }
                }
                foreach (City_Drifter.ListviewLocationDropdown dropdownItemLooped in locationsDropdownlist)
                {
                    if (dropdownItemLooped.iIndex != LVElement.iIndex)
                    {
                        dropdownItemLooped.isSelected = false;
                        dropdownItemLooped.selectedIcon = "";
                    }
                }
                locationsListview.ItemTemplate = new DataTemplate(typeof(ListViewCell)); // Update Page
                await Task.Delay(500);
                locationsListviewLayout.IsVisible = false;
                //await Task.Delay(200);
                isLocationTab = false;
                toggleShowOptionsTab();
                //overlay.IsVisible = true;
            };
        }


        async public void DownloadProvince(object sender, EventArgs e)
        {
            Console.WriteLine($"DownloadProvince called.");
            toggleShowOptionsTab();
        }

        async public void ShowRoads(object sender, EventArgs e)
        {
            Console.WriteLine($"ShowRoads called.");
            //selectedShowLocation            
            //TO DO : 1) Stop listening, 2) Remove position of your marker 3) Show map, bounds in polylines, 4) draw polylines:
            await StopListening();
            //2:
            mySelf.Position = new Position(0, 0);
            //3:
            toggleShowOptionsTab();
            showRoadMap();
        }

        public void showRoadMap()
        {
            Console.WriteLine($"showRoadMap called");
            //LatLng marker1LatLng = new LatLng(marker1lat, marker1lng);
            //LatLngBounds.Builder b = new LatLngBounds.Builder().Include(marker1LatLng);
            //map.MoveCamera(CameraUpdateFactory.NewLatLngBounds(b.Build(), 120));
            map.Polylines.Clear();
            String travelMode = walkingSwitch.IsToggled == false ? "WALKING" : "DRIVING";
            var travelledLocations = Database.GetRoadsDone(selectedShowLocation.Country, selectedShowLocation.State, selectedShowLocation.City, travelMode);
            Console.WriteLine($"addDatabasePolylines FOUND " + travelledLocations.Result.Count + " LINES MOVED.");
            if (travelledLocations.Result.Count > 0)
            {
                var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                var lastPosition = new Position(travelledLocations.Result[0].Latitude, travelledLocations.Result[0].Longitude);
                var newPosition = new Position();
                var mostWest = travelledLocations.Result[0].Longitude;
                var mostEast = travelledLocations.Result[0].Longitude;
                var mostNorth = travelledLocations.Result[0].Latitude;
                var mostSouth = travelledLocations.Result[0].Latitude;
                for (var i = 1; i < travelledLocations.Result.Count; i++)
                {
                    polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                    polyline.StrokeColor = Color.Blue;
                    polyline.StrokeWidth = 2f;
                    polyline.Positions.Add(lastPosition);
                    newPosition = new Position(travelledLocations.Result[i].Latitude, travelledLocations.Result[i].Longitude);
                    if (travelledLocations.Result[i].Latitude < mostSouth)
                    {
                        mostSouth= travelledLocations.Result[i].Latitude;
                    }
                    if (travelledLocations.Result[i].Latitude > mostNorth)
                    {
                        mostNorth = travelledLocations.Result[i].Latitude;
                    }
                    if (travelledLocations.Result[i].Longitude > mostEast)
                    {
                        mostEast= travelledLocations.Result[i].Longitude;
                    }
                    if (travelledLocations.Result[i].Longitude < mostWest)
                    {
                        mostWest= travelledLocations.Result[i].Longitude;
                    }
                    Console.WriteLine($"showRoadMap ADDING POLYLINE LATITUDE = " + travelledLocations.Result[i].Latitude + $", LONGITUDE = " + travelledLocations.Result[i].Longitude);
                    polyline.Positions.Add(newPosition);
                    lastPosition = newPosition;
                    map.Polylines.Add(polyline);
                }
                Console.WriteLine($"showRoadMap mostSouth = " + mostSouth + $", mostWest= " + mostWest+ $", mostNorth = " + mostNorth + $", mostEast = " + mostEast);
                Position southwestBound = new Position(mostSouth, mostWest);
                Position northeastBound = new Position(mostNorth, mostEast);
                var bounds = new Bounds(southwestBound, northeastBound);
                map.MoveToRegion(MapSpan.FromBounds(bounds));
                Console.WriteLine($"showRoadMap ADDED DATABASE POLYLINES!!!");
            }
        }

        
        public void ResumeDrifting(object sender, EventArgs e)
        {
            toggleShowOptionsTab();
            StartListening();
        }

        async public void ShowOptionsTab(object sender, EventArgs e)
        {
            toggleShowOptionsTab();
        }

        async public void toggleShowOptionsTab()
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
                //SET LOCATIONS VISITED:
                var locationsVisited = await Database.GetLocationsVisited();
                locationsDropdownlist.Clear();
                City_Drifter.ListviewLocationDropdown locationLoop;
                Console.WriteLine("locationsVisited LENGTH = " + locationsVisited.Count);
                bool isSelectLocation = false;
                int selectedLocationIndex = 0;
                for (var i = 0; i < locationsVisited.Count; i++)
                {
                    Console.WriteLine($"locationsVisited[i].ID = " + locationsVisited[i].ID + $", mySelectedLocationID = " + mySelectedLocationID);
                    if (locationsVisited[i].ID == mySelectedLocationID)
                    {
                        Console.WriteLine($"isSelectLocation IS TRUE !!!");
                        isSelectLocation = true;
                        selectedLocationIndex = i;
                    }
                    else
                    {
                        isSelectLocation = false;
                    }
                    locationLoop = new City_Drifter.ListviewLocationDropdown() { ID = locationsVisited[i].ID, iIndex = i, isSelected = isSelectLocation, Country = locationsVisited[i].Country, State = locationsVisited[i].State, City = locationsVisited[i].City, displayText = "Country: " + locationsVisited[i].Country + ", State: " + locationsVisited[i].State + ", City: " + locationsVisited[i].City };
                    locationsDropdownlist.Add(locationLoop);
                }
                if (locationsDropdownlist.Count > 0)
                {
                    SelectedLocationButton.Text = locationsDropdownlist[selectedLocationIndex].displayText + " ▼";
                    ShowRoadsButton.IsEnabled = true;
                }
                else
                {
                    ShowRoadsButton.IsEnabled = false;
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