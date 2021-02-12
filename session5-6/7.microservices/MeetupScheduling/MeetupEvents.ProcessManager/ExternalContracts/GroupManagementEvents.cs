using System;

namespace Meetup.GroupManagement.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record MeetupGroupMemberLeft (Guid GroupId, string GroupSlug, Guid UserId, DateTimeOffset LeftAt);
        }
    }
}