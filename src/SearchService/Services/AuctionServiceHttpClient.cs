using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // Get the last updated date of the items in the searchDb
        public async Task<List<Item>> GetItemsForSearchDb()
        {
            // DB.Find<Item, string> = find the Item but return the date in a string format
            var lastUpdatedAt = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            // Project = produce the date of UpadtedAt in a string format
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync(); //
            
            // Create the http request to the AuctionService
            return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceBaseURL"] + "/api/auctions?date=" + lastUpdatedAt);
        }
        
    }
}