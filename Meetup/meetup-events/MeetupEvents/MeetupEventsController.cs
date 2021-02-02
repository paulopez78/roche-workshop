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
        public IActionResult CreateMeetup(MeetupEvent meetupEvent)
        {
            _database.Add(meetupEvent);
            return Ok();
        }

        [HttpPut("{id:Guid}")]
        public IActionResult Publish(Guid id)
        {
            var meetup = _database.Get(id);
            meetup.Published = true;
            return Ok();
        }

        [HttpDelete("{id:Guid}")]
        public IActionResult Cancel(Guid id)
        {
            _database.Remove(id);
            return Ok();
        }

        [HttpGet("{id:Guid}")]
        public IActionResult Get(Guid id)
        {
            var meetup = _database.Get(id);
            return Ok(meetup);
        }
        
        [HttpGet]
        public IActionResult Get()
        {
            var meetup = _database.GetAll();
            return Ok(meetup);
        }
    }

    public class MeetupEvent
    {
        public Guid   Id        { get; set; }
        public string Title     { get; set; }
        public bool   Published { get; set; }
    }
}