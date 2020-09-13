using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace City_Drifter
{
    public partial class App : Application
    {
        public static double DisplayScreenWidth { get; set; }
        public static double DisplayScreenHeight = 0f;
        public static double DisplayScaleFactor = 0f;

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
            //var assembliesToInclude = new [] { typeof(Xamarin.Forms.Maps.UWP.MapRenderer).GetTypeInfo().Assembly };
            //Xamarin.Forms.Forms.Init(e, assembliesToInclude);
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
