using System;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventApi : ControllerBase
    {
        readonly InMemoryDb _database;

        public MeetupEventApi(InMemoryDb db)
        {
            _database = db;
        }

        [HttpPost]
        public IActionResult CreateMeetup(MeetupEvent meetupEvent) =>
            _database.Add(meetupEvent)
                ? Ok()
                : BadRequest($"Meetup id {meetupEvent.Id} already exists");

        [HttpPut("{id:Guid}")]
        public IActionResult Publish(Guid id)
        {
            var meetup = _database.Get(id);

            if (meetup is null)
                return NotFound();

            // meetup.Published = true;
            // var newMeetup = new MeetupEvent(meetup.Id, meetup.Title, true);

            _database.Replace(meetup, meetup with {Published = true});

            return Ok();
        }

        [HttpDelete("{id:Guid}")]
        public IActionResult Cancel(Guid id) =>
            _database.Remove(id)
                ? Ok()
                : NotFound();

        [HttpGet("{id:Guid}")]
        public IActionResult Get(Guid id) =>
            _database.Get(id) switch
            {
                null            => NotFound(),
                var meetupEvent => Ok(meetupEvent)
            };

        [HttpGet]
        public IActionResult Get()
        {
            var meetup = _database.GetAll();
            return Ok(meetup);
        }
    }

#nullable disable
    public record MeetupEvent(Guid Id, string Title, bool Published = false);
}