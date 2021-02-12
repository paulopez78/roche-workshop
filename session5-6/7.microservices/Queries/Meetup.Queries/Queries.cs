using System;

namespace Meetup.Queries
{
    public static class V1
    {
        public record GetMeetupGroup(string Group);
        
        public record GetMeetupEvent(string Group, Guid EventId);
        
        public record GetNotifications(Guid UserId);
    }
}