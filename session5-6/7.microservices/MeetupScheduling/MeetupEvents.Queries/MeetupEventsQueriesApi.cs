using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static MeetupEvents.Contracts.Queries.V1;

namespace MeetupEvents.Queries
{
    [ApiController]
    [Route("/api/meetup/")]
    public class MeetupEventsQueriesApi : ControllerBase
    {
        readonly MeetupEventQueries _queries;

        public MeetupEventsQueriesApi(MeetupEventQueries queries) => _queries = queries;

        [HttpGet("{groupId:Guid}/events")]
        public async Task<IActionResult> GetByGroup(Guid groupId)
            => Ok(
                await _queries.Handle(new GetByGroup(groupId))
            );

        [HttpGet("events/{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await _queries.Handle(new Get(id))
                switch
                {
                    null       => NotFound($"Meetup event {id} not found"),
                    var meetup => Ok(meetup),
                };
    }
}