using System;

namespace MeetupEvents.Contracts
{
    public static class MeetupEvents
    {
        public static class V1
        {
            public record Created(Guid Id, Guid GroupId, string Title, string Description);

            public record DetailsUpdated(Guid Id, string Title, string Description);

            public record MadeOnline(Guid Id, Uri Url);

            public record MadeOnsite(Guid Id, string Address);

            public record Scheduled(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record Published(Guid Id, Guid GroupId, DateTimeOffset At);

            public record Canceled(Guid Id, Guid GroupId, string Reason, DateTimeOffset At);

            public record Started(Guid Id);

            public record Finished(Guid Id);
        }
    }
}