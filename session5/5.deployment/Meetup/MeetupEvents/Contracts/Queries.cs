using System;

namespace MeetupEvents.Contracts
{
    public static class Queries
    {
        public static class V1
        {
            public record Get(Guid Id);

            public record GetByGroup(Guid GroupId);
        }
    }
}