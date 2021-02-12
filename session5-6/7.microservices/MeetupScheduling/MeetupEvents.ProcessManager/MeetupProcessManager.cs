using System;
using System.Threading.Tasks;
using MassTransit;
using Meetup.GroupManagement.Contracts;
using Meetup.Notifications.Contracts;
using static MeetupEvents.Contracts.MeetupCommands.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;
using static MeetupEvents.Contracts.IntegrationEvents.V1;

namespace MeetupEvents.ProcessManager
{
    public class MeetupProcessManager :
        IConsumer<MeetupCreated>,
        IConsumer<MeetupScheduled>,
        IConsumer<MeetupPublished>,
        IConsumer<MeetupCancelled>,
        IConsumer<MeetupStarted>,
        IConsumer<MeetupFinished>,
        IConsumer<MeetupAttendantAdded>,
        IConsumer<MeetupAttendantMovedToWaiting>,
        IConsumer<Events.V1.MeetupGroupMemberLeft>
    {
        public Task Consume(ConsumeContext<MeetupCreated> context)
            => context.Send(new CreateAttendantList(Guid.NewGuid(), context.Message.MeetupEventId, 0));

        public Task Consume(ConsumeContext<MeetupScheduled> context)
        {
            var address = new Uri("queue:meetup_events-commands");

            return Task.WhenAll(
                context.ScheduleSend(address,
                    context.Message.Start.DateTime,
                    new Start(context.Message.MeetupEventId)
                ),
                context.ScheduleSend(address,
                    context.Message.End.DateTime,
                    new Finish(context.Message.MeetupEventId)
                )
            );
        }

        public Task Consume(ConsumeContext<MeetupPublished> context)
            => Task.WhenAll(
                context.Send(
                    new Open(context.Message.MeetupEventId)
                ),
                context.Send(
                    new Commands.V1.NotifyMeetupPublished(context.Message.MeetupEventId, context.Message.GroupId)
                )
            );

        public Task Consume(ConsumeContext<MeetupCancelled> context)
            => Task.WhenAll(
                context.Send(
                    new Close(context.Message.MeetupEventId)
                ),
                context.Send(
                    new Commands.V1.NotifyMeetupCancelled(context.Message.MeetupEventId, context.Message.GroupId,
                        context.Message.Reason)
                )
            );

        public Task Consume(ConsumeContext<MeetupStarted> context) =>
            context.Send(new Close(context.Message.MeetupEventId));

        public Task Consume(ConsumeContext<MeetupFinished> context) =>
            context.Send(new Archive(context.Message.MeetupEventId));

        public Task Consume(ConsumeContext<Events.V1.MeetupGroupMemberLeft> context) =>
            context.Send(
                new RemoveAttendantFromMeetups(
                    context.Message.UserId, context.Message.GroupId)
            );

        public Task Consume(ConsumeContext<MeetupAttendantMovedToWaiting> context) =>
            context.Send(
                new Commands.V1.NotifyMeetupAttendantWaiting(context.Message.MeetupEventId, context.Message.MemberId)
            );

        public Task Consume(ConsumeContext<MeetupAttendantAdded> context) =>
            context.Send(
                new Commands.V1.NotifyMeetupAttendantGoing(context.Message.MeetupEventId, context.Message.MemberId)
            );
    }
}