using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MeetupEvents.Application;
using MeetupEvents.Contracts;
using MeetupEvents.Contracts.Commands.V1;
using MeetupEvents.Domain;
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
            GrpcClient     = Fixture.CreateGrpcClient();
        }

        readonly WebTestFixture                                Fixture;
        readonly HttpClient                                    Client;
        readonly MeetupEventsService.MeetupEventsServiceClient GrpcClient;

        [Fact]
        public async Task Should_Create_Meetup_Grpc()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            var title    = "Microservices Failures";

            // act
            var commandReply = await GrpcClient.CreateMeetupAsync(new Create
                {Id = meetupId.ToString(), Capacity = 10, Title = title});

            // assert 
            // var meetup = await Get(meetupId);
            // Assert.Equal(title, meetup.Title);
            
            Assert.Equal(meetupId.ToString(), commandReply.Id);
        }

        [Fact]
        public async Task Should_Create_Meetup()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            var title    = "Microservices Failures";

            // act
            var response = await CreateMeetup(meetupId, title);

            // assert 
            // var meetup = await Get(meetupId);
            // Assert.Equal(title, meetup.Title);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Should_Return_BadRequest_Create_Meetup()
        {
            // arrange
            var meetupId = Guid.NewGuid();
            var title    = "Microservices Failures";
            await CreateMeetup(meetupId, title);

            // act
            var response = await CreateMeetup(meetupId, title);

            // assert 
            // var meetup = await Get(meetupId);
            // Assert.Equal(title, meetup.Title);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        [Fact]
        public async Task Should_Return_NotFound_Cancel_Meetup()
        {
            // arrange
            var meetupId = Guid.NewGuid();

            // act
            var response = await Cancel(meetupId);

            // assert 
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        private const string BaseUrl = "/api/meetup/events";

        Task<HttpResponseMessage> CreateMeetup(Guid id, string title = "Microservices Failures", int capacity = 10) =>
            Client.PostAsJsonAsync(BaseUrl, new Create {Id = id.ToString(), Title = title, Capacity = capacity});

        Task<HttpResponseMessage> Publish(Guid id) =>
            Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish {Id = id.ToString()});

        Task<HttpResponseMessage> Cancel(Guid id) =>
            Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel {Id = id.ToString()});

        // Task<MeetupEventEntity> Get(Guid id) =>
        //     Client.GetFromJsonAsync<MeetupEventEntity>($"/api/meetup/events/{id}");
    }
}