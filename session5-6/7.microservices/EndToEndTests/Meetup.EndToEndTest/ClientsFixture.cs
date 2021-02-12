using System;
using System.Net.Http;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using static Meetup.EndToEndTest.HttpPolicies;
using Meetup.GroupManagement.Contracts.Commands.V1;
using Meetup.GroupManagement.Contracts.Queries.V1;
using Meetup.Notifications.Queries.Contracts.V1;

namespace Meetup.EndToEndTest
{
    public class ClientsFixture
    {
        const string MeetupSchedulingCommandsName = nameof(MeetupSchedulingCommandsName);
        const string MeetupSchedulingQueriesName  = nameof(MeetupSchedulingQueriesName);
        const string GroupManagementCommandsName  = nameof(GroupManagementCommandsName);
        const string GroupManagementQueriesName   = nameof(GroupManagementQueriesName);
        const string UserProfileName              = nameof(UserProfileName);
        const string NotificationName             = nameof(NotificationName);

        public HttpClient                                          MeetupSchedulingCommands { get; }
        public HttpClient                                          MeetupSchedulingQueries  { get; }
        public MeetupGroupManagement.MeetupGroupManagementClient   GroupManagementCommands  { get; }
        public MeetupGroupQueries.MeetupGroupQueriesClient         GroupManagementQueries   { get; }
        public NotificationsQueries.NotificationsQueriesClient     Notifications            { get; }
        public UserProfile.Contracts.UserProfile.UserProfileClient UserProfile              { get; }

        public ClientsFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            AddHttpClient(
                configuration["MeetupScheduling:CommandsAddress"],
                MeetupSchedulingCommandsName
            );
            AddHttpClient(
                configuration["MeetupScheduling:QueriesAddress"],
                MeetupSchedulingQueriesName
            );
            AddGrpcClient<MeetupGroupManagement.MeetupGroupManagementClient>(
                configuration["GroupManagement:Address"],
                GroupManagementCommandsName
            );
            AddGrpcClient<MeetupGroupQueries.MeetupGroupQueriesClient>(
                configuration["GroupManagement:Address"],
                GroupManagementQueriesName
            );
            AddGrpcClient<NotificationsQueries.NotificationsQueriesClient>(
                configuration["Notifications:Address"],
                NotificationName
            );
            AddGrpcClient<UserProfile.Contracts.UserProfile.UserProfileClient>(
                configuration["UserProfile:Address"],
                UserProfileName
            );

            var sp = services.BuildServiceProvider();

            MeetupSchedulingCommands =
                CreateHttpClient(MeetupSchedulingCommandsName);
            MeetupSchedulingQueries =
                CreateHttpClient(MeetupSchedulingQueriesName);
            GroupManagementCommands =
                CreateGrpcClient<MeetupGroupManagement.MeetupGroupManagementClient>(GroupManagementCommandsName);
            GroupManagementQueries =
                CreateGrpcClient<MeetupGroupQueries.MeetupGroupQueriesClient>(GroupManagementQueriesName);
            Notifications =
                CreateGrpcClient<NotificationsQueries.NotificationsQueriesClient>(NotificationName);
            UserProfile =
                CreateGrpcClient<UserProfile.Contracts.UserProfile.UserProfileClient>(UserProfileName);

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

            T CreateGrpcClient<T>(string clientName) where T : class
                => sp.GetRequiredService<GrpcClientFactory>()
                    .CreateClient<T>(clientName);

            HttpClient CreateHttpClient(string clientName)
                => sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(clientName);
        }
    }
}