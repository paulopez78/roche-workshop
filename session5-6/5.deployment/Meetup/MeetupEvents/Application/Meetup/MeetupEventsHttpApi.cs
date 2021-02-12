using System.Threading.Tasks;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static MeetupEvents.Contracts.MeetupCommands.V1;

namespace MeetupEvents.Application.Meetup
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsHttpApi : ControllerBase
    {
        readonly IApplicationService _appService;

        public MeetupEventsHttpApi(ApplicationServiceBuilder<MeetupEventsApplicationService> builder) =>
            _appService = builder.WithOutbox().WithExceptionLogging().Build();

        [HttpPost]
        public Task<IActionResult> CreateMeetup(Create command) =>
            _appService.HandleHttp(command);

        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            _appService.HandleHttp(command);

        [HttpPut("schedule")]
        public Task<IActionResult> Schedule(Schedule command) =>
            _appService.HandleHttp(command);

        [HttpPut("online")]
        public Task<IActionResult> MakeOnline(MakeOnline command) =>
            _appService.HandleHttp(command);

        [HttpPut("onsite")]
        public Task<IActionResult> MakeOnsite(MakeOnsite command) =>
            _appService.HandleHttp(command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Publish command) =>
            _appService.HandleHttp(command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Cancel command) =>
            _appService.HandleHttp(command);

        [HttpPut("start")]
        public Task<IActionResult> Start(Start command) =>
            _appService.HandleHttp(command);

        [HttpPut("finish")]
        public Task<IActionResult> Finish(Finish command) =>
            _appService.HandleHttp(command);
    }
}