#nullable disable
using System;
using System.Collections.Generic;

namespace Meetup.Queries.Contracts
{
    public static class ReadModels
    {
        public static class V1
        {
            public record MeetupGroup
            {
                public Group       Group  { get; set; }
                public List<Event> Events { get; set; } = new();
            }

            public record MeetupEvent()
            {
                public Group           Group   { get; set; }
                public Event           Event   { get; set; }
                public List<Attendant> Going   { get; } = new();
                public List<Attendant> Waiting { get; } = new();
            }

            public record Event(string Id, string Title, string Description, int Capacity, string Status,
                DateTimeOffset Start, DateTimeOffset End, string Location, bool Online);

            public record Group(string Id, string Title, string Description);

            public record Attendant(string UserId, string FirstName, string LastName, DateTimeOffset AddedAt);
        }
    }
}