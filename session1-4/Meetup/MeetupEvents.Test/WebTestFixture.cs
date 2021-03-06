﻿using Grpc.Net.Client;
using MeetupEvents.Application;
using MeetupEvents.Contracts.Commands.V1;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MeetupEvents.Test
{
    public class WebTestFixture : WebApplicationFactory<Startup>
    {
        public ITestOutputHelper Output { get; set; }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddXUnit(Output);
            });

            builder.ConfigureServices(services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<MeetupEventsDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            });

            return builder;
        }
        
        public MeetupEventsService.MeetupEventsServiceClient CreateGrpcClient()
        {
            var client = CreateDefaultClient();
            var channel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });
        
            return new MeetupEventsService.MeetupEventsServiceClient(channel);
        } 
    }
}