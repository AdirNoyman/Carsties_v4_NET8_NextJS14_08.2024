using System.Net;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    Console.WriteLine("The application has started 🚀");

    try
    {
        await DbInitilaizer.InitDb(app);
    }
    catch (Exception e)
    {

        Console.WriteLine("Error initilaizing the mongo db 😫");
        Console.WriteLine(e.Message);
    }
});




app.Run();

// Policy for handeling http response message coming from the auction service
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        // retry every 3 seconds
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
}
