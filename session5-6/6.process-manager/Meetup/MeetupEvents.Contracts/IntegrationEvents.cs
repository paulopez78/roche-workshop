using System;

namespace MeetupEvents.Contracts
{
    public static class IntegrationEvents
    {
        public static class V1
        {
            public record MeetupPublished(Guid MeetupEventId, DateTimeOffset PublishedAt);

            public record MeetupCanceled(Guid MeetupEventId, string Reason, DateTimeOffset CancelledAt);

            public record MeetupAttendantAdded(Guid MeetupEventId, Guid MemberId, DateTimeOffset AddedAt);

            public record MeetupAttendantMovedToWaiting(Guid MeetupEventId, Guid MemberId, DateTimeOffset At);
        }

        public static class V2
        {
            public record MeetupPublished(Guid Id, string Title, string Description);
        }
    }
}