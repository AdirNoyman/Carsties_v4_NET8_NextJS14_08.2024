using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitilaizer.InitDb(app);
}
catch (Exception e)
{
    
    Console.WriteLine("Error initilaizing the mongo db 😫");
    Console.WriteLine(e.Message);
}


app.Run();
