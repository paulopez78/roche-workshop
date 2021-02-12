using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Framework;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class OutboxMiddleware : IApplicationService
    {
        readonly IApplicationService  _appService;
        readonly DbContext            _dbContext;
        readonly IPublishEndpoint     _publishEndpoint;
        readonly Func<DateTimeOffset> _getUtcNow;

        public OutboxMiddleware(IApplicationService applicationService, DbContext dbContext,
            IPublishEndpoint publishEndpoint, Func<DateTimeOffset> getUtcNow)
        {
            _appService      = applicationService;
            _dbContext       = dbContext;
            _publishEndpoint = publishEndpoint;
            _getUtcNow       = getUtcNow;
        }

        public async Task<CommandResult> Handle(object command)
        {
            CommandResult result;
            List<Outbox>  outbox;

            await using (var _ = await _dbContext.Database.BeginTransactionAsync())
            {
                result = await _appService.Handle(command);

                outbox = result.Changes.Select(x => Outbox.From(result.Id, x)).ToList();

                await _dbContext.Set<Outbox>().AddRangeAsync(outbox);

                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
            }

            await Dispatch();

            return result;

            async Task Dispatch()
            {
                await Task.WhenAll(
                    outbox.Select(x => _publishEndpoint.Publish(x.DomainEvent))
                );

                foreach (var domainEvent in outbox)
                    domainEvent.DispatchedAt = _getUtcNow();

                await _dbContext.SaveChangesAsync();
            }
        }
    }

    #nullable disable
    public record Outbox
    {
        public Guid            AggregateId  { get; set; }
        public string          MessageType  { get; set; }
        public string          Payload      { get; set; }
        public DateTimeOffset? DispatchedAt { get; set; }

        public object DomainEvent => JsonSerializer.Deserialize(Payload, Type.GetType(MessageType));

        public static Outbox From(Guid aggregateId, object domainEvent) =>
            new()
            {
                AggregateId = aggregateId,
                Payload     = JsonSerializer.Serialize(domainEvent),
                MessageType = $"{domainEvent.GetType().FullName}, {domainEvent.GetType().Assembly.FullName}",
            };
    }
}