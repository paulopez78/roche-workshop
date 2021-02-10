using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Queries
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsQueriesApi : ControllerBase
    {
        readonly MeetupEventsDbContext _database;

        public MeetupEventsQueriesApi(MeetupEventsDbContext dbContext)
        {
            _database = dbContext;
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await _database.MeetupEvents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
                switch
                {
                    null            => NotFound(),
                    var meetupEvent => Ok(meetupEvent)
                };

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var meetup = await _database.MeetupEvents.AsNoTracking().ToListAsync();
            return Ok(meetup);
        }
    }
}