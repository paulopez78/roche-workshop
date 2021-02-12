using System.Threading.Tasks;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using static System.Guid;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application.AttendantList
{
    public class AttendantListEventsHandler :
        IEventHandler<Created>,
        IEventHandler<Published>,
        IEventHandler<Canceled>
    {
        readonly IApplicationService _applicationService;

        public AttendantListEventsHandler(ApplicationServiceBuilder<AttendantListApplicationService> builder) =>
            _applicationService = builder.Build();

        public Task Handle(Created created) =>
            _applicationService.Handle(new CreateAttendantList(NewGuid(), created.Id, 10));

        public Task Handle(Published created) =>
            _applicationService.Handle(new Open(created.Id));

        public Task Handle(Canceled created) =>
            _applicationService.Handle(new Close(created.Id));
    }
}