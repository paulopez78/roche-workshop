using System;

namespace Meetup.Notifications.Contracts
{
    public static class Commands
    {
        public static class V1
        {
            public record Notify(Guid UserId, string Message);

            public record NotifyGroupCreated(Guid GroupId);

            public record NotifyMemberJoined(Guid GroupId, Guid MemberId);

            public record NotifyMemberLeft(Guid GroupId, Guid MemberId);

            public record NotifyMeetupPublished(Guid MeetupId, string GroupSlug);
            
            public record NotifyMeetupCancelled(Guid MeetupId, string GroupSlug, string Message);

            public record NotifyMeetupAttendantWaiting(Guid MeetupId, Guid AttendantId);

            public record NotifyMeetupAttendantGoing(Guid MeetupId, Guid AttendantId);
        }
    }
}