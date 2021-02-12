using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Meetup.Notifications.Queries.Contracts.V1;
using static Meetup.EndToEndTest.UserProfileExtensions;

namespace Meetup.EndToEndTest
{
    public static class NotificationsExtensions
    {
        public static async Task<IEnumerable<GetNotificationRequest.Types.Notification>> UserNotifications(
            this NotificationsQueries.NotificationsQueriesClient client, User user)
        {
            var notificationReply = await client.GetAsync(new GetNotificationRequest
                {
                    UserId = user.Id.ToString()
                }
            );

            return notificationReply?.Notifications;
        }

        public static async Task<bool> OfType(
            this Task<IEnumerable<GetNotificationRequest.Types.Notification>> getNotifications,
            params NotificationType[] types)
        {
            var notifications = await getNotifications;
            return notifications.All(x => types.Any(y => x.NotificationType == y.ToString()));
        }

        public static async Task ShouldHaveReceived(this Task<bool> @this)
            => (await @this).Should().BeTrue();


        public enum NotificationType
        {
            Message,
            NewGroupCreated,
            MeetupPublished,
            MeetupCancelled,
            MemberJoined,
            MemberLeft,
            Waiting,
            Attending
        }
    }
}