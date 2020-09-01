using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace City_Drifter
{
 
    public class LocationItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Country { get; set; }
        public string State { get; set; }        
        public string City { get; set; }
        public string Road { get; set; }
        public bool Done { get; set; }
    }

}
