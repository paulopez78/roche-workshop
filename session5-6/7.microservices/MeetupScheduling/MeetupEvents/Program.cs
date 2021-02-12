using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using static System.Environment;
using MeetupEvents.Infrastructure;

namespace MeetupEvents
{
    public static class Program
    {
        public static string ApplicationKey = "meetup_events";

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
                .Enrich.WithProperty(nameof(ApplicationKey), ApplicationKey)
                .CreateLogger();

            try
            {
                Log.Information("Starting up");
                var builder = CreateHostBuilder(args).Build();

                using (var scope = builder.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MeetupEventsDbContext>();
                    dbContext.Database?.EnsureCreated();
                }

                builder.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}