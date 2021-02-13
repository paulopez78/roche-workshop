using System;
using System.Threading.Tasks;
using Meetup.Queries.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Queries
{
    [Route("/api")]
    [ApiController]
    public class MeetupQueryApi : ControllerBase
    {
        readonly MeetupQueryHandler QueryHandler;

        public MeetupQueryApi(MeetupQueryHandler handler) => QueryHandler = handler;

        [HttpGet("meetup/{groupId:guid}")]
        public Task<IActionResult> GetByGroup(Guid groupId)
            => QueryHandler.Handle(new V1.GetMeetupGroup(groupId));

        [HttpGet("meetup/{groupId:guid}/{eventId:guid}")]
        public Task<IActionResult> GetById(Guid groupId, Guid eventId)
            => QueryHandler.Handle(new V1.GetMeetupEvent(groupId, eventId));

        [HttpGet("notifications/{userId:guid}")]
        public Task<IActionResult> GetById([FromRoute] V1.GetNotifications query)
            => QueryHandler.Handle(query);
    }
}