#nullable disable
using System;
using System.Collections.Immutable;

namespace Meetup.Scheduling.Queries
{
    public record MeetupEvent ()
    {
        public Guid                      Id                  { get; set; }
        public string                    Title               { get; set; }
        public string                    Description         { get; set; }
        public string                    Group               { get; set; }
        public int                       Capacity            { get; set; }
        public string                    Status              { get; set; }
        public DateTimeOffset            Start               { get; set; }
        public DateTimeOffset            End                 { get; set; }
        public string                    Location            { get; set; }
        public bool                      Online              { get; set; }
        public Guid?                     AttendantListId     { get; set; }
        public string                    AttendantListStatus { get; set; }
        public ImmutableList<Attendant>? Attendants          { get; set; }
    }

    public record Attendant
    {
        public Guid           UserId  { get; set; }
        public bool           Waiting { get; set; }
        public DateTimeOffset AddedAt { get; set; }
    }
}