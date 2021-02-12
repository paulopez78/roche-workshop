using System;

namespace MeetupEvents.Contracts
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(Guid Id, string Title, string Description, int Capacity);

            public record UpdateDetails(Guid Id, string Title, string Description);

            public record MakeOnline(Guid Id, Uri Url);

            public record MakeOnsite(Guid Id, string Address);

            public record IncreaseCapacity(Guid Id, int ByNumber);

            public record ReduceCapacity(Guid Id, int ByNumber);

            public record Schedule(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record Attend(Guid Id, Guid MemberId);

            public record CancelAttendance(Guid Id, Guid MemberId);

            public record Publish(Guid Id);

            public record Cancel(Guid Id, string Reason);

            public record Start(Guid Id);

            public record Finish(Guid Id);
        }
    }
}