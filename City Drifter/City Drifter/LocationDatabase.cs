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

        public Task<List<LocationItem>> GetRoadsDoneAsync(String country, String state, String city)
        {
            return Database.QueryAsync<LocationItem>("SELECT * FROM [LocationItem] WHERE [Done] = 1 AND [Country] = '" + country + "' AND [State] = '" + state + "' AND [City] = '" + city + "'");
        }

        public Task<LocationItem> GetItemAsync(int id)
        {
            return Database.Table<LocationItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(LocationItem item)
        {
            if (item.ID != 0)
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