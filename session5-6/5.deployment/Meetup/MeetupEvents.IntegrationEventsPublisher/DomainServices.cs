using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace MeetupEvents.IntegrationEventsPublisher
{
    public delegate Task<Guid?> GetMeetupEventId(Guid id);

    public delegate Task<MeetupDetails?> GetMeetupDetails(Guid id);

    public record MeetupDetails(string Title, string Description);

    public static class DomainServices
    {
        public static async Task<Guid?> GetMeetupEventId(Func<DbConnection> getDbConnection, Guid attendantListId)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT AL.\"MeetupEventId\" FROM \"AttendantList\" AL WHERE AL.\"Id\" = @id",
                new {Id = attendantListId}
            );
        }

        public static async Task<MeetupDetails?> GetMeetupDetails(Func<DbConnection> getDbConnection, Guid meetupId)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<MeetupDetails>(
                "SELECT M.\"Title\", M.\"Description\" FROM \"MeetupEvent\" M WHERE M.\"Id\" = @id",
                new {Id = meetupId}
            );
        }
    }
}