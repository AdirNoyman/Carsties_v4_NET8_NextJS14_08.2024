using Duende.IdentityServer;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        // Define the CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MyPolicy", builder =>
            {
                builder.WithOrigins("http://localhost:5001")  // Replace with the origin you want to allow
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials(); // If you need to support credentials like cookies
            });
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                // options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;

        });

        builder.Services.ConfigureExternalCookie(options =>
       {
           options.Cookie.SameSite = SameSiteMode.Lax;
       });

        builder.Services.AddAuthentication();


        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.Use(async (context, next) =>
        {
            // Broad CSP policy for debugging purposes
            context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; connect-src * wss:;");
            await next();
        });

        // Use the defined CORS policy globally
        app.UseCors("MyPolicy");

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}