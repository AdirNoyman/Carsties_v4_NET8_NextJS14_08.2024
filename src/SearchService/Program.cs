using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));

// For the search functionality, create indexes for properties that will be used for searching auctions
await DB.Index<Item>()
    .Key(a => a.Make, KeyType.Text)
    .Key(a => a.Model, KeyType.Text)   
    .Key(a => a.Color, KeyType.Text)    
    .CreateAsync();

app.Run();
