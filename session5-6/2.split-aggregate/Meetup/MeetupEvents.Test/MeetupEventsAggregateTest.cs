using System;
using System.Linq;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventsAggregateTest
    {
        [Fact]
        public void Given_None_Meetup_When_Create_Then_Draft()
        {
            // Arrange - Given
            // Act - When
            var meetup = CreateMeetup();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Draft);
        }

        [Fact]
        public void Given_Draft_Meetup_When_Publish_Then_Published()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline();

            // Act - When
            meetup.Publish();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Published);
        }

        [Fact]
        public void Given_Published_Meetup_When_Cancel_Then_Cancelled()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();

            // Act - When
            meetup.Cancel();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Cancelled);
        }

        [Fact]
        public void Given_Published_Meetup_When_Start_Then_Started()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();

            // Act - When
            meetup.Start();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Started);
        }

        [Fact]
        public void Given_Started_Meetup_When_Finish_Then_Finished()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();
            meetup.Start();

            // Act - When
            meetup.Finish();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Finished);
        }

        [Fact]
        public void Given_Draft_Meetup_When_Cancel_Then_InvalidOperation()
        {
            // Arrange - Given
            var meetup = CreateMeetup();

            // Act - When
            Action publish = () => meetup.Cancel();

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Given_Cancelled_Meetup_When_Publish_Then_InvalidOperation()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();
            meetup.Cancel();

            // Act - When
            Action publish = () => meetup.Publish();

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Given_Not_Active_Meetup_When_MakeOnline_Then_InvalidOperation()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();
            meetup.Cancel();

            // Act - When
            Action publish = () => meetup.MakeOnline(new Uri("https://zoom.us/netcorebcn"));

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        MeetupEventAggregate CreateMeetup(int capacity = 10)
        {
            var meetup = new MeetupEventAggregate();
            meetup.Create(NewGuid(), NewGuid(), Details.From("Microservices failures", "This is a meetup about ..."));
            return meetup;
        }
    }

    public static class MeetupEventsAggregateTestExtensions
    {
        public static MeetupEventAggregate Schedule(this MeetupEventAggregate meetup)
        {
            var now = DateTimeOffset.UtcNow;
            meetup.Schedule(
                ScheduleDateTime.From(() => now, now.AddMonths(1), durationInHours: 2)
            );
            return meetup;
        }

        public static MeetupEventAggregate MakeOnline(this MeetupEventAggregate meetup)
        {
            meetup.MakeOnline(new Uri("http://zoom.us/netcorebcn"));
            return meetup;
        }

        public static MeetupEventAggregate PublishMeetup(this MeetupEventAggregate meetup)
        {
            meetup.Publish();
            return meetup;
        }
    }
}