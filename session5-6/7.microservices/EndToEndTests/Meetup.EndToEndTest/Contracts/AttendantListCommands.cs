using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListCommands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid Id, Guid MeetupId, int Capacity);

            public record Open (Guid MeetupId);

            public record Close(Guid MeetupId);

            public record Archive(Guid MeetupId);

            public record IncreaseCapacity(Guid MeetupId, int ByNumber);

            public record ReduceCapacity(Guid MeetupId, int ByNumber);

            public record Attend(Guid MeetupId, Guid MemberId);

            public record CancelAttendance(Guid MeetupId, Guid MemberId);

            public record RemoveAttendantFromMeetups(Guid MemberId, Guid GroupId);
        }
    }
}