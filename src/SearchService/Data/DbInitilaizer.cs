using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

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

                if (count == 0)
                {
                    Console.WriteLine("No Data in SearchDb. Adding sample data");
                    var itemData = await File.ReadAllTextAsync("Data/auctions.json");

                    var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

                    // Parse the sample data from the JSON to Item POCOs
                    var items = JsonSerializer.Deserialize<List<Item>>(itemData,options);

                    // Save the Items to the MongoDB
                    await DB.SaveAsync(items);
                }
        }

    }
}