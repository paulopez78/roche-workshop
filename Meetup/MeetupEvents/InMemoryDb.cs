using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupEvents
{
    public class InMemoryDb
    {
        private List<MeetupEvent> Meetups = new();

        public InMemoryDb()
        {
        }

        public MeetupEvent Get(Guid id)
            => Meetups.SingleOrDefault(x => x.Id == id);

        public IEnumerable<MeetupEvent> GetAll()
            => Meetups;

        public void Add(MeetupEvent meetupEvent)
        {
            Meetups.Add(meetupEvent);
        }

        public void Remove(Guid id)
        {
            var meetupEvent = Get(id);
            Meetups.Remove(meetupEvent);
        }
    }
}