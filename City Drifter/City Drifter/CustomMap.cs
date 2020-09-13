using System.Collections.Generic;
using Xamarin.Forms.Maps;
namespace City_Drifter
{
    public class CustomMap : Map
    {
        public List<CustomPin> CustomPins { get; set; }
    }
}