using MassTransit;
using Meetup.UserProfile.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OpenTelemetry.Trace;

namespace Meetup.UserProfile
{
    public class Startup
    {
        public static string ApplicationKey = "meetup_user_profiles";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            MongoConventions.RegisterConventions();
            var connectionString = Configuration.GetConnectionString("UserProfile");
            var mongoDb          = CreateMongoDb(connectionString);

            services.AddSingleton(mongoDb);
            services.AddGrpc();

            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.ServiceName = ApplicationKey;
                        o.AgentHost   = Configuration["JAEGER_HOST"] ?? "localhost";
                    })
            );

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((_, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                });
            });
            services.AddMassTransitHostedService();
            // services.AddHostedService<IntegrationEventsPublisher>();
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
                endpoints.MapGrpcService<UserProfileService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }

        static IMongoDatabase CreateMongoDb(string connectionString)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(ApplicationKey);
        }
    }
}