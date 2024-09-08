using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitilaizer
    {

        public static async Task InitDb(WebApplication app)
        {

            await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            // For the search functionality, create indexes for properties that will be used for searching auctions
            await DB.Index<Item>()
                .Key(a => a.Make, KeyType.Text)
                .Key(a => a.Model, KeyType.Text)
                .Key(a => a.Color, KeyType.Text)
                .CreateAsync();

                var count = await DB.CountAsync<Item>();

                using var scope = app.Services.CreateScope();

                var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();

                var items = await httpClient.GetItemsForSearchDb();

                Console.WriteLine($"{items.Count} items returned from the auctions service 🤓🤘");

                // Save the items to the database, only if there are any items
                if (items.Count > 0) await DB.SaveAsync(items);

               
        }

    }
}