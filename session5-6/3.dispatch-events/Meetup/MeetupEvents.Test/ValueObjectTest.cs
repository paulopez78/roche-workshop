using System;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;

namespace MeetupEvents.Test
{
    public class ValueObjectTest
    {
        [Fact]
        public void DateTime_ValueObject_Test()
        {
            var now = DateTime.UtcNow;

            // make illegal state irrepresentable 
            var isDateTime = DateTime.TryParse("this is not a datetime", out var result);
            Assert.False(isDateTime);

            var dateTime1 = now.AddDays(1).AddMinutes(1);
            var dateTime2 = now.AddDays(1).AddMinutes(1);

            Assert.Equal(dateTime1, dateTime2);
        }

        [Fact]
        public void Can_Create_Schedule()
        {
            var now   = DateTime.UtcNow;
            var start = now.AddDays(7);

            var sut = ScheduleDateTime.From(() => now, start, 2);

            sut.Start.Should().Be(start);
            sut.End.Should().Be(start.AddHours(2));
        }

        [Fact]
        public void Create_Schedule_InvalidDuration_Should_Throw()
        {
            var now   = DateTime.UtcNow;
            var start = now.AddDays(7);

            Action sut = () => ScheduleDateTime.From(() => now, start, -1);

            sut.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Create_Schedule_InvalidStart_Should_Throw()
        {
            var now   = DateTime.UtcNow;
            var start = now.AddDays(-7);

            Action sut = () => ScheduleDateTime.From(() => now, start, -1);

            sut.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void Create_Schedule_InvalidEnd_Should_Throw()
        {
            var now   = DateTime.UtcNow;
            var start = now.AddDays(7);

            Action sut = () => ScheduleDateTime.From(() => now, start, start.AddHours(-2));

            sut.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void Should_Create_PositiveNumber(int number)
            => Assert.Equal<PositiveNumber>(number, PositiveNumber.From(number));

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(0)]
        public void Should_Create_PositiveNumber_With_Zero_Value(int number)
            => Assert.Equal<PositiveNumber>(0, PositiveNumber.From(number));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_Not_Create_Address(string address)
        {
            Address CreateAddress() => Address.From(address);
            Assert.ThrowsAny<Exception>(CreateAddress);
        }

        [Theory]
        [InlineData("Address 1")]
        public void Should_Create_Address(string address)
            => Assert.Equal(address, Address.From(address));

        [Fact]
        public void Should_Create_Online_Location()
        {
            var expected = new Uri("http://zoom.us/netcorebcn");
            var sut      = Location.OnLine(expected);
            sut.IsOnline.Should().BeTrue();
            Assert.Equal(expected, sut.Url);
        }

        [Fact]
        public void Should_Create_OnSite_Location()
        {
            var expected = Address.From("Address 1");
            var sut      = Location.OnSite(expected);
            Assert.False(sut.IsOnline);
            Assert.Equal(expected, sut.Address);
        }
    }
}