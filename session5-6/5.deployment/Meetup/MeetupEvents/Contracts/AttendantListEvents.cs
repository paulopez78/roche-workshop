using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListEvents
    {
        public static class V1
        {
            public record AttendantListCreated(Guid Id, Guid MeetupEventId, int Capacity);

            public record Opened(Guid Id, DateTimeOffset At);

            public record Closed(Guid Id, DateTimeOffset At);

            public record Archived(Guid Id, DateTimeOffset At);

            public record CapacityIncreased(Guid Id, int ByNumber);

            public record CapacityReduced(Guid Id, int ByNumber);

            public record AttendantAdded(Guid Id, Guid MemberId, DateTimeOffset At);

            public record AttendantMovedToWaiting(Guid Id, Guid MemberId, DateTimeOffset At);

            public record AttendantRemoved(Guid Id, Guid MemberId);
        }
    }
}