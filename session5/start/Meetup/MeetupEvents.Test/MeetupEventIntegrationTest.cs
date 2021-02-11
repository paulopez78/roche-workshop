using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MeetupEvents.Contracts;
using Xunit;
using Xunit.Abstractions;
using static MeetupEvents.Contracts.Commands.V1;

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
            var meetupId = Guid.NewGuid();
            var title    = "Microservices Failures";

            // act
            var response = await CreateMeetup(meetupId, title);

            // assert 
            var meetup = await Get(meetupId);

            meetup.Title.Should().Be(title);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Create_Meetup_And_Return_BadRequest()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            var title    = "Microservices Failures";
            await CreateMeetup(meetupId, title);

            // act
            var response = await CreateMeetup(meetupId, title);

            // assert 
            var meetup = await Get(meetupId);
            meetup.Title.Should().Be(title);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Publish_Meetup()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            await CreateMeetup(meetupId);

            // act
            var response = await Publish(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Cancel_Meetup()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            await CreateMeetup(meetupId);
            await Publish(meetupId);

            // act
            var response = await Cancel(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Should_Not_Cancel_Meetup_And_Return_BadRequest()
        {
            // arrange
            var meetupId = Guid.NewGuid();

            // act
            var response = await Cancel(meetupId);

            // assert 
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private const string BaseUrl = "/api/meetup/events";

        Task<HttpResponseMessage> CreateMeetup(
            Guid id, string title = null, string description = null, int capacity = 10) =>
            Client.PostAsJsonAsync(BaseUrl,
                new Create(id, title ?? "Microservices Failures", description ?? "This is a talk about ...", capacity));

        Task<HttpResponseMessage> Publish(Guid id) =>
            Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(id));

        Task<HttpResponseMessage> Cancel(Guid id, string reason = "") =>
            Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(id, reason));

        Task<ReadModels.V1.MeetupEvent> Get(Guid id) =>
            Client.GetFromJsonAsync<ReadModels.V1.MeetupEvent>($"/api/meetup/events/{id}");
    }
}