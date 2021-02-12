using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Meetup.GroupManagement.Contracts.Queries.V1;
using MeetupEvents.Contracts;
using static System.Linq.Enumerable;

namespace Meetup.Notifications.Application
{
    public delegate Task<IEnumerable<string>> GetGroupMembers(Guid groupId);

    public delegate Task<IEnumerable<Guid>> GetMeetupAttendants(Guid meetupId);

    public delegate Task<IEnumerable<Guid>> GetInterestedUsers(Guid groupId);

    public delegate Task<string> GetGroupOrganizer(Guid groupId);

    public static class ExternalServices
    {
        public static GetGroupMembers GetGroupMembers(Func<MeetupGroupQueries.MeetupGroupQueriesClient> getClient)
            => async groupId =>
            {
                var group = await getClient().GetAsync(new GetGroup {GroupId = groupId.ToString()});
                return group?.Group?.Members.Select(x => x.UserId);
            };

        public static GetGroupOrganizer GetGroupOrganizer(Func<MeetupGroupQueries.MeetupGroupQueriesClient> getClient)
            => async groupId =>
            {
                var group = await getClient().GetAsync(new GetGroup {GroupId = groupId.ToString()});
                return group?.Group?.OrganizerId;
            };

        public static GetMeetupAttendants GetMeetupAttendants(Func<HttpClient> getClient)
            => async (meetupId) =>
            {
                var response = await getClient().GetAsync($"/api/meetup/events/{meetupId}");
                response.EnsureSuccessStatusCode();

                var meetup = await response.Content.ReadFromJsonAsync<ReadModels.V1.MeetupEvent>();
                return meetup?.Attendants?.Select(x => x.MemberId);
            };

        public static GetInterestedUsers GetInterestedUsers()
            => _ => Task.FromResult(Empty<Guid>());
    }
}