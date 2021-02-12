using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Meetup.GroupManagement.Contracts.Queries.V1;
using Meetup.Notifications.Queries.Contracts.V1;
using Meetup.UserProfile.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.ReadModels.V1;

namespace Meetup.Queries
{
    public class MeetupQueryHandler
    {
        readonly HttpClient                                          SchedulingClient;
        readonly MeetupGroupQueries.MeetupGroupQueriesClient         GroupClient;
        readonly NotificationsQueries.NotificationsQueriesClient     NotificationsClient;
        readonly UserProfile.Contracts.UserProfile.UserProfileClient UserProfileClient;

        public MeetupQueryHandler(
            HttpClient schedulingClient,
            MeetupGroupQueries.MeetupGroupQueriesClient groupClient,
            NotificationsQueries.NotificationsQueriesClient notificationsClient,
            UserProfile.Contracts.UserProfile.UserProfileClient userProfileClient)
        {
            SchedulingClient    = schedulingClient;
            GroupClient         = groupClient;
            NotificationsClient = notificationsClient;
            UserProfileClient   = userProfileClient;
        }

        public async Task<IActionResult> Handle(object query)
        {
            switch (query)
            {
                case V1.GetMeetupGroup groupQuery:
                    // parallel get
                    var group        = GetGroup(groupQuery.Group);
                    var meetupEvents = GetMeetupEvents(groupQuery.Group);
                    await Task.WhenAll(group, meetupEvents);

                    return new ObjectResult(
                        new ReadModels.V1.MeetupGroup
                        {
                            Group  = Map(group.Result),
                            Events = meetupEvents.Result.Select(Map).ToList()
                        }
                    );

                case V1.GetMeetupEvent eventQuery:
                    // parallel get
                    var meetupGroup = GetGroup(eventQuery.Group);
                    var meetupEvent = GetMeetupEvent(eventQuery);
                    await Task.WhenAll(meetupEvent, meetupGroup);

                    var result = new ReadModels.V1.MeetupEvent
                    {
                        Group = Map(meetupGroup.Result),
                        Event = Map(meetupEvent.Result),
                    };

                    var users = await GetUsers(meetupEvent.Result.Attendants.Select(x => x.UserId));

                    foreach (var attendant in meetupEvent.Result.Attendants)
                    {
                        var foundUser = users.FirstOrDefault(x => x.UserId == attendant.UserId.ToString());
                        if (foundUser is not null)
                        {
                            var combined = new ReadModels.V1.Attendant(
                                foundUser.UserId, foundUser.FirstName, foundUser.LastName, attendant.AddedAt
                            );

                            if (attendant.Waiting)
                                result.Waiting.Add(combined);
                            else
                                result.Going.Add(combined);
                        }
                    }

                    return new OkObjectResult(result);

                default:
                    throw new ApplicationException("query handler not found");
            }
        }

        ReadModels.V1.Event Map(MeetupEvent source)
            => new(source.Id.ToString(), source.Title, source.Description, source.Capacity, source.Status, source.Start,
                source.End, source.Location, source.Online);

        ReadModels.V1.Group Map(GetGroup.Types.Group source)
            => new(source.Id, source.Title, source.Description);

        async Task<MeetupEvent> GetMeetupEvent(V1.GetMeetupEvent query)
        {
            var queryResponse = await SchedulingClient.GetAsync($"api/meetup/{query.Group}/events/{query.EventId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<MeetupEvent>();
            return queryResult;
        }

        async Task<IEnumerable<MeetupEvent>> GetMeetupEvents(string group)
        {
            var queryResponse = await SchedulingClient.GetAsync($"api/meetup/{group}/");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<IEnumerable<MeetupEvent>>();
            return queryResult;
        }

        async Task<GetGroup.Types.Group> GetGroup(string group)
        {
            var result = await GroupClient.GetAsync(new GetGroup {GroupSlug = group});
            return result.Group;
        }

        async Task<IEnumerable<User>> GetUsers(IEnumerable<Guid> userIds)
        {
            if (!userIds.Any()) return Enumerable.Empty<User>();
            var users = await UserProfileClient.GetUsersAsync(new GetUsersRequest
            {
                Users =
                {
                    userIds.Select(x => x.ToString())
                }
            });
            return users.Users;
        }
    }
}