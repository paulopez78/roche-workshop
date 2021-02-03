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

        public MeetupEvent? Get(Guid id)
            => Meetups.SingleOrDefault(x => x.Id == id);

        public IEnumerable<MeetupEvent> GetAll()
            => Meetups;

        public bool Add(MeetupEvent meetupEvent)
        {
            var meetup = Get(meetupEvent.Id);
            // already exists
            if (meetup is not null)
                return false;
                
            Meetups.Add(meetupEvent);
            return true;
        }

        public void Replace(MeetupEvent previous, MeetupEvent meetupEvent)
        {
            Meetups.Remove(previous);
            Meetups.Add(meetupEvent);
        }

        public bool Remove(Guid id)
        {
            var meetupEvent = Get(id);

            if (meetupEvent == null)
                return false;

            Meetups.Remove(meetupEvent);
            return true;
        }
    }
}