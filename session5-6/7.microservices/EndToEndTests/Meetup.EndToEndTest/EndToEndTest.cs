using System;
using System.Threading.Tasks;
using FluentAssertions;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;
using static System.Guid;
using static Meetup.EndToEndTest.UserProfileExtensions;
using static Meetup.EndToEndTest.NotificationsExtensions;

namespace Meetup.EndToEndTest
{
    public class EndToEndTest : IClassFixture<ClientsFixture>
    {
        readonly ClientsFixture Fixture;
        public EndToEndTest(ClientsFixture fixture) => Fixture = fixture;

        [Fact]
        public async Task Meetup_EndToEnd_Test()
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddJaegerExporter(b => b.ServiceName = nameof(EndToEndTest))
                .Build();

            await Fixture.UserProfile
                .CreateUserProfile(Pau, Joe, Carla, Alice, Bob);

            await Fixture.GroupManagementCommands
                .StartMeetupGroup(
                    NetCoreBcn.ToString(),
                    "Barcelona .NET Core",
                    "This is an awesome group",
                    groupSlug: NetCoreBcnSlug,
                    location: "Barcelona",
                    organizer: Pau
                );

            // Add some members to the group
            await Fixture.GroupManagementCommands
                .AddGroupMember(NetCoreBcn.ToString(), Joe, Carla, Alice, Bob);

            // create meetup
            await Fixture.MeetupSchedulingCommands
                .CreateMeetup(
                    MicroservicesMeetup,
                    NetCoreBcn,
                    "Microservices failures",
                    "This is a talk about failures"
                );

            await Fixture.MeetupSchedulingCommands
                .MakeOnline(MicroservicesMeetup, "https://zoom.us/netcorebcn");

            await Fixture.MeetupSchedulingCommands
                .Schedule(MicroservicesMeetup, StartTime, EndTime);

            await Fixture.MeetupSchedulingCommands
                .Publish(MicroservicesMeetup);

            await Task.Delay(5_000);

            await Fixture.MeetupSchedulingCommands
                .ReduceCapacity(MicroservicesMeetup, byNumber: 8);

            await Fixture.MeetupSchedulingCommands
                .Attend(MicroservicesMeetup,
                    Joe,
                    Carla,
                    Alice,
                    Bob
                );

            await Fixture.MeetupSchedulingCommands
                .CancelAttendance(MicroservicesMeetup, Joe);

            await Fixture.MeetupSchedulingCommands
                .ReduceCapacity(MicroservicesMeetup, byNumber: 1);

            await Fixture.MeetupSchedulingCommands
                .IncreaseCapacity(MicroservicesMeetup, byNumber: 1);

            await Fixture.GroupManagementCommands
                .LeaveGroup(NetCoreBcn.ToString(), Alice);

            await Fixture.MeetupSchedulingCommands
                .Attend(MicroservicesMeetup, Joe);

            // assert
            await Task.Delay(35_000);
            var meetup = await Fixture.MeetupSchedulingQueries.Get(MicroservicesMeetup);

            meetup.Status.Should().Be("Finished");
            meetup.AttendantListStatus.Should().Be("Archived");

            meetup.Waiting(Joe).Should().BeTrue();
            meetup.Going(Carla).Should().BeTrue();
            meetup.NotGoing(Alice).Should().BeTrue();
            meetup.Going(Bob).Should().BeTrue();

            await Fixture.Notifications
                .UserNotifications(Joe)
                .OfType(NotificationType.MeetupPublished, NotificationType.Attending, NotificationType.Waiting)
                .ShouldHaveReceived();

            await Fixture.Notifications
                .UserNotifications(Carla)
                .OfType(NotificationType.MeetupPublished, NotificationType.Attending)
                .ShouldHaveReceived();

            await Fixture.Notifications
                .UserNotifications(Alice)
                .OfType(NotificationType.MeetupPublished, NotificationType.Waiting, NotificationType.Attending)
                .ShouldHaveReceived();

            await Fixture.Notifications
                .UserNotifications(Bob)
                .OfType(NotificationType.MeetupPublished, NotificationType.Waiting, NotificationType.Attending)
                .ShouldHaveReceived();

            await Fixture.Notifications
                .UserNotifications(Pau)
                .OfType(NotificationType.MeetupPublished, NotificationType.MemberJoined, NotificationType.MemberLeft)
                .ShouldHaveReceived();
        }

        static Guid   NetCoreBcn          = NewGuid();
        static string NetCoreBcnSlug      = $"netcorebcn-{NetCoreBcn}";
        static Guid   MicroservicesMeetup = NewGuid();

        DateTimeOffset StartTime = DateTimeOffset.UtcNow.AddSeconds(15);
        DateTimeOffset EndTime   = DateTimeOffset.UtcNow.AddSeconds(30);

        User Pau   = new(NewGuid(), "Pau", "Lopez", "pau.lopez@meetup.com");
        User Joe   = new(NewGuid(), "Joe", "Smith", "joe.smith@meetup.com");
        User Carla = new(NewGuid(), "Carla", "Garcia", "carla.garcia@meetup.com");
        User Alice = new(NewGuid(), "Alice", "Joplin", "alice.joplin@meetup.com");
        User Bob   = new(NewGuid(), "Bob", "Dylan", "bob.dylan@meetup.com");
    }
}