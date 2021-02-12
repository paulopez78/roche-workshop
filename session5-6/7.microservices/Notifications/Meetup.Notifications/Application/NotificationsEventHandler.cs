using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Notifications.Contracts;
using GroupManagementEvents = Meetup.GroupManagement.Contracts.Events.V1;
using SchedulingEvents = Meetup.Scheduling.Contracts.Events.V1;

namespace Meetup.Notifications.Application
{
    public class NotificationsEventHandler :
            IConsumer<GroupManagementEvents.MeetupGroupFounded>,
            IConsumer<GroupManagementEvents.MeetupGroupMemberJoined>,
            IConsumer<GroupManagementEvents.MeetupGroupMemberLeft>
        // IConsumer<SchedulingEvents.MeetupPublished>,
        // IConsumer<SchedulingEvents.MeetupCancelled>,
        // IConsumer<SchedulingEvents.MeetupAttendantAdded>,
        // IConsumer<SchedulingEvents.MeetupAttendantsAddedToWaitingList>,
        // IConsumer<SchedulingEvents.MeetupAttendantsRemovedFromWaitingList>
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

        // public Task Consume(ConsumeContext<SchedulingEvents.MeetupPublished> context)
        //     => ApplicationService.Handle(
        //         new Commands.V1.NotifyMeetupPublished(context.Message.MeetupId, context.Message.GroupSlug)
        //     );
        //
        // public Task Consume(ConsumeContext<SchedulingEvents.MeetupCancelled> context)
        //     => ApplicationService.Handle(
        //         new Commands.V1.NotifyMeetupCancelled(context.Message.MeetupId, context.Message.GroupSlug)
        //     );
        //
        // public Task Consume(ConsumeContext<SchedulingEvents.MeetupAttendantAdded> context)
        //     => ApplicationService.Handle(
        //         new Commands.V1.NotifyMeetupAttendantGoing(context.Message.MeetupEventId, context.Message.AttendantId)
        //     );
        //
        // public Task Consume(ConsumeContext<SchedulingEvents.MeetupAttendantsAddedToWaitingList> context)
        //     => Task.WhenAll(
        //         context.Message.Attendants.Select(attendantId =>
        //             context.Send(
        //                 new Commands.V1.NotifyMeetupAttendantWaiting(context.Message.MeetupEventId, attendantId))
        //         )
        //     );
        //
        // public Task Consume(ConsumeContext<SchedulingEvents.MeetupAttendantsRemovedFromWaitingList> context)
        //     => Task.WhenAll(
        //         context.Message.Attendants.Select(attendantId =>
        //             context.Send(new Commands.V1.NotifyMeetupAttendantGoing(context.Message.MeetupEventId, attendantId))
        //         )
        //     );
    }
}