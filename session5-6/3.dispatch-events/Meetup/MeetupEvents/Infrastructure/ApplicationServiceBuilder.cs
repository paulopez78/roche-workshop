using MeetupEvents.Framework;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class ApplicationServiceBuilder<TApplicationService> where TApplicationService : IApplicationService
    {
        readonly IApplicationService   _applicationService;
        readonly ILogger               _logger;
        readonly EventsDispatcher      _dispatcher;
        readonly MeetupEventsDbContext _dbContext;

        public ApplicationServiceBuilder(
            TApplicationService applicationService,
            EventsDispatcher dispatcher,
            MeetupEventsDbContext dbContext,
            ILogger<TApplicationService> logger)
        {
            _applicationService = applicationService;
            _logger             = logger;
            _dispatcher         = dispatcher;
            _dbContext          = dbContext;
        }

        public IApplicationService Build() =>
            new ExceptionLoggingMiddleware(_logger,
                new EventsDispatcherMiddleware(_applicationService, _dbContext, _dispatcher)
            );
    }
}