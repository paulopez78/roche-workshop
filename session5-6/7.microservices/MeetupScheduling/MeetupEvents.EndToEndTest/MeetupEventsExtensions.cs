using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeetupEvents.Contracts;

namespace MeetupEvents.EndToEndTest
{
    public static class MeetupEventsExtensions
    {
        const string BaseUrl = "/api/meetup/events";

        public static Task<HttpResponseMessage> CreateMeetup(this HttpClient client, Guid id, Guid groupId,
            string title, string description) =>
            client.PostAsJsonAsync(BaseUrl, new MeetupCommands.V1.Create(id, groupId, title, description));

        public static Task<HttpResponseMessage> UpdateDetails(this HttpClient client, Guid id, string title,
            string description) =>
            client.PutAsJsonAsync($"{BaseUrl}/details",
                new MeetupCommands.V1.UpdateDetails(id, title, description));

        public static Task<HttpResponseMessage> Publish(this HttpClient client, Guid id) =>
            client.PutAsJsonAsync($"{BaseUrl}/publish", new MeetupCommands.V1.Publish(id));

        public static Task<HttpResponseMessage> Schedule(this HttpClient client, Guid id, DateTimeOffset start,
            DateTimeOffset end) =>
            client.PutAsJsonAsync($"{BaseUrl}/schedule", new MeetupCommands.V1.Schedule(id, start, end));

        public static Task<HttpResponseMessage> MakeOnline(this HttpClient client, Guid id, string url) =>
            client.PutAsJsonAsync($"{BaseUrl}/online",
                new MeetupCommands.V1.MakeOnline(id, new Uri(url)));

        public static Task<HttpResponseMessage> Cancel(this HttpClient client, Guid id, string reason = "") =>
            client.PutAsJsonAsync($"{BaseUrl}/cancel", new MeetupCommands.V1.Cancel(id, reason));

        public static async Task Attend(this HttpClient client, Guid meetupId, params Guid[] members)
        {
            foreach (var memberId in members)
                await client.PutAsJsonAsync($"{BaseUrl}/attendants/attend",
                    new AttendantListCommands.V1.Attend(meetupId, memberId));
        }
        
        public static async Task CancelAttendance(this HttpClient client, Guid meetupId, params Guid[] members)
        {
            foreach (var memberId in members)
                await client.PutAsJsonAsync($"{BaseUrl}/attendants/cancel",
                    new AttendantListCommands.V1.CancelAttendance(meetupId, memberId));
        }

        public static Task<HttpResponseMessage> ReduceCapacity(this HttpClient client, Guid meetupId, int byNumber) =>
            client.PutAsJsonAsync($"{BaseUrl}/attendants/reduce",
                new AttendantListCommands.V1.ReduceCapacity(meetupId, byNumber));

        public static Task<HttpResponseMessage> IncreaseCapacity(this HttpClient client, Guid meetupId, int byNumber) =>
            client.PutAsJsonAsync($"{BaseUrl}/attendants/increase",
                new AttendantListCommands.V1.IncreaseCapacity(meetupId, byNumber));
        
        public static Task<ReadModels.V1.MeetupEvent> Get(this HttpClient client, Guid meetupId) =>
            client.GetFromJsonAsync<ReadModels.V1.MeetupEvent>($"{BaseUrl}/{meetupId}" );
        
        public static bool Going(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.MemberId == memberId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.All(x => x.MemberId != memberId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.MemberId == memberId && x.Waiting);
    }
}