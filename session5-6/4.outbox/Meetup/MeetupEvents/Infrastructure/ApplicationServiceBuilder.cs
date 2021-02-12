using System;
using MassTransit;
using MeetupEvents.Framework;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class ApplicationServiceBuilder<TApplicationService> where TApplicationService : IApplicationService
    {
        readonly ILogger               _logger;
        readonly EventsDispatcher      _dispatcher;
        readonly MeetupEventsDbContext _dbContext;
        readonly IPublishEndpoint      _publishEndpoint;
        readonly Func<DateTimeOffset>  _getUtcNow;
        private  IApplicationService   _applicationService;

        public ApplicationServiceBuilder(
            TApplicationService applicationService,
            EventsDispatcher dispatcher,
            MeetupEventsDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            Func<DateTimeOffset> getUtcNow,
            ILogger<TApplicationService> logger)
        {
            _logger             = logger;
            _dispatcher         = dispatcher;
            _dbContext          = dbContext;
            _publishEndpoint    = publishEndpoint;
            _applicationService = applicationService;
            _getUtcNow          = getUtcNow;
        }

        public ApplicationServiceBuilder<TApplicationService> WithExceptionLogging()
        {
            _applicationService = new ExceptionLoggingMiddleware(_logger, _applicationService);
            return this;
        }

        public ApplicationServiceBuilder<TApplicationService> WithEventDispatcher()
        {
            _applicationService = new EventsDispatcherMiddleware(_applicationService, _dbContext, _dispatcher);
            return this;
        }

        public ApplicationServiceBuilder<TApplicationService> WithOutbox()
        {
            _applicationService = new OutboxMiddleware(_applicationService, _dbContext, _publishEndpoint, _getUtcNow);
            return this;
        }

        public IApplicationService Build() => _applicationService;
    }
}