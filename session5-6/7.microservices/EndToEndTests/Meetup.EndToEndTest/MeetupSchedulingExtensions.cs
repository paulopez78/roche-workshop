using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;
using static Meetup.Scheduling.Contracts.ReadModels.V1;
using static Meetup.EndToEndTest.UserProfileExtensions;

namespace Meetup.EndToEndTest
{
    public static class MeetupSchedulingExtensions
    {
        const string BaseUrl = "/api/meetup";

        public static async Task<HttpResponseMessage> CreateMeetup(
            this HttpClient client, Guid meetupId, string group, string title, string description, int capacity)
            => await client.Post("events/details", new CreateMeetup(
                meetupId, group, title, description, capacity)
            );

        public static Task<HttpResponseMessage> Schedule(this HttpClient client, Guid eventId, DateTimeOffset start,
            DateTimeOffset end)
            => client.Put($"events/schedule", new Schedule(eventId, start, end));

        public static Task<HttpResponseMessage> MakeOnline(this HttpClient client, Guid eventId, string url)
            => client.Put($"events/makeonline", new MakeOnline(eventId, url));

        public static Task<HttpResponseMessage> Publish(this HttpClient client, Guid eventId)
            => client.Put($"events/publish", new Publish(eventId));

        public static async Task Attend(this HttpClient client, Guid eventId, params User[] users)
        {
            foreach (var user in users)
                await client.Put($"attendants/add", new Attend(eventId, user.Id));
        }

        public static Task<HttpResponseMessage> IncreaseCapacity(this HttpClient client, Guid eventId, int byNumber) =>
            client.Put($"attendants/capacity/increase", new IncreaseCapacity(eventId, byNumber));

        public static Task<HttpResponseMessage> ReduceCapacity(this HttpClient client, Guid eventId, int byNumber) =>
            client.Put($"attendants/capacity/reduce", new ReduceCapacity(eventId, byNumber));

        public static Task<HttpResponseMessage> DontAttend(this HttpClient client, Guid eventId, User user) =>
            client.Put($"attendants/remove", new DontAttend(eventId, user.Id));

        public static async Task<MeetupEvent> Get(this HttpClient client,
            string groupSlug, Guid eventId, int delay = 2000)
        {
            // eventual consistency hack, better to poll (query) checking consistency with a timeout
            await Task.Delay(delay);

            var queryResponse = await client.GetAsync($"{BaseUrl}/{groupSlug}/events/{eventId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<MeetupEvent>();
            return queryResult;
        }

        static Task<HttpResponseMessage> Put(this HttpClient client, string url, object command) =>
            client.PutAsync($"{BaseUrl}/{url}", Serialize(command));

        static Task<HttpResponseMessage> Post(this HttpClient client, string url, object command) =>
            client.PostAsync($"{BaseUrl}/{url}", Serialize(command));

        static StringContent Serialize(object command)
            => new(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        public static bool Going(this MeetupEvent meetup, User user) =>
            meetup.Attendants.Any(x => x.UserId == user.Id && !x.Waiting);

        public static bool Waiting(this MeetupEvent meetup, User user) =>
            meetup.Attendants.Any(x => x.UserId == user.Id && x.Waiting);

        public static bool NotGoing(this MeetupEvent meetup, User user) =>
            meetup.Attendants.All(x => x.UserId != user.Id);
    }
}