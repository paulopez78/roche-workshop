using System;

namespace MeetupEvents.Contracts
{
    public static class IntegrationEvents
    {
        public static class V1
        {
            public record MeetupEventPublished(Guid MeetupEventId, DateTimeOffset PublishedAt);

            public record MeetupEventCanceled(Guid MeetupEventId, string Reason, DateTimeOffset CancelledAt);

            public record AttendantAdded(Guid MeetupEventId, Guid MemberId, DateTimeOffset AddedAt);

            public record AttendantMovedToWaiting(Guid MeetupEventId, Guid MemberId, DateTimeOffset At);
        }

        public static class V2
        {
            public record MeetupEventPublished(Guid Id, string Title, string Description);
        }
    }
}