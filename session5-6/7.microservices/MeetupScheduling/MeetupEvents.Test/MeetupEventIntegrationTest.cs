using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Guid;
using MeetupEvents.Contracts;
using MeetupEvents.Queries;
using static MeetupEvents.Contracts.MeetupCommands.V1;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebTestFixture>
    {
        readonly HttpClient         _client;
        readonly MeetupEventQueries _queries;

        public MeetupEventIntegrationTest(WebTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            fixture.Output = testOutputHelper;
            _client        = fixture.CreateClient();
            _queries       = fixture.Queries;
        }

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

        Task<HttpResponseMessage> CreateMeetup(Guid id) =>
            _client.PostAsJsonAsync(BaseUrl, new Create(id, NewGuid(), Title, Description));

        Task<HttpResponseMessage> UpdateDetails(Guid id, string title, string description = null) =>
            _client.PutAsJsonAsync($"{BaseUrl}/details", new UpdateDetails(id, title, description ?? Description));

        Task<HttpResponseMessage> Publish(Guid id) =>
            _client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(id));

        Task<HttpResponseMessage> Schedule(Guid id)
        {
            var now = DateTimeOffset.UtcNow;
            return _client.PutAsJsonAsync($"{BaseUrl}/schedule",
                new Schedule(id, now.AddMonths(1), now.AddMonths(1).AddHours(2)));
        }

        Task<HttpResponseMessage> MakeOnline(Guid id) =>
            _client.PutAsJsonAsync($"{BaseUrl}/online", new MakeOnline(id, new Uri("https://zoom.us/netcore")));

        Task<HttpResponseMessage> Cancel(Guid id, string reason = "") =>
            _client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(id, reason));

        Task<ReadModels.V1.MeetupEvent> Get(Guid id) =>
            _queries.Handle(new Contracts.Queries.V1.Get(id));
    }
}