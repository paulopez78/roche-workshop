using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Trace;
using Serilog;
using static MeetupEvents.IntegrationEventsPublisher.DomainServices;
using static System.Environment;

namespace MeetupEvents.IntegrationEventsPublisher
{
    public class Program
    {
        public static string ApplicationKey = "meetup_events_publisher";

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
                CreateHostBuilder(args).Build().Run();
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
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("MeetupEvents");
                    services.AddSingleton<GetMeetupEventId>(id =>
                        GetMeetupEventId(() => new NpgsqlConnection(connectionString), id)
                    );

                    services.AddSingleton<GetMeetupDetails>(id =>
                        GetMeetupDetails(() => new NpgsqlConnection(connectionString), id)
                    );
                    
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<IntegrationEventsPublisher>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration.GetValue("RabbitMQ:Host", "localhost"), "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            cfg.UseMessageRetry(r =>
                            {
                                r.Interval(5, TimeSpan.FromMilliseconds(100));
                                r.Handle<InvalidOperationException>();
                                r.Ignore<ArgumentException>();
                            });

                            cfg.ReceiveEndpoint($"meetup_events-publish",
                                e => e.Consumer<IntegrationEventsPublisher>(context));
                        });
                    });
                    services.AddMassTransitHostedService();
                    
                    services.AddOpenTelemetryTracing(b =>
                        b.AddMassTransitInstrumentation()
                            .AddJaegerExporter(o =>
                            {
                                o.ServiceName = ApplicationKey;
                                o.AgentHost   = hostContext.Configuration["JAEGER_HOST"] ?? "localhost";
                            })
                    );
                });
    }
}