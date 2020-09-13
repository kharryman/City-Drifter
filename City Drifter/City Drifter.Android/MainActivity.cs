using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Plugin.CurrentActivity;

namespace City_Drifter.Droid
{
    [Activity(Label = "City_Drifter", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
         const int RequestLocationId = 0;

         readonly string[] LocationPermissions =
         {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
         };       
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            App.DisplayScreenWidth = (double)Resources.DisplayMetrics.WidthPixels / (double)Resources.DisplayMetrics.Density;
            App.DisplayScreenHeight = (double)Resources.DisplayMetrics.HeightPixels / (double)Resources.DisplayMetrics.Density;
            App.DisplayScaleFactor = (double)Resources.DisplayMetrics.Density;
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == RequestLocationId)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int)Android.Content.PM.Permission.Granted))
                {
                    // Permissions granted - display a message.
                }
                else
                {
                    // Permissions denied - display a message.
                }
            }
            else
            {
               base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }            
        }

         protected override void OnStart()
         {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
               if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
               {
                     RequestPermissions(LocationPermissions, RequestLocationId);
               }
               else
               {
                     // Permissions already granted - display a message.
               }
            }
         }       
    }
}