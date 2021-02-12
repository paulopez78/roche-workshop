using System;
using System.Threading.Tasks;
using MeetupEvents.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    [ApiController]
    [Route("/api/meetup/events/attendants")]
    public class AttendantListHttpApi : ControllerBase
    {
        readonly IApplicationService _appService;

        public AttendantListHttpApi(
            AttendantListApplicationService applicationService,
            ILogger<AttendantListApplicationService> logger) =>
            _appService = new ExceptionLoggingMiddleware(logger, applicationService);

        [HttpPost]
        public Task<IActionResult> Create(Create command) =>
            _appService.HandleHttp(command);

        [HttpPut("open")]
        public Task<IActionResult> Open(Open command) =>
            _appService.HandleHttp(command);

        [HttpPut("close")]
        public Task<IActionResult> Close(Close command) =>
            _appService.HandleHttp(command);

        [HttpPut("archive")]
        public Task<IActionResult> Archive(Archive command) =>
            _appService.HandleHttp(command);

        [HttpPut("reduce")]
        public Task<IActionResult> Reduce(ReduceCapacity command) =>
            _appService.HandleHttp(command);

        [HttpPut("increase")]
        public Task<IActionResult> Increase(IncreaseCapacity command) =>
            _appService.HandleHttp(command);

        [HttpPut("attend")]
        public Task<IActionResult> Attend(Attend command) =>
            _appService.HandleHttp(command);

        [HttpPut("cancel")]
        public Task<IActionResult> CancelAttendance(CancelAttendance command) =>
            _appService.HandleHttp(command);
    }
}