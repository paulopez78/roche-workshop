using System;
using System.Collections.Generic;

namespace MeetupEvents.Contracts
{
    public static class ReadModels
    {
        public static class V1
        {
            public record MeetupEvent (
                Guid Id, string Title, string Description, string Status, int Capacity, List<Attendant> Attendants
            );

            public record Attendant(Guid Id, DateTimeOffset At, bool Waiting);
        }
    }
}