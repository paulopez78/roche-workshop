using System.Linq;
using System.Threading.Tasks;
using MeetupEvents.Framework;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class EventsDispatcherMiddleware : IApplicationService
    {
        readonly IApplicationService _appService;
        readonly DbContext           _dbContext;
        readonly EventsDispatcher    _dispatcher;

        public EventsDispatcherMiddleware(
            IApplicationService applicationService,
            DbContext dbContext,
            EventsDispatcher dispatcher)
        {
            _appService = applicationService;
            _dbContext  = dbContext;
            _dispatcher = dispatcher;
        }

        public async Task<CommandResult> Handle(object command)
        {
            if (_dbContext.Database.CurrentTransaction is not null)
                return await DispatchEvents();

            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            var result = await DispatchEvents();

            await _dbContext.Database.CommitTransactionAsync();
            return result;

            async Task<CommandResult> DispatchEvents()
            {
                var commandResult = await _appService.Handle(command);
                await Task.WhenAll(
                    commandResult.Changes.Select(_dispatcher.Publish)
                );
                return commandResult;
            }
        }
    }
}