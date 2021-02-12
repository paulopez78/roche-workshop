using System;
using System.Data;
using FluentValidation;
using GreenPipes;
using MassTransit;
using MediatR;
using Npgsql;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Meetup.GroupManagement.Data;
using Meetup.GroupManagement.Middleware;

namespace Meetup.GroupManagement
{
    public class Startup
    {
        public static string ApplicationKey = "meetup_group";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("MeetupGroupManagement");
            services.AddDbContext<MeetupGroupManagementDbContext>(options => options.UseNpgsql(connectionString));
            services.AddSingleton<Func<IDbConnection>>(() => new NpgsqlConnection(connectionString));
            services.AddMediatR(typeof(Startup));

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OutboxBehavior<,>));
            services.AddValidatorsFromAssemblies(new[] {typeof(Startup).Assembly});

            services.AddScoped<IntegrationEventsPublisher>();

            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.ServiceName = ApplicationKey;
                        o.AgentHost   = Configuration["JAEGER_HOST"] ?? "localhost";
                    })
            );

            services.AddGrpc();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // https://masstransit-project.com/usage/exceptions.html
                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromMilliseconds(100));
                        r.Handle<NpgsqlException>();
                        r.Ignore<ArgumentException>();
                    });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-publish-integration-events",
                        e => { e.Consumer<IntegrationEventsPublisher>(context); });
                });
            });
            services.AddMassTransitHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<MeetupGroupManagementService>();
                endpoints.MapGrpcService<MeetupGroupQueriesService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }
    }
}