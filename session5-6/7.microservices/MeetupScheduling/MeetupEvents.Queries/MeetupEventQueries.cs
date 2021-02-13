using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using static MeetupEvents.Contracts.Queries.V1;
using static MeetupEvents.Contracts.ReadModels.V1;

namespace MeetupEvents.Queries
{
    public class MeetupEventQueries
    {
        const string BaseQuery =
            "SELECT " +
            "M.\"Id\", M.\"GroupId\", M.\"Title\", M.\"Description\", M.\"Start\", M.\"End\", " +
            "M.\"Url\",M.\"Address\", M.\"IsOnline\", M.\"Status\", " +
            "AL.\"Id\" AS AttendantListId, AL.\"Capacity\", AL.\"Status\" AS AttendantListStatus, " +
            "A.\"Id\", A.\"MemberId\", A.\"AddedAt\", A.\"Waiting\" " +
            "FROM \"MeetupEvent\" M " +
            "LEFT JOIN \"AttendantList\" AL on M.\"Id\" = AL.\"MeetupEventId\" " +
            "LEFT JOIN \"Attendant\" A on AL.\"Id\" = A.\"AttendantListAggregateId\"";

        readonly Func<IDbConnection> _getConnection;

        public MeetupEventQueries(Func<IDbConnection> getConnection)
            => _getConnection = getConnection;

        public async Task<MeetupEvent?> Handle(Get query)
        {
            using var connection = _getConnection();

            MeetupEvent? result = null;

            await connection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>($"{BaseQuery} WHERE M.\"Id\"=@id ORDER BY A.\"AddedAt\"",
                (evt, inv) =>
                {
                    result ??= evt;
                    if (inv is not null) result.Attendants.Add(inv);
                    return result;
                },
                new {query.Id});

            return result;
        }

        public async Task<IEnumerable<MeetupEvent>> Handle(GetByGroup query)
        {
            using var dbConnection = _getConnection();

            var lookup = new Dictionary<Guid, MeetupEvent>();

            await dbConnection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>(
                $"{BaseQuery} WHERE M.\"GroupId\"=@groupId ORDER BY A.\"AddedAt\"",
                (evt, inv) =>
                {
                    if (!lookup.ContainsKey(evt.Id)) lookup.Add(evt.Id, evt);

                    var meetupEvent = lookup[evt.Id];
                    if (inv is not null) meetupEvent.Attendants.Add(inv);
                    return meetupEvent;
                },
                new {query.GroupId});

            return lookup.Values;
        }
    }
}