using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Meetup.UserProfile.Contracts;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Meetup.UserProfile
{
    public class IntegrationEventsPublisher : BackgroundService
    {
        readonly IMongoCollection<Data.UserProfile> DbCollection;
        readonly IBus                               Bus;

        public IntegrationEventsPublisher(IMongoDatabase database, IBus bus)
        {
            DbCollection = database.GetCollection<Data.UserProfile>(nameof(UserProfile));
            Bus          = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = new ChangeStreamOptions {FullDocument = ChangeStreamFullDocumentOption.UpdateLookup};
            var pipeline =
                new EmptyPipelineDefinition<ChangeStreamDocument<Data.UserProfile>>().Match(
                    "{ operationType: { $in: [ 'insert', 'update', 'delete' ] } }");

            using var changeStream = DbCollection.Watch(pipeline, options);

            await changeStream.ForEachAsync(async change =>
            {
                var userProfile = change?.FullDocument;
                
                object @event = (change?.OperationType) switch
                {
                    ChangeStreamOperationType.Delete =>
                        new Events.V1.UserProfileDeleted(
                            Guid.Parse(change.DocumentKey["_id"].ToString())
                        ),
                    ChangeStreamOperationType.Update or ChangeStreamOperationType.Insert =>
                        new Events.V1.UserProfileCreatedOrUpdated(
                            Guid.Parse(userProfile.Id), userProfile.FirstName, userProfile.LastName, userProfile.Email
                        ),
                    _ => null
                };

                // publish integration event
                if (@event is not null)
                    await Bus.Publish(@event, stoppingToken);
            }, cancellationToken: stoppingToken);
        }
    }
}