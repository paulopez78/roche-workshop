using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Meetup.GroupManagement.Contracts.Queries.V1;
using Meetup.Scheduling.Queries;
using static System.Linq.Enumerable;

namespace Meetup.Notifications.Application
{
    public delegate Task<IEnumerable<string>> GetGroupMembers(string groupSlug);

    public delegate Task<IEnumerable<Guid>> GetMeetupAttendants(Guid meetupId, string groupSlug);

    public delegate Task<IEnumerable<Guid>> GetInterestedUsers(Guid groupId);

    public delegate Task<string> GetGroupOrganizer(Guid groupId);

    public static class ExternalServices
    {
        public static GetGroupMembers GetGroupMembers(Func<MeetupGroupQueries.MeetupGroupQueriesClient> getClient)
            => async groupSlug =>
            {
                var group = await getClient().GetAsync(new GetGroup {GroupSlug = groupSlug});
                return group?.Group?.Members.Select(x => x.UserId);
            };

        public static GetGroupOrganizer GetGroupOrganizer(Func<MeetupGroupQueries.MeetupGroupQueriesClient> getClient)
            => async groupId =>
            {
                var group = await getClient().GetAsync(new GetGroup {GroupId = groupId.ToString()});
                return group?.Group?.OrganizerId;
            };

        public static GetMeetupAttendants GetMeetupAttendants(Func<HttpClient> getClient)
            => async (meetupId, groupSlug) =>
            {
                var response = await getClient().GetAsync($"/api/meetup/{groupSlug}/events/{meetupId}");
                response.EnsureSuccessStatusCode();

                var meetup = await response.Content.ReadFromJsonAsync<MeetupEvent>();
                return meetup?.Attendants?.Select(x => x.UserId);
            };

        public static GetInterestedUsers GetInterestedUsers()
            => _ => Task.FromResult(Empty<Guid>());
    }
}