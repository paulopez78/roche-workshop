using System;

namespace MeetupEvents.Contracts
{
    public static class IntegrationEvents
    {
        public static class V1
        {
            public record MeetupCreated(Guid MeetupEventId);

            public record MeetupScheduled(Guid MeetupEventId, DateTimeOffset Start, DateTimeOffset End);

            public record MeetupPublished(Guid MeetupEventId, DateTimeOffset PublishedAt);

            public record MeetupCancelled(Guid MeetupEventId, string Reason, DateTimeOffset CancelledAt);
            
            public record MeetupStarted(Guid MeetupEventId);
            
            public record MeetupFinished(Guid MeetupEventId);

            public record MeetupAttendantAdded(Guid MeetupEventId, Guid MemberId, DateTimeOffset AddedAt);

            public record MeetupAttendantMovedToWaiting(Guid MeetupEventId, Guid MemberId, DateTimeOffset At);
        }

        public static class V2
        {
            public record MeetupPublished(Guid Id, string Title, string Description);
        }
    }
}