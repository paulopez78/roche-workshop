using System.Threading.Tasks;
using MassTransit;
using Meetup.Notifications.Contracts;
using GroupManagementEvents = Meetup.GroupManagement.Contracts.Events.V1;

namespace Meetup.Notifications.Application
{
    public class NotificationsEventHandler :
            IConsumer<GroupManagementEvents.MeetupGroupFounded>,
            IConsumer<GroupManagementEvents.MeetupGroupMemberJoined>,
            IConsumer<GroupManagementEvents.MeetupGroupMemberLeft>
    {
        readonly NotificationsApplicationService ApplicationService;

        public NotificationsEventHandler(NotificationsApplicationService applicationService)
            => ApplicationService = applicationService;

        public Task Consume(ConsumeContext<GroupManagementEvents.MeetupGroupFounded> context)
            => ApplicationService.Handle(new Commands.V1.NotifyGroupCreated(context.Message.GroupId));

        public Task Consume(ConsumeContext<GroupManagementEvents.MeetupGroupMemberJoined> context)
            => ApplicationService.Handle(
                new Commands.V1.NotifyMemberJoined(context.Message.GroupId, context.Message.UserId)
            );

        public Task Consume(ConsumeContext<GroupManagementEvents.MeetupGroupMemberLeft> context)
            => ApplicationService.Handle(
                new Commands.V1.NotifyMemberLeft(context.Message.GroupId, context.Message.UserId)
            );
    }
}