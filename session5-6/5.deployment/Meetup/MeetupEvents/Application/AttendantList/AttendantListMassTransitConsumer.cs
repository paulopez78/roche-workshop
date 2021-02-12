using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using static System.Guid;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application.AttendantList
{
    public class AttendantListMassTransitConsumer :
        IConsumer<MeetupCreated>,
        IConsumer<Published>,
        IConsumer<Canceled>
    {
        readonly IApplicationService _applicationService;

        public AttendantListMassTransitConsumer(ApplicationServiceBuilder<AttendantListApplicationService> builder) =>
            _applicationService = builder.Build();

        public Task Consume(ConsumeContext<MeetupCreated> context) => 
            _applicationService.Handle(new CreateAttendantList(NewGuid(), context.Message.Id, 10));

        public Task Consume(ConsumeContext<Published> context) =>
            _applicationService.Handle(new Open(context.Message.Id));

        public Task Consume(ConsumeContext<Canceled> context) =>
            _applicationService.Handle(new Close(context.Message.Id));
    }
}