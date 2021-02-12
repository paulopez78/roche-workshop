using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace MeetupEvents.EndToEndTest
{
    public class ClientsFixture
    {
        const string CommandsClientName = nameof(CommandsClientName);
        const string QueriesClientName  = nameof(QueriesClientName);

        public HttpClient CommandsClient { get; }
        public HttpClient QueriesClient  { get; }

        public ClientsFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            AddHttpClient(
                configuration["MeetupEvents:CommandsAddress"],
                CommandsClientName
            );
            AddHttpClient(
                configuration["MeetupEvents:QueriesAddress"],
                QueriesClientName
            );

            var sp = services.BuildServiceProvider();

            CommandsClient =
                CreateHttpClient(CommandsClientName);
            QueriesClient =
                CreateHttpClient(QueriesClientName);

            void AddHttpClient(string address, string clientName)
            {
                var jitterer = new Random();
                services.AddHttpClient(clientName, c => c.BaseAddress = new Uri(address))
                    .AddTransientHttpErrorPolicy(p =>
                        p.WaitAndRetryAsync(3, // exponential back-off plus some jitter
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))));
            }

            HttpClient CreateHttpClient(string clientName)
                => sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(clientName);
        }
    }
}