using System;
using System.Threading.Tasks;
using MeetupEvents.Framework;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class ExceptionLoggingMiddleware : IApplicationService
    {
        readonly ILogger             _logger;
        readonly IApplicationService _appService;

        public ExceptionLoggingMiddleware(ILogger logger, IApplicationService applicationService)
        {
            _logger     = logger;
            _appService = applicationService;
        }

        public async Task<CommandResult> Handle(object command)
        {
            try
            {
                // before
                _logger.LogInformation($"Executing command {command}");

                // next
                var result = await _appService.Handle(command);

                // after
                _logger.LogInformation($"Command {command} executed");

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error executing {command}");
                throw;
            }
        }
    }
}