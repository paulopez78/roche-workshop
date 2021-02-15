using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
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

        [Fact]
        public async Task Should_Produce_Concurrency_Problem_When_Attending()
        {
            // arrange
            var meetupId = NewGuid();

            await CreateMeetup(meetupId);
            // eventual consistency, we should retry till is attendant list is opened
            await Task.Delay(1000);

            await Schedule(meetupId);
            await MakeOnline(meetupId);
            await Publish(meetupId);
            // eventual consistency, we should retry till is attendant list is opened
            await Task.Delay(1000);

            await ReduceCapacity(meetupId, byNumber: 8);

            var bob   = NewGuid();
            var alice = NewGuid();
            var joe   = NewGuid();

            // act
            await Attend(meetupId, bob);
            await Attend(meetupId, alice);
            await Attend(meetupId, joe);

            // assert 
            var meetup = await Get(meetupId);
            meetup.Attendants.Should().HaveCount(3);
            meetup.Attendants.Count(x => x.Waiting).Should().Be(1);
        }

        [Fact]
        public async Task Should_Produce_Concurrency_Problem_When_Attend_And_UpdateDetails()
        {
            // arrange
            var meetupId = NewGuid();
            await CreateMeetup(meetupId);
            // eventual consistency, we should retry till is attendant list is opened
            await Task.Delay(1000);

            await Schedule(meetupId);
            await MakeOnline(meetupId);
            await Publish(meetupId);

            // eventual consistency, we should retry till is attendant list is opened
            await Task.Delay(1000);

            var bob   = NewGuid();
            var title = "Microservices Benefits";

            // act
            await Attend(meetupId, bob);
            await UpdateDetails(meetupId, title);

            // assert 
            var meetup = await Get(meetupId);
            meetup.Title.Should().Be(title);
            meetup.Going(bob).Should().BeTrue();
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

        Task<HttpResponseMessage> Attend(Guid meetupId, Guid memberId) =>
            _client.PutAsJsonAsync($"{BaseUrl}/attendants/attend",
                new AttendantListCommands.V1.Attend(meetupId, memberId));

        Task<HttpResponseMessage> ReduceCapacity(Guid meetupId, int byNumber) =>
            _client.PutAsJsonAsync($"{BaseUrl}/attendants/reduce",
                new AttendantListCommands.V1.ReduceCapacity(meetupId, byNumber));

        Task<ReadModels.V1.MeetupEvent> Get(Guid id) =>
            _queries.Handle(new Contracts.Queries.V1.Get(id));

        AsyncRetryPolicy<HttpResponseMessage> Retry()
        {
            Random jitterer = new();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }
    }

    public static class MeetupEventsIntegrationTestExtensions
    {
        public static bool Going(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.MemberId == memberId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.All(x => x.MemberId != memberId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid memberId)
            => meetup.Attendants.Any(x => x.MemberId == memberId && x.Waiting);
    }
}