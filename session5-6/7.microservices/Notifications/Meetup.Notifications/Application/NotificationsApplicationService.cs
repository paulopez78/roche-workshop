using System.Linq;
using System.Threading.Tasks;
using Meetup.Notifications.Contracts;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Trace;
using static System.Guid;
using static Meetup.Notifications.Contracts.ReadModels.V1;

namespace Meetup.Notifications.Application
{
    public class NotificationsApplicationService
    {
        public static string ApplicationKey = "meetup_notifications";

        readonly IMongoCollection<Notification> DbCollection;
        readonly GetGroupMembers                GetGroupMembers;
        readonly GetMeetupAttendants            GetMeetupAttendants;
        readonly GetGroupOrganizer              GetGroupOrganizer;
        readonly GetInterestedUsers             GetInterestedUsers;

        public NotificationsApplicationService(IMongoDatabase database, GetGroupMembers getGroupMembers,
            GetMeetupAttendants getMeetupAttendants, GetGroupOrganizer getGroupOrganizer,
            GetInterestedUsers getInterestedUsers)
        {
            DbCollection        = database.GetCollection<Notification>(nameof(Notification));
            GetGroupMembers     = getGroupMembers;
            GetMeetupAttendants = getMeetupAttendants;
            GetGroupOrganizer   = getGroupOrganizer;
            GetInterestedUsers  = getInterestedUsers;
        }

        public async Task Handle(object command)
        {
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(o => o.ServiceName = ApplicationKey)
                .AddZipkinExporter(o => o.ServiceName = ApplicationKey)
                .Build();

            switch (command)
            {
                case Commands.V1.Notify notify:
                    await DbCollection.InsertOneAsync(new()
                    {
                        Id               = NewGuid().ToString(),
                        Message          = notify.Message,
                        NotificationType = NotificationType.Message,
                    });
                    break;

                case Commands.V1.NotifyGroupCreated groupCreated:
                    var users = await GetInterestedUsers(groupCreated.GroupId);

                    if (users.Any())
                        await DbCollection.InsertManyAsync(
                            users.Select(user =>
                                new Notification()
                                {
                                    Id               = NewGuid().ToString(),
                                    UserId           = user.ToString(),
                                    GroupId          = groupCreated.GroupId.ToString(),
                                    NotificationType = NotificationType.NewGroupCreated,
                                })
                        );
                    break;

                case Commands.V1.NotifyMeetupPublished published:
                    var members = await GetGroupMembers(published.GroupId);

                    if (members.Any())
                        await DbCollection.InsertManyAsync(
                            members.Select(member =>
                                new Notification()
                                {
                                    Id               = NewGuid().ToString(),
                                    UserId           = member.ToString(),
                                    MeetupId         = published.MeetupId.ToString(),
                                    NotificationType = NotificationType.MeetupPublished,
                                })
                        );
                    break;

                case Commands.V1.NotifyMeetupCancelled cancelled:
                    var attendants = await GetMeetupAttendants(cancelled.MeetupId);

                    if (attendants.Any())
                        await DbCollection.InsertManyAsync(
                            attendants.Select(attendant =>
                                new Notification()
                                {
                                    Id               = NewGuid().ToString(),
                                    UserId           = attendant.ToString(),
                                    MeetupId         = cancelled.MeetupId.ToString(),
                                    NotificationType = NotificationType.MeetupCancelled,
                                })
                        );
                    break;

                case Commands.V1.NotifyMemberJoined joined:
                    var organizer = await GetGroupOrganizer(joined.GroupId);
                    if (organizer is not null)
                        await DbCollection.InsertOneAsync(new()
                        {
                            Id               = NewGuid().ToString(),
                            UserId           = organizer,
                            GroupId          = joined.GroupId.ToString(),
                            MemberId         = joined.MemberId.ToString(),
                            NotificationType = NotificationType.MemberJoined,
                        });
                    break;

                case Commands.V1.NotifyMemberLeft left:
                    organizer = await GetGroupOrganizer(left.GroupId);
                    if (organizer is not null)
                        await DbCollection.InsertOneAsync(new()
                        {
                            Id               = NewGuid().ToString(),
                            UserId           = organizer,
                            GroupId          = left.GroupId.ToString(),
                            MemberId         = left.MemberId.ToString(),
                            NotificationType = NotificationType.MemberLeft,
                        });
                    break;

                case Commands.V1.NotifyMeetupAttendantGoing going:
                    await DbCollection.InsertOneAsync(new()
                    {
                        Id               = NewGuid().ToString(),
                        UserId           = going.AttendantId.ToString(),
                        MeetupId         = going.MeetupId.ToString(),
                        NotificationType = NotificationType.Attending,
                    });
                    break;

                case Commands.V1.NotifyMeetupAttendantWaiting waiting:
                    await DbCollection.InsertOneAsync(new()
                    {
                        Id               = NewGuid().ToString(),
                        UserId           = waiting.AttendantId.ToString(),
                        MeetupId         = waiting.MeetupId.ToString(),
                        NotificationType = NotificationType.Waiting,
                    });
                    break;
            }
        }
    }
}