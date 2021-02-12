using System;

namespace Meetup.UserProfile.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record UserProfileCreatedOrUpdated(Guid UserId, string FirstName, string LastName, string Email);

            public record UserProfileDeleted(Guid UserId);
        }
    }
}