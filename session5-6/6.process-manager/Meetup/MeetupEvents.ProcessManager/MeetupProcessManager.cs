using System;
using System.Threading.Tasks;
using MassTransit;
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
        IConsumer<MeetupFinished>
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
            => context.Send(new Open(context.Message.MeetupEventId));

        public Task Consume(ConsumeContext<MeetupCancelled> context)
            => context.Send(new Close(context.Message.MeetupEventId));

        public Task Consume(ConsumeContext<MeetupStarted> context) =>
            context.Send(new Close(context.Message.MeetupEventId));

        public Task Consume(ConsumeContext<MeetupFinished> context) =>
            context.Send(new Archive(context.Message.MeetupEventId));
    }
}