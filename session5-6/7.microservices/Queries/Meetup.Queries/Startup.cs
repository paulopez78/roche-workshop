using System;
using System.Net.Http;
using Grpc.Net.ClientFactory;
using Meetup.GroupManagement.Contracts.Queries.V1;
using Meetup.Notifications.Queries.Contracts.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Polly;
using static Meetup.Queries.HttpPolicies;

namespace Meetup.Queries
{
    public class Startup
    {
        public static string ApplicationKey = "meetup_queries";

        const string MeetupSchedulingClientName = nameof(MeetupSchedulingClientName);
        const string GroupManagementClientName  = nameof(GroupManagementClientName);
        const string UserProfileClientName      = nameof(UserProfileClientName);
        const string NotificationClientName     = nameof(NotificationClientName);

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.ServiceName = ApplicationKey;
                        o.AgentHost   = Configuration["JAEGER_HOST"] ?? "localhost";
                    })
            );
            AddHttpClient(
                Configuration["MeetupScheduling:Address"],
                MeetupSchedulingClientName
            );
            AddGrpcClient<MeetupGroupQueries.MeetupGroupQueriesClient>(
                Configuration["GroupManagement:Address"],
                GroupManagementClientName
            );
            AddGrpcClient<NotificationsQueries.NotificationsQueriesClient>(
                Configuration["Notifications:Address"],
                NotificationClientName
            );
            AddGrpcClient<UserProfile.Contracts.UserProfile.UserProfileClient>(
                Configuration["UserProfile:Address"],
                UserProfileClientName
            );

            services.AddScoped(sp => new MeetupQueryHandler(
                CreateHttpClient(sp, MeetupSchedulingClientName),
                CreateGrpcClient<MeetupGroupQueries.MeetupGroupQueriesClient>(sp, GroupManagementClientName),
                CreateGrpcClient<NotificationsQueries.NotificationsQueriesClient>(sp, NotificationClientName),
                CreateGrpcClient<UserProfile.Contracts.UserProfile.UserProfileClient>(sp, UserProfileClientName)
            ));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Meetup Queries", Version = "v1"});
            });

            void AddGrpcClient<T>(string address, string clientName) where T : class =>
                services.AddGrpcClient<T>(
                        clientName,
                        o => o.Address = new Uri(address))
                    .AddPolicyHandler(RetryPolicy())
                    .AddPolicyHandler(GetCircuitBreakerPolicy());

            void AddHttpClient(string address, string clientName)
            {
                var jitterer = new Random();
                services.AddHttpClient(clientName, c => c.BaseAddress = new Uri(address))
                    .AddTransientHttpErrorPolicy(p =>
                        p.WaitAndRetryAsync(3, // exponential back-off plus some jitter
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))));
            }

            T CreateGrpcClient<T>(IServiceProvider sp, string clientName) where T : class
                => sp.GetRequiredService<GrpcClientFactory>()
                    .CreateClient<T>(clientName);

            HttpClient CreateHttpClient(IServiceProvider sp, string clientName)
                => sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(clientName);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "meetup queries v1"));
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}