using System;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventsUnitTest
    {
        [Fact]
        public void Given_Draft_Meetup_When_Publish_Then_Published()
        {
            // Arrange - Given
            var meetup = new MeetupEventEntity();
            meetup.Create(NewGuid(), "Microservices failures", 10);

            // Act - When
            meetup.Publish();

            // Assert - Then 
            meetup.Status.Should().Be(MeetupEventStatus.Published);
            Assert.Equal(MeetupEventStatus.Published, meetup.Status);
        }

        [Fact]
        public void Given_Cancelled_Meetup_When_Publish_Then_InvalidOperation()
        {
            // Arrange - Given
            var meetup = new MeetupEventEntity();
            meetup.Create(NewGuid(), "Microservices failures", 10);
            meetup.Publish();
            meetup.Cancel();

            // Act - When
            Action publish = () => meetup.Publish();

            // Assert - Then 
            publish.Should().ThrowExactly<InvalidOperationException>();
        }
    }
}