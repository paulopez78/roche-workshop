using System;
using System.Threading.Tasks;

namespace Meetup.EndToEndTest
{
    public static class UserProfileExtensions
    {
        public static async Task CreateUserProfile(
            this UserProfile.Contracts.UserProfile.UserProfileClient client,
            params User[] users)
        {
            foreach (var user in users)
            {
                await client.CreateOrUpdateAsync(new()
                    {
                        UserId    = user.Id.ToString(),
                        FirstName = user.Name,
                        LastName  = user.Lastname,
                        Email     = user.Email,
                    }
                );
            }
        }

        public record User (Guid Id, string Name, string Lastname, string Email);
    }
}