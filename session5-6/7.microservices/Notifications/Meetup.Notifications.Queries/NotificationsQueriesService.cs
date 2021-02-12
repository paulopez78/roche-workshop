using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MongoDB.Driver;
using Meetup.Notifications.Queries.Contracts.V1;
using MongoDB.Driver.Linq;
using static System.String;
// using static MongoDB.Driver.Builders<Meetup.Notifications.Contracts.ReadModels.V1.Notification>;
using static Meetup.Notifications.Contracts.ReadModels.V1;

namespace Meetup.Notifications.Queries
{
    public class NotificationsQueriesService : NotificationsQueries.NotificationsQueriesBase
    {
        readonly IMongoCollection<Notification> DbCollection;

        public NotificationsQueriesService(IMongoDatabase database)
        {
            DbCollection = database.GetCollection<Notification>(nameof(Notification));
        }

        public override async Task<GetNotificationRequest.Types.GeNotificationReply> Get(GetNotificationRequest request,
            ServerCallContext context)
        {
            var notifications = await DbCollection.AsQueryable().Where(x => x.UserId == request.UserId).ToListAsync();
            // var notifications = await DbCollection.Find(Filter.Eq(x => x.UserId, request.UserId)).ToListAsync();

            return new()
            {
                Notifications =
                {
                    notifications.Select(x => new GetNotificationRequest.Types.Notification
                    {
                        NotificationId   = x.Id,
                        NotificationType = x.NotificationType.ToString(),
                        GroupId          = x.GroupId ?? Empty,
                        MeetupId         = x.MeetupId ?? Empty,
                        MemberId         = x.MemberId ?? Empty,
                        Message          = x.Message ?? Empty
                    })
                }
            };
        }
    }
}