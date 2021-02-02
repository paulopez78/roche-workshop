using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

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
            Assert.Equal(title, meetup.Title);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        Task<HttpResponseMessage> CreateMeetup(Guid id, string title = "Microservices Failures") =>
            Client.PostAsJsonAsync("/api/meetup/events",
                new MeetupEvent {Id = id, Title = title, Published = false});

        Task<HttpResponseMessage> Publish(Guid id) =>
            Client.PutAsync($"/api/meetup/events/{id}", null);

        Task<HttpResponseMessage> Cancel(Guid id) =>
            Client.DeleteAsync($"/api/meetup/events/{id}");

        Task<MeetupEvent> Get(Guid id) =>
            Client.GetFromJsonAsync<MeetupEvent>($"/api/meetup/events/{id}");
    }
}