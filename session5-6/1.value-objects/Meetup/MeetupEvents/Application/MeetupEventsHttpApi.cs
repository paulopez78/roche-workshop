using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.Commands.V1;

namespace MeetupEvents.Application
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsHttpApi : ControllerBase
    {
        readonly IApplicationService _appService;

        public MeetupEventsHttpApi(
            MeetupEventsApplicationService applicationService,
            ILogger<MeetupEventsApplicationService> logger) =>
            _appService = new ExceptionLoggingMiddleware(logger, applicationService);

        [HttpPost]
        public Task<IActionResult> CreateMeetup(Create command) =>
            Handle(command);

        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            Handle(command);

        [HttpPut("schedule")]
        public Task<IActionResult> Schedule(Schedule command) =>
            Handle(command);

        [HttpPut("online")]
        public Task<IActionResult> MakeOnline(MakeOnline command) =>
            Handle(command);

        [HttpPut("onsite")]
        public Task<IActionResult> MakeOnsite(MakeOnsite command) =>
            Handle(command);

        [HttpPut("reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            Handle(command);

        [HttpPut("increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            Handle(command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Publish command) =>
            Handle(command);

        [HttpPut("attend")]
        public Task<IActionResult> Attend(Attend command) =>
            Handle(command);

        [HttpPut("cancel-attendance")]
        public Task<IActionResult> CancelAttendance(CancelAttendance command) =>
            Handle(command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Cancel command) =>
            Handle(command);

        [HttpPut("start")]
        public Task<IActionResult> Start(Start command) =>
            Handle(command);

        [HttpPut("finish")]
        public Task<IActionResult> Finish(Finish command) =>
            Handle(command);

        public async Task<IActionResult> Handle(object command)
        {
            try
            {
                var commandResult = await _appService.Handle(command);
                return commandResult.Error
                    ? BadRequest(commandResult.ErrorMessage)
                    : Ok();
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}