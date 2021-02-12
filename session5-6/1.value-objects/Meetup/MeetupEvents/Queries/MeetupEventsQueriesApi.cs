using System;
using System.Linq;
using System.Threading.Tasks;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
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

        public MeetupEventsQueriesApi(MeetupEventsDbContext dbContext) => _database = dbContext;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var meetups = await _database.MeetupEvents
                .Include(x => x.Attendants)
                .AsNoTracking()
                .ToListAsync();

            return Ok(meetups.Select(Map));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await _database.MeetupEvents
                    .Include(x => x.Attendants)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == id)
                switch
                {
                    null       => NotFound($"Meetup event {id} not found"),
                    var meetup => Ok(Map(meetup)),
                };

        static ReadModels.V1.MeetupEvent Map(MeetupEventAggregate meetup) =>
            new(
                meetup.Id,
                meetup.Details.Title,
                meetup.Details.Description,
                meetup.Status.ToString(),
                meetup.Capacity,
                meetup.Attendants.Select(x => new ReadModels.V1.Attendant(x.MemberId, x.AddedAt, x.Waiting)).ToList()
            );
    }
}