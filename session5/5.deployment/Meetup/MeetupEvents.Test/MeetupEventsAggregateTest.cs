using System;
using System.Linq;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;
using static MeetupEvents.Contracts.MeetupEvents.V1;

namespace MeetupEvents.Test
{
    public class MeetupEventsAggregateTest
    {
        [Fact]
        public void Given_None_Meetup_When_Create_Then_Draft()
        {
            // Arrange - Given - Act - When
            var meetup = CreateMeetup();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Draft);

            meetup.Changes.Should().HaveCount(1);
            var created = meetup.Changes.OfType<MeetupCreated>().FirstOrDefault();
            created.Should().NotBeNull();
            created?.Id.Should().Be(meetup.Id);
            created?.Title.Should().Be(meetup.Details.Title);
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
            var meetup = CreateMeetup().Schedule().MakeOnline().Publish();

            // Act - When
            meetup.Cancel();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Cancelled);

            var changes = meetup.Changes.ToArray();
            changes.Should().HaveCount(5);
            changes[0].Should().BeOfType<MeetupCreated>();
            changes[1].Should().BeOfType<Scheduled>();
            changes[2].Should().BeOfType<MadeOnline>();
            changes[3].Should().BeOfType<Published>();
            changes[4].Should().BeOfType<Canceled>();
        }

        [Fact]
        public void Given_Published_Meetup_When_Start_Then_Started()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().Publish();

            // Act - When
            meetup.Start();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Started);
        }

        [Fact]
        public void Given_Started_Meetup_When_Finish_Then_Finished()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().Publish();
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
            var meetup = CreateMeetup().Schedule().MakeOnline().Publish().Cancel();

            // Act - When
            Action publish = () => meetup.Publish();

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Given_Not_Active_Meetup_When_MakeOnline_Then_InvalidOperation()
        {
            // Arrange - Given
            var meetup = CreateMeetup().Schedule().MakeOnline().Publish().Cancel();

            // Act - When
            Action publish = () => meetup.MakeOnline(new Uri("https://zoom.us/netcorebcn"));

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        MeetupEventAggregate CreateMeetup()
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

        public static MeetupEventAggregate Publish(this MeetupEventAggregate meetup)
        {
            meetup.Publish(DateTimeOffset.UtcNow);
            return meetup;
        }

        public static MeetupEventAggregate Cancel(this MeetupEventAggregate meetup)
        {
            meetup.Cancel(DateTimeOffset.UtcNow);
            return meetup;
        }
    }
}