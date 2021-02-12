using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.MeetupCommands.V1;

namespace MeetupEvents.Application.Meetup
{
    public class MeetupEventsMassTransitConsumer:
        IConsumer<Create>,
        IConsumer<Schedule>,
        IConsumer<MakeOnline>,
        IConsumer<MakeOnsite>,
        IConsumer<UpdateDetails>,
        IConsumer<Publish>,
        IConsumer<Cancel>,
        IConsumer<Start>,
        IConsumer<Finish>
    {
        readonly IApplicationService _applicationService;

        public MeetupEventsMassTransitConsumer(ApplicationServiceBuilder<MeetupEventsApplicationService> builder) =>
            _applicationService= builder.WithOutbox().WithExceptionLogging().Build();

        public Task Consume(ConsumeContext<Create> context) =>
            _applicationService.HandleMassTransit(context.Message);

        public Task Consume(ConsumeContext<Schedule> context) =>
            _applicationService.HandleMassTransit(context.Message);

        public Task Consume(ConsumeContext<MakeOnline> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<MakeOnsite> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<UpdateDetails> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Publish> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Cancel> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Start> context) =>
            _applicationService.HandleMassTransit(context.Message);
        
        public Task Consume(ConsumeContext<Finish> context) =>
            _applicationService.HandleMassTransit(context.Message);
    }
}