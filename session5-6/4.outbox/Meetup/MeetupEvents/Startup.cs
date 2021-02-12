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
using MeetupEvents.Application.Integrations;
using MeetupEvents.Application.Meetup;
using MeetupEvents.Infrastructure;
using MeetupEvents.Queries;
using static MeetupEvents.Application.AttendantList.DomainServices;
using static MeetupEvents.Application.Integrations.DomainServices;

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
                GetAttendantListId(() => new NpgsqlConnection(connectionString), id)
            );

            services.AddSingleton<GetMeetupEventId>(id =>
                GetMeetupEventId(() => new NpgsqlConnection(connectionString), id)
            );

            services.AddSingleton<GetMeetupDetails>(id =>
                GetMeetupDetails(() => new NpgsqlConnection(connectionString), id)
            );

            services.AddSingleton(new MeetupEventQueries(() => new NpgsqlConnection(connectionString)));
            services.AddHostedService<OutboxProcessor>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AttendantListMassTransitConsumer>();
                x.AddConsumer<IntegrationEventsDispatcher>();

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
                        // r.Immediate(10);
                        r.Handle<InvalidOperationException>();
                        r.Handle<NpgsqlException>();
                        r.Ignore<ArgumentException>();
                    });

                    cfg.ReceiveEndpoint("attendant-list",
                        e => e.Consumer<AttendantListMassTransitConsumer>(context));

                    cfg.ReceiveEndpoint("publish-integration-events",
                        e => e.Consumer<IntegrationEventsDispatcher>(context));
                });
            });
            services.AddMassTransitHostedService();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MeetupEvents", Version = "v1"});
            });
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