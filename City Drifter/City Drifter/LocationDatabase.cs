using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace City_Drifter
{
    public class LocationDatabase
    {
        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        public LocationDatabase()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(LocationItem).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(LocationItem)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        public Task<List<LocationItem>> GetItemsAsync()
        {
            return Database.Table<LocationItem>().ToListAsync();
        }


        public Task<List<LocationItem>> GetLocationsVisited()
        {
            return Database.QueryAsync<LocationItem>("SELECT * FROM [LocationItem] GROUP BY [Country], [State], [City] ORDER BY ID");
        }

        public Task<List<LocationItem>> GetRoadsDone(String country, String state, String city, String travelMode)
        {
            String query = "SELECT* FROM[LocationItem] WHERE[Country] = '" + country + "' AND[State] = '" + state + "' AND[City] = '" + city + "' AND[Travel_Mode] = '" + travelMode + "'";
            Console.WriteLine($"GetRoadsDone called. QUERY = {query}");
            return Database.QueryAsync<LocationItem>(query);
        }

        public Task<LocationItem> GetItemAsync(int id)
        {
            return Database.Table<LocationItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public void UpdateItemAsync(LocationItem item)
        {            
            String query = "SELECT * FROM [LocationItem] WHERE [Country] = '" + item.Country + "' AND [State] = '" + item.State + "' AND [City] = '" + item.City + "'";
            Console.WriteLine($"LocationDatabase UpdateItemAsync called. query = " + query); ;
            var updateLocation = Database.QueryAsync<LocationItem>(query);
            Console.WriteLine($"LocationDatabase UpdateItemAsync updateLocation.Result.Count = " + updateLocation.Result.Count); ;
            if (updateLocation.Result.Count == 0)
            {
                Console.WriteLine("LocationDatabase UpdateItemAsync INSERTED INTO DATABASE !!!"); ;
                Database.InsertAsync(item);
            }
        }

        public Task<int> SaveItemAsync(LocationItem item)
        {
            Console.WriteLine($"SaveItemAsync called. item = {item.Travel_Mode}, {item.Latitude}, {item.Longitude}");
            if (item.ID!=null && item.ID != 0)
            {
                return Database.UpdateAsync(item);
            }
            else
            {
                return Database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(LocationItem item)
        {
            return Database.DeleteAsync(item);
        }
    }
}