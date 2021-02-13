using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Serilog;
using static System.Environment;
using static MeetupEvents.Contracts.MeetupCommands.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;
using static Meetup.Notifications.Contracts.Commands.V1;

namespace MeetupEvents.ProcessManager
{
    public class Program
    {
        public static string ApplicationKey = "meetup_events_process_manager";

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
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<MeetupProcessManager>();
                        x.AddRabbitMqMessageScheduler();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UseDelayedExchangeMessageScheduler();
                            cfg.Host(hostContext.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            // https://masstransit-project.com/usage/exceptions.html
                            cfg.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(100));
                                r.Handle<ApplicationException>();
                                r.Ignore<ArgumentException>();
                            });

                            cfg.ReceiveEndpoint($"{ApplicationKey}",
                                e => { e.Consumer<MeetupProcessManager>(context); });

                            var meetupQueue = new Uri($"queue:meetup_events-commands");
                            EndpointConvention.Map<CreateAttendantList>(meetupQueue);
                            EndpointConvention.Map<Open>(meetupQueue);
                            EndpointConvention.Map<Close>(meetupQueue);
                            EndpointConvention.Map<Archive>(meetupQueue);
                            EndpointConvention.Map<Start>(meetupQueue);
                            EndpointConvention.Map<Finish>(meetupQueue);
                            EndpointConvention.Map<RemoveAttendantFromMeetups>(meetupQueue);
                            
                            var notificationsQueue = new Uri($"queue:meetup_notifications-commands");
                            EndpointConvention.Map<NotifyMeetupPublished>(notificationsQueue);
                            EndpointConvention.Map<NotifyMeetupCancelled>(notificationsQueue);
                            EndpointConvention.Map<NotifyMeetupAttendantWaiting>(notificationsQueue);
                            EndpointConvention.Map<NotifyMeetupAttendantGoing>(notificationsQueue);
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