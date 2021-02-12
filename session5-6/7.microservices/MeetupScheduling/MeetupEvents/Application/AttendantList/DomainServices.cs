using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace MeetupEvents.Application.AttendantList
{
    public delegate Task<Guid?> GetMappedId(Guid id);
    public delegate Task<IEnumerable<Guid>> GetAttendingMeetups(Guid groupId, Guid memberId);

    public static class DomainServices
    {
        public static async Task<Guid?> GetAttendingMeetups(Func<DbConnection> getDbConnection, Guid meetupId)
        {
            return null;
        }

        public static async Task<Guid?> GetAttendantListId(Func<DbConnection> getDbConnection, Guid meetupId)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT AL.\"Id\" FROM \"AttendantList\" AL WHERE AL.\"MeetupEventId\" = @id",
                new {Id = meetupId}
            );
        }
    }
}