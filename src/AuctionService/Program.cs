using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Register the AutoMapper profiles to the application configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Register Masstransit services
builder.Services.AddMassTransit(x =>
{
    // Configure messages outbox for resiliency
    x.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
    {
        // Once the servicebus is up again, try every 10 seconds, to look in the outbox for messages waiting to be send to the servicebus
        options.QueryDelay = TimeSpan.FromSeconds(10);
        options.UsePostgres();
        options.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

// Seed the Auctions database (if needed) before running the app
try
{
    DbInitilaizer.InitDb(app);
}
catch (Exception e)
{
    
    Console.WriteLine("Error seeding the database: " + e.Message);
}

app.Run();
