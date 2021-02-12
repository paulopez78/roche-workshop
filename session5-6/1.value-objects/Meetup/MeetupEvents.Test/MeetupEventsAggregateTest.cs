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

        [Fact]
        public void Given_Active_Meetup_When_Member_Attend_Then_Going()
        {
            // arrange
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();

            // act
            meetup.Attend(joe, DateTimeOffset.UtcNow);

            // assert
            meetup.Going(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Member_Attending_When_Cancel_Attendance_Then_Removed_From_Attendants()
        {
            // arrange
            var meetup = CreateMeetup().Schedule().MakeOnline().PublishMeetup();
            meetup.Attend(joe, DateTimeOffset.UtcNow);

            // act
            meetup.CancelAttendance(joe);

            // assert
            meetup.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Active_Meetup_With_Enough_Capacity_When_Members_Attend_Then_All_Going()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 3).Schedule().MakeOnline().PublishMeetup();

            // act
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Active_Meetup_Without_Enough_Capacity_When_Members_Attend_Then_Some_Waiting()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 2).Schedule().MakeOnline().PublishMeetup();

            // act
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Waiting(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Active_Meetup_Without_Enough_Capacity_When_Cancel_Attendance_Then_WaitingList_Updated()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 2).Schedule().MakeOnline().PublishMeetup();

            // act
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));
            meetup.CancelAttendance(alice);

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.NotGoing(alice).Should().BeTrue();
            meetup.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Full_Meetup_When_ReduceCapacity_Then_Last_Attendant_Waiting()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 3).Schedule().MakeOnline().PublishMeetup();
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // act
            meetup.ReduceCapacity(1);

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Waiting(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Meetup_With_Waiting_Attendant_When_IncreaseCapacity_Then_Attendant_Going()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 2).Schedule().MakeOnline().PublishMeetup();
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // act
            meetup.IncreaseCapacity(1);

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Going(bob).Should().BeTrue();
        }

        Guid joe   = NewGuid();
        Guid alice = NewGuid();
        Guid bob   = NewGuid();

        MeetupEventAggregate CreateMeetup(int capacity = 10)
        {
            var meetup = new MeetupEventAggregate();
            meetup.Create(NewGuid(), Details.From("Microservices failures", "This is a meetup about ..."), capacity);
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

        public static bool Going(this MeetupEventAggregate aggregate, Guid memberId)
            => aggregate.Attendants.Any(x => x.MemberId == memberId && !x.Waiting);

        public static bool NotGoing(this MeetupEventAggregate aggregate, Guid memberId)
            => aggregate.Attendants.All(x => x.MemberId != memberId);

        public static bool Waiting(this MeetupEventAggregate aggregate, Guid memberId)
            => aggregate.Attendants.Any(x => x.MemberId == memberId && x.Waiting);
    }
}