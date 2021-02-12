using System.Threading.Tasks;
using Meetup.GroupManagement.Contracts.Commands.V1;

namespace Meetup.EndToEndTest
{
    public static class GroupManagementExtensions
    {
        public static async Task StartMeetupGroup(this MeetupGroupManagement.MeetupGroupManagementClient client,
            string groupId, string title, string description, string groupSlug, string location,
            UserProfileExtensions.User organizer) =>
            await client.CreateAsync(new()
                {
                    Id          = groupId,
                    Title       = title,
                    Slug        = groupSlug,
                    OrganizerId = organizer.Id.ToString(),
                    Description = description,
                    Location    = location
                }
            );

        public static async Task AddGroupMember(this MeetupGroupManagement.MeetupGroupManagementClient client,
            string groupId, params UserProfileExtensions.User[] users)
        {
            foreach (var user in users)
            {
                await client.JoinAsync(
                    new()
                    {
                        GroupId = groupId,
                        UserId  = user.Id.ToString()
                    }
                );
            }
        }

        public static async Task LeaveGroup(this MeetupGroupManagement.MeetupGroupManagementClient client,
            string groupId, params UserProfileExtensions.User[] users)
        {
            foreach (var user in users)
            {
                await client.LeaveAsync(
                    new()
                    {
                        GroupId = groupId,
                        UserId  = user.Id.ToString()
                    }
                );
            }
        }
    }
}