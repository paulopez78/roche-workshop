using System;
using System.Threading.Tasks;
using MeetupEvents.Contracts.Commands.V1;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Application
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsHttpApi : ControllerBase
    {
        readonly IApplicationService _appService;

        public MeetupEventsHttpApi(MeetupEventsApplicationService applicationService, ILogger<MeetupEventsApplicationService> logger)
        {
            _appService = new ExceptionLoggingMiddleware(logger, applicationService);
        }

        [HttpPost]
        public Task<IActionResult> CreateMeetup(Create command) =>
            HandleHttpCommand(command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Publish command) =>
            HandleHttpCommand(command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Cancel command) =>
            HandleHttpCommand(command);

        public async Task<IActionResult> HandleHttpCommand(object command)
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