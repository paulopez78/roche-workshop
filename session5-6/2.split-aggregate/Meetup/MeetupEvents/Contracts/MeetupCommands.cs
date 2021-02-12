using System;

namespace MeetupEvents.Contracts
{
    public static class MeetupCommands
    {
        public static class V1
        {
            public record Create(Guid Id, Guid GroupId, string Title, string Description);

            public record UpdateDetails(Guid Id, string Title, string Description);

            public record MakeOnline(Guid Id, Uri Url);

            public record MakeOnsite(Guid Id, string Address);

            public record Schedule(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record Publish(Guid Id);

            public record Cancel(Guid Id, string Reason);

            public record Start(Guid Id);

            public record Finish(Guid Id);
        }
    }
}