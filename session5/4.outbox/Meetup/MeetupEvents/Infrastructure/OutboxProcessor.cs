using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class OutboxProcessor : BackgroundService
    {
        readonly IServiceProvider         _serviceProvider;
        readonly Func<DateTimeOffset>     _getUtcNow;
        readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(
            IServiceProvider serviceProvider,
            Func<DateTimeOffset> getUtcNow,
            ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _getUtcNow       = getUtcNow;
            _logger          = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessage(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error processing outbox");
                }

                await Task.Delay(10_000, stoppingToken);
            }
        }

        private async Task ProcessMessage(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var sp = scope.ServiceProvider;

            var dbContext       = sp.GetRequiredService<MeetupEventsDbContext>();
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();

            var outbox = await dbContext.Set<Outbox>()
                .Where(x => x.DispatchedAt == null)
                .ToListAsync(stoppingToken);

            foreach (var message in outbox)
            {
                var domainEvent = JsonSerializer.Deserialize(message.Payload, Type.GetType(message.MessageType)!);
                if (domainEvent is null) return;

                await publishEndpoint.Publish(domainEvent, stoppingToken);
                message.DispatchedAt = _getUtcNow();
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}