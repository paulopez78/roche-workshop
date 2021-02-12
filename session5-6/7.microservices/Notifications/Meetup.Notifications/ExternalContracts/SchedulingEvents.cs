using System;

namespace Meetup.Scheduling.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record MeetupAttendantAdded(Guid MeetupEventId, Guid AttendantId);

            public record MeetupAttendantsRemovedFromWaitingList(Guid MeetupEventId, Guid[] Attendants);

            public record MeetupAttendantsAddedToWaitingList(Guid MeetupEventId, Guid[] Attendants);

            public record MeetupPublished(Guid MeetupId, string GroupSlug);

            public record MeetupCancelled(Guid MeetupId, string GroupSlug, string Reason);
        }
    }
}