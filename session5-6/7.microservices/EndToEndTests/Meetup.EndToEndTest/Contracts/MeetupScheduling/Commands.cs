using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.Contracts
{
    public static class MeetupDetailsCommands
    {
        public static class V1
        {
            public record CreateMeetup(Guid EventId, string Group, [Required] string Title, string Description,
                int Capacity);

            public record UpdateDetails(Guid EventId, [Required] string Title, string Description);

            public record MakeOnline(Guid EventId, [Required] string Url);

            public record MakeOnsite(Guid EventId, [Required] string Address);

            public record Schedule(Guid EventId, [Required] DateTimeOffset StartTime, DateTimeOffset EndTime);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId, string Reason);

            public record Start(Guid EventId);

            public record Finish(Guid EventId);
        }
    }

    public static class AttendantListCommands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid Id, Guid MeetupEventId, int Capacity);

            public record Open(Guid MeetupEventId);

            public record Close (Guid MeetupEventId);

            public record Archive (Guid MeetupEventId);

            public record Attend(Guid MeetupEventId, Guid UserId);

            public record DontAttend(Guid MeetupEventId, Guid UserId);

            public record IncreaseCapacity(Guid MeetupEventId, int Capacity);

            public record ReduceCapacity(Guid MeetupEventId, int Capacity);

            public record RemoveAttendantFromMeetups(Guid UserId, string GroupSlug);
        }
    }
}