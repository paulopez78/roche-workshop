using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;
using static System.Guid;

namespace MeetupEvents.EndToEndTest
{
    public class EndToEndTest : IClassFixture<ClientsFixture>
    {
        readonly HttpClient CommandsClient;
        readonly HttpClient QueriesClient;

        public EndToEndTest(ClientsFixture fixture)
        {
            CommandsClient = fixture.CommandsClient;
            QueriesClient  = fixture.QueriesClient;
        }

        [Fact]
        public async Task MeetupEvent_EndToEnd_Test()
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(b => b.ServiceName = "MeetupEvent_EndToEnd_Test")
                .Build();

            // create meetup
            await CommandsClient
                .CreateMeetup(
                    MicroservicesMeetup,
                    BcnNetCoreGroup,
                    "Microservices failures",
                    "This is a talk about failures"
                );

            await CommandsClient
                .MakeOnline(MicroservicesMeetup, "https://zoom.us/netcorebcn");

            await CommandsClient
                .Schedule(MicroservicesMeetup, StartTime, EndTime);
            

            await CommandsClient
                .Publish(MicroservicesMeetup);

            await Task.Delay(5_000);
            
            await CommandsClient
                .ReduceCapacity(MicroservicesMeetup, byNumber: 7);

            await CommandsClient
                .Attend(MicroservicesMeetup,
                    Joe,
                    Carla,
                    Alice,
                    Bob
                );

            await CommandsClient
                .CancelAttendance(MicroservicesMeetup, Joe);

            await CommandsClient
                .ReduceCapacity(MicroservicesMeetup, byNumber: 1);

            await CommandsClient
                .IncreaseCapacity(MicroservicesMeetup, byNumber: 1);

            await CommandsClient
                .Attend(MicroservicesMeetup, Joe);

            // assert
            await Task.Delay(35_000);
            var meetup = await QueriesClient.Get(MicroservicesMeetup);

            meetup.Status.Should().Be("Finished");
            meetup.AttendantListStatus.Should().Be("Archived");

            meetup.Waiting(Joe).Should().BeTrue();
            meetup.Going(Carla).Should().BeTrue();
            meetup.Going(Bob).Should().BeTrue();
        }

        Guid MicroservicesMeetup = NewGuid();
        Guid BcnNetCoreGroup     = NewGuid();

        DateTimeOffset StartTime = DateTimeOffset.UtcNow.AddSeconds(15);
        DateTimeOffset EndTime   = DateTimeOffset.UtcNow.AddSeconds(30);

        private Guid Joe   = NewGuid();
        private Guid Carla = NewGuid();
        private Guid Alice = NewGuid();
        private Guid Bob   = NewGuid();
    }
}