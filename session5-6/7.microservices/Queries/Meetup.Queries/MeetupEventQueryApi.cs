using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Queries
{
    [Route("/api")]
    [ApiController]
    public class MeetupQueryApi : ControllerBase
    {
        readonly MeetupQueryHandler QueryHandler;

        public MeetupQueryApi(MeetupQueryHandler handler) => QueryHandler = handler;

        [HttpGet("meetup/{group}")]
        public Task<IActionResult> GetByGroup(string group)
            => QueryHandler.Handle(new V1.GetMeetupGroup(group));

        [HttpGet("meetup/{group}/{eventId:guid}")]
        public Task<IActionResult> GetById(string group, Guid eventId)
            => QueryHandler.Handle(new V1.GetMeetupEvent(group, eventId));

        [HttpGet("notifications/{userId:guid}")]
        public Task<IActionResult> GetById([FromRoute] V1.GetNotifications query)
            => QueryHandler.Handle(query);
    }
}