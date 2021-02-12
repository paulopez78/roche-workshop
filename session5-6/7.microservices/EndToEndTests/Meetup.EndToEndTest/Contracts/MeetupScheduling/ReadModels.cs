#nullable disable
using System;
using System.Collections.Immutable;

namespace Meetup.Scheduling.Contracts
{
    public static class ReadModels
    {
        public static class V1
        {
            public record MeetupEvent ()
            {
                public Guid                      Id              { get; set; }
                public string                    Title           { get; set; }
                public string                    Description     { get; set; }
                public string                    Group           { get; set; }
                public int                       Capacity        { get; set; }
                public string                    Status          { get; set; }
                public DateTimeOffset            Start           { get; set; }
                public DateTimeOffset            End             { get; set; }
                public string                    Location        { get; set; }
                public bool                      Online          { get; set; }
                public Guid?                     AttendantListId { get; set; }
                public ImmutableList<Attendant>? Attendants      { get; set; }

                public string AttendantListStatus { get; set; }
            }

            public record Attendant
            {
                public Guid           UserId  { get; set; }
                public bool           Waiting { get; set; }
                public DateTimeOffset AddedAt { get; set; }
            }

            public record AttendantListReadModel(Guid Id, Guid MeetupEventId, int Capacity, string Status)
            {
                public ImmutableList<Attendant> Attendants { get; init; } = ImmutableList<Attendant>.Empty;
            }

            public record MeetupDetailsEventReadModel (Guid Id, string Title, string Description, string Group,
                int Capacity,
                string Status)
            {
                public DateTimeOffset Start    { get; init; }
                public DateTimeOffset End      { get; init; }
                public string         Location { get; init; }
                public bool           Online   { get; init; }
            }
        }
    }
}