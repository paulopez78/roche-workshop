using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MeetupEvents.Contracts;
using Xunit;
using Xunit.Abstractions;
using static MeetupEvents.Contracts.Commands.V1;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebTestFixture>
    {
        public MeetupEventIntegrationTest(WebTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture        = fixture;
            Fixture.Output = testOutputHelper;
            Client         = Fixture.CreateClient();
        }

        readonly WebTestFixture Fixture;
        readonly HttpClient     Client;

        [Fact]
        public async Task Should_Create_Meetup()
        {
            // arrange
            var meetupId = NewGuid();

            // act
            var response = await CreateMeetup(meetupId);

            // assert 
            var meetup = await Get(meetupId);
            meetup.Title.Should().Be(Title);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Not_Duplicate_Meetup()
        {
            // arrange
            var meetupId = NewGuid();
            await CreateMeetup(meetupId);

            // act
            var response = await CreateMeetup(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Publish_Meetup()
        {
            // arrange
            var meetupId = NewGuid();
            await CreateMeetup(meetupId);
            await Schedule(meetupId);
            await MakeOnline(meetupId);

            // act
            var response = await Publish(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Cancel_Meetup()
        {
            // arrange
            var meetupId = NewGuid();
            await CreateMeetup(meetupId);
            await Schedule(meetupId);
            await MakeOnline(meetupId);
            await Publish(meetupId);

            // act
            var response = await Cancel(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Not_Cancel_None_Existing_Meetup()
        {
            // arrange
            var meetupId = NewGuid();

            // act
            var response = await Cancel(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        const string BaseUrl     = "/api/meetup/events";
        const string Title       = "Microservices Failures";
        const string Description = "This is a talk about ...";

        Task<HttpResponseMessage> CreateMeetup(Guid id, int capacity = 10) =>
            Client.PostAsJsonAsync(BaseUrl, new Create(id, Title, Description, capacity));

        Task<HttpResponseMessage> Publish(Guid id) =>
            Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(id));

        Task<HttpResponseMessage> Schedule(Guid id)
        {
            var now = DateTimeOffset.UtcNow;
            return Client.PutAsJsonAsync($"{BaseUrl}/schedule",
                new Schedule(id, now.AddMonths(1), now.AddMonths(1).AddHours(2)));
        }

        Task<HttpResponseMessage> MakeOnline(Guid id) =>
            Client.PutAsJsonAsync($"{BaseUrl}/online", new MakeOnline(id, new Uri("https://zoom.us/netcore")));

        Task<HttpResponseMessage> Cancel(Guid id, string reason = "") =>
            Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(id, reason));

        Task<ReadModels.V1.MeetupEvent> Get(Guid id) =>
            Client.GetFromJsonAsync<ReadModels.V1.MeetupEvent>($"/api/meetup/events/{id}");

        // AsyncRetryPolicy<HttpResponseMessage> Retry()
        // {
        //     Random jitterer = new();
        //     return HttpPolicyExtensions
        //         .HandleTransientHttpError()
        //         .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(jitterer.Next(50, 200)));
        // }
    }

    public static class MeetupEventsIntegrationTestExtensions
    {
        public static bool Going(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.Id == memberId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.All(x => x.Id != memberId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.Id == memberId && x.Waiting);
    }
}