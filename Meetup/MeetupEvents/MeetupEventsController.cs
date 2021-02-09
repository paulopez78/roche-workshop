using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventApi : ControllerBase
    {
        readonly MeetupEventsRepository _database;

        public MeetupEventApi(MeetupEventsRepository db)
        {
            _database = db;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeetup(MeetupEvent meetupEvent) =>
            await _database.Add(meetupEvent)
                ? Ok()
                : BadRequest($"Meetup id {meetupEvent.Id} already exists");

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var meetup = await _database.Get(id);

            if (meetup is null)
                return NotFound();

            meetup.Published = true;

            await _database.Commit();

            return Ok();
        }

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Cancel(Guid id) =>
            await _database.Remove(id)
                ? Ok()
                : NotFound();

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await _database.Get(id) switch
            {
                null => NotFound(),
                var meetupEvent => Ok(meetupEvent)
            };

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var meetup = await _database.GetAll();
            return Ok(meetup);
        }
    }

#nullable disable
    public record MeetupEvent(Guid Id, string Title, int Capacity = 10)
    {
        public bool Published { get; set; }
    }
}