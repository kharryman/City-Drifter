using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace City_Drifter
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]

    public class Employee
    {
        public string DisplayName { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        public string tag = "MainPage";
        public Boolean isTab = false;
        ObservableCollection<Employee> employees = new ObservableCollection<Employee>();
        public ObservableCollection<Employee> Employees { get { return employees; } }

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
            EmployeeView.ItemsSource = employees;

            // ObservableCollection allows items to be added after ItemsSource
            // is set and the UI will react to changes
            employees.Add(new Employee { DisplayName = "Rob Finnerty" });
            employees.Add(new Employee { DisplayName = "Bill Wrestler" });
            employees.Add(new Employee { DisplayName = "Dr. Geri-Beth Hooper" });
            employees.Add(new Employee { DisplayName = "Dr. Keith Joyce-Purdy" });
            employees.Add(new Employee { DisplayName = "Sheri Spruce" });
            employees.Add(new Employee { DisplayName = "Burt Indybrick" });
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
                ColWidth = 0;
            }
            Log.Warning(tag, "ShowOptionsTab called, SET ColWidth = " + ColWidth);
        }
    }
}