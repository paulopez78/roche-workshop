using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Meetup.GroupManagement.Application;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Middleware
{
    public class OutboxNotificationsHandler : INotificationHandler<GroupCreated>, INotificationHandler<MemberJoined>,
        INotificationHandler<MemberLeft>
    {
        readonly DbContext DbContext;

        public OutboxNotificationsHandler(MeetupGroupManagementDbContext dbContext) => DbContext = dbContext;

        public Task Handle(GroupCreated notification, CancellationToken cancellationToken)
            => DbContext.Set<Outbox>().AddRangeAsync(Outbox.From(notification.Id, notification));

        public Task Handle(MemberJoined notification, CancellationToken cancellationToken)
            => DbContext.Set<Outbox>().AddRangeAsync(Outbox.From(notification.GroupId, notification));

        public Task Handle(MemberLeft notification, CancellationToken cancellationToken)
            => DbContext.Set<Outbox>().AddRangeAsync(Outbox.From(notification.GroupId, notification));
    }
}