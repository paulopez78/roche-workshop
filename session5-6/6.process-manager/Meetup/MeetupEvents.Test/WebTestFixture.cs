using MeetupEvents.Infrastructure;
using MeetupEvents.Queries;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit.Abstractions;

namespace MeetupEvents.Test
{
    public class WebTestFixture : WebApplicationFactory<Startup>
    {
        public ITestOutputHelper Output { get; set; }

        public MeetupEventQueries Queries { get; set; }

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
                var connectionString = services.BuildServiceProvider().GetService<IConfiguration>()
                    .GetConnectionString("MeetupEvents");

                Queries = new MeetupEventQueries(
                    () => new NpgsqlConnection(connectionString)
                );

                using var scope = services.BuildServiceProvider().CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<MeetupEventsDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            });

            return builder;
        }
    }
}