using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListCommands
    {
        public static class V1
        {
            public record Create(Guid Id, Guid MeetupId, int Capacity);

            public record Open (Guid Id);

            public record Close(Guid Id);

            public record Archive(Guid Id);

            public record IncreaseCapacity(Guid Id, int ByNumber);

            public record ReduceCapacity(Guid Id, int ByNumber);

            public record Attend(Guid Id, Guid MemberId);

            public record CancelAttendance(Guid Id, Guid MemberId);
        }
    }
}