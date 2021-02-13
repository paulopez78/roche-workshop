using System;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Npgsql;
using MeetupEvents.Application.AttendantList;
using MeetupEvents.Application.Meetup;
using MeetupEvents.Infrastructure;
using OpenTelemetry.Trace;
using static MeetupEvents.Contracts.AttendantListCommands.V1;
using static MeetupEvents.Program;

namespace MeetupEvents
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("MeetupEvents");
            services.AddDbContext<MeetupEventsDbContext>(options => { options.UseNpgsql(connectionString); });

            services.AddSingleton<Func<DateTimeOffset>>(() => DateTimeOffset.UtcNow);
            services.AddEventsDispatcher(typeof(Startup));

            services.AddScoped<MeetupEventsApplicationService>();
            services.AddScoped<AttendantListApplicationService>();
            services.AddScoped<ApplicationServiceBuilder<AttendantListApplicationService>>();
            services.AddScoped<ApplicationServiceBuilder<MeetupEventsApplicationService>>();

            services.AddSingleton<GetMappedId>(id =>
                DomainServices.GetAttendantListId(() => new NpgsqlConnection(connectionString), id)
            );

            services.AddSingleton<GetAttendingMeetups>((groupId, memberId) =>
                DomainServices.GetAttendingMeetups(() => new NpgsqlConnection(connectionString), groupId, memberId)
            );

            services.AddHostedService<OutboxProcessor>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AttendantListMassTransitConsumer>();
                x.AddConsumer<MeetupEventsMassTransitConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration.GetValue("RabbitMQ:Host", "localhost"), "/", h =>
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

                    var endpointName  = $"{ApplicationKey}-commands";
                    cfg.ReceiveEndpoint(endpointName,
                        e =>
                        {
                            e.Consumer<AttendantListMassTransitConsumer>(context);
                            e.Consumer<MeetupEventsMassTransitConsumer>(context);
                        });
                    
                    var endpointQueue = new Uri($"queue:{endpointName}");
                    EndpointConvention.Map<CancelAttendance>(endpointQueue);
                });
            });
            services.AddMassTransitHostedService();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MeetupEvents", Version = "v1"});
            });

            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.ServiceName = ApplicationKey;
                        o.AgentHost   = Configuration["JAEGER_HOST"] ?? "localhost";
                    })
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeetupEvents v1"));
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}