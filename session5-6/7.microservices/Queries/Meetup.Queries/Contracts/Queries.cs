using System;

namespace Meetup.Queries.Contracts
{
    public static class V1
    {
        public record GetMeetupGroup(Guid GroupId);
        
        public record GetMeetupEvent(Guid GroupId, Guid EventId);
        
        public record GetNotifications(Guid UserId);
    }
}