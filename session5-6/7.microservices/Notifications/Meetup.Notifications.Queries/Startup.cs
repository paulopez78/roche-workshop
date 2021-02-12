using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using static MongoDB.Driver.Builders<Meetup.Notifications.Contracts.ReadModels.V1.Notification>;
using static Meetup.Notifications.Contracts.ReadModels.V1;

namespace Meetup.Notifications.Queries
{
    public class Startup
    {
        public static string ApplicationKey = "meetup_notifications";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var mongoDb = CreateMongoDb();
            services.AddSingleton(mongoDb);
            services.AddGrpc();
            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
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
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<NotificationsQueriesService>();

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(
                            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                    });
            });
        }

        IMongoDatabase CreateMongoDb()
        {
            MongoConventions.RegisterConventions();
            var client = new MongoClient(Configuration.GetConnectionString("Notifications"));

            var db = client.GetDatabase(ApplicationKey);
            db.GetCollection<Notification>(nameof(Notification))
                .Indexes
                .CreateOne(new CreateIndexModel<Notification>(IndexKeys.Ascending(x => x.UserId)));

            return db;
        }
    }
}