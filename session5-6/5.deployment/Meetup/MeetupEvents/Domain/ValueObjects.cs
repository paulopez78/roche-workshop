using System;

namespace MeetupEvents.Domain
{
    public record Location
    {
        public bool     IsOnline { get; }
        public Uri?     Url      { get; }
        public Address? Address  { get; }

        Location(Address address)
        {
            IsOnline = false;
            Address  = address;
        }

        Location(Uri url)
        {
            IsOnline = true;
            Url      = url;
        }

        public static Location OnSite(Address address)
            => new(address);

        public static Location OnLine(Uri url)
            => new(url);
    }

    public record ScheduleDateTime
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End   { get; }

        ScheduleDateTime(DateTimeOffset start, DateTimeOffset end)
        {
            Start = start;
            End   = end;
        }

        public static ScheduleDateTime From(Func<DateTimeOffset> getNow, DateTimeOffset start, DateTimeOffset end)
        {
            var now = getNow();

            if (start <= now)
                throw new ArgumentException($"Schedule start time {start} can not be before now {now}");

            if (start >= end)
                throw new ArgumentException($"Schedule start time {start} can not be after end {end}");

            return new ScheduleDateTime(start, end);
        }

        public static ScheduleDateTime From(Func<DateTimeOffset> getNow, DateTimeOffset start,
            PositiveNumber durationInHours) =>
            From(getNow, start, start.AddHours(durationInHours));
    }

    public record Details
    {
        public string  Title       { get; }
        public string  Description { get; }

        Details(string title, string description)
        {
            Title = (string.IsNullOrWhiteSpace(title), title.Length) switch
            {
                (true, _)    => throw new ArgumentNullException(nameof(title)),
                (false, >50) => throw new ArgumentException($"{nameof(title)} has 50 max length"),
                (_, _)       => title!
            };

            Description = (string.IsNullOrWhiteSpace(description), description.Length) switch
            {
                (true, _)       => throw new ArgumentNullException(nameof(description)),
                (false, > 4096) => throw new ArgumentException($"{nameof(description)} has 4096 max length"),
                (_, _)          => description!
            };
        }

        public static Details From(string title, string description) => new(title, description);
    }

    public record Address
    {
        public string Value { get; }

        Address(string value) => Value = value;

        public static Address From(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address));

            return new(address);
        }

        public static implicit operator string(Address address) => address.Value;
        public static implicit operator Address(string address) => From(address);
    }

    public record PositiveNumber
    {
        public int Value { get; }

        PositiveNumber(int value) => Value = value;

        public static PositiveNumber From(int number)
        {
            // if (number < 0)
            //     throw new ArgumentException($"{nameof(number)} is not positive");

            return new(number < 0 ? 0 : number);
        }

        public static implicit operator int(PositiveNumber number) => number.Value;
        public static implicit operator PositiveNumber(int number) => From(number);
    }
}