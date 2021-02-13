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
        public static async Task<IEnumerable<Guid>> GetAttendingMeetups(Func<DbConnection> getDbConnection,
            Guid groupId, Guid memberId)
        {
            await using var connection = getDbConnection();

            return await connection.QueryAsync<Guid>(
                "SELECT M.\"Id\" FROM \"MeetupEvent\" M " +
                "LEFT JOIN \"AttendantList\" AL ON M.\"Id\"= AL.\"MeetupEventId\" " +
                "LEFT JOIN \"Attendant\" A on AL.\"Id\" = A.\"AttendantListAggregateId\" " +
                "WHERE M.\"Status\"!='Finished' AND M.\"GroupId\" = @groupId AND A.\"MemberId\" = @memberId",
                new {GroupId = groupId, MemberId = memberId}
            );
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