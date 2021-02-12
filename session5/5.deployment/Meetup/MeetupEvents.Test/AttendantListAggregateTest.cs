using System;
using System.Linq;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class AttendantListAggregateTest
    {
        [Fact]
        public void Given_None_List_When_Create_Then_Closed()
        {
            var list = Create();
            list.Status.Should().Be(AttendantListStatus.Closed);
        }

        [Fact]
        public void Given_Opened_List_When_Close_Then_Closed()
        {
            // Arrange - Given
            var list = Create().Open();

            // Act - When
            list.Close();

            // Assert - Then 
            list.Status.Should().Be(AttendantListStatus.Closed);
        }
        
        [Fact]
        public void Given_Closed_List_When_Archive_Then_Archived()
        {
            // Arrange - Given
            var list = Create().Open();

            // Act - When
            list.Archive();

            // Assert - Then 
            list.Status.Should().Be(AttendantListStatus.Archived);
        }

        [Fact]
        public void Given_Archived_List_When_Open_Then_InvalidOperation()
        {
            // Arrange - Given
            var list = Create().Archive();

            // Act - When
            Action open = () => list.Open();

            // Assert - Then 
            open.Should().ThrowExactly<InvalidOperationException>();
        }
        
        [Fact]
        public void Given_Archived_List_When_Close_Then_InvalidOperation()
        {
            // Arrange - Given
            var list = Create().Archive();

            // Act - When
            Action close = () => list.Close();

            // Assert - Then 
            close.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Given_Active_Meetup_When_Member_Attend_Then_Going()
        {
            // arrange
            var list = Create().Open();

            // act
            list.Attend(joe, DateTimeOffset.UtcNow);

            // assert
            list.Going(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Member_Attending_When_Cancel_Attendance_Then_Removed_From_Attendants()
        {
            // arrange
            var list = Create().Open();
            list.Attend(joe, DateTimeOffset.UtcNow);

            // act
            list.CancelAttendance(joe);

            // assert
            list.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Open_List_With_Enough_Capacity_When_Members_Attend_Then_All_Going()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var list = Create(capacity: 3).Open();

            // act
            list.Attend(joe, now);
            list.Attend(alice, now.AddSeconds(1));
            list.Attend(bob, now.AddSeconds(2));

            // assert
            list.Going(joe).Should().BeTrue();
            list.Going(alice).Should().BeTrue();
            list.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Open_List_Without_Enough_Capacity_When_Members_Attend_Then_Some_Waiting()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var list = Create(capacity: 2).Open();

            // act
            list.Attend(joe, now);
            list.Attend(alice, now.AddSeconds(1));
            list.Attend(bob, now.AddSeconds(2));

            // assert
            list.Going(joe).Should().BeTrue();
            list.Going(alice).Should().BeTrue();
            list.Waiting(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Open_List_Without_Enough_Capacity_When_Cancel_Attendance_Then_WaitingList_Updated()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var list = Create(capacity: 2).Open();

            // act
            list.Attend(joe, now);
            list.Attend(alice, now.AddSeconds(1));
            list.Attend(bob, now.AddSeconds(2));
            list.CancelAttendance(alice);

            // assert
            list.Going(joe).Should().BeTrue();
            list.NotGoing(alice).Should().BeTrue();
            list.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Full_Meetup_When_ReduceCapacity_Then_Last_Attendant_Waiting()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var list = Create(capacity: 3).Open();
            list.Attend(joe, now);
            list.Attend(alice, now.AddSeconds(1));
            list.Attend(bob, now.AddSeconds(2));

            // act
            list.ReduceCapacity(1);

            // assert
            list.Going(joe).Should().BeTrue();
            list.Going(alice).Should().BeTrue();
            list.Waiting(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Meetup_With_Waiting_Attendant_When_IncreaseCapacity_Then_Attendant_Going()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var list = Create(capacity: 2).Open();
            list.Attend(joe, now);
            list.Attend(alice, now.AddSeconds(1));
            list.Attend(bob, now.AddSeconds(2));

            // act
            list.IncreaseCapacity(1);

            // assert
            list.Going(joe).Should().BeTrue();
            list.Going(alice).Should().BeTrue();
            list.Going(bob).Should().BeTrue();
        }

        Guid joe   = NewGuid();
        Guid alice = NewGuid();
        Guid bob   = NewGuid();

        AttendantListAggregate Create(int capacity = 10)
        {
            var attendantList = new AttendantListAggregate();
            attendantList.Create(NewGuid(), NewGuid(), capacity);
            return attendantList;
        }
    }

    public static class AttendantListAggregateTestExtensions
    {
        public static AttendantListAggregate Open(this AttendantListAggregate attendantList)
        {
            attendantList.Open(DateTimeOffset.UtcNow);
            return attendantList;
        }
        
        public static AttendantListAggregate Close(this AttendantListAggregate attendantList)
        {
            attendantList.Close(DateTimeOffset.UtcNow);
            return attendantList;
        }
        
        public static AttendantListAggregate Archive(this AttendantListAggregate attendantList)
        {
            attendantList.Archive(DateTimeOffset.UtcNow);
            return attendantList;
        }

        public static bool Going(this AttendantListAggregate aggregate, Guid memberId)
            => aggregate.Attendants.Any(x => x.MemberId == memberId && !x.Waiting);

        public static bool NotGoing(this AttendantListAggregate aggregate, Guid memberId)
            => aggregate.Attendants.All(x => x.MemberId != memberId);

        public static bool Waiting(this AttendantListAggregate aggregate, Guid memberId)
            => aggregate.Attendants.Any(x => x.MemberId == memberId && x.Waiting);
    }
}