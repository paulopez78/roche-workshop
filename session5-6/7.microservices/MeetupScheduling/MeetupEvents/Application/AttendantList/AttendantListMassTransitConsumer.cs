using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application.AttendantList
{
    public class AttendantListMassTransitConsumer:
        IConsumer<CreateAttendantList>,
        IConsumer<Open>,
        IConsumer<Close>,
        IConsumer<Archive>,
        IConsumer<Attend>,
        IConsumer<CancelAttendance>,
        IConsumer<IncreaseCapacity>,
        IConsumer<ReduceCapacity>
    {
        readonly IApplicationService _applicationService;

        public AttendantListMassTransitConsumer(ApplicationServiceBuilder<AttendantListApplicationService> builder) =>
            _applicationService= builder.WithOutbox().WithExceptionLogging().Build();

        public Task Consume(ConsumeContext<CreateAttendantList> context) =>
            _applicationService.HandleMassTransit(context.Message);

        public Task Consume(ConsumeContext<Open> context) =>
            _applicationService.HandleMassTransit(context.Message);

        public Task Consume(ConsumeContext<Close> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Archive> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Attend> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<CancelAttendance> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<IncreaseCapacity> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<ReduceCapacity> context) =>
            _applicationService.HandleMassTransit(context.Message);
    }
}