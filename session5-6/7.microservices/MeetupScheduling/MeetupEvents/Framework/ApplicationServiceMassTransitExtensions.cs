using System;
using System.Threading.Tasks;

namespace MeetupEvents.Framework
{
    public static class ApplicationServiceMassTransitExtensions
    {
        public static async Task HandleMassTransit(this IApplicationService appService, object command)
        {
            var commandResult = await appService.Handle(command);
            if (commandResult.Error)
                throw new InvalidOperationException(commandResult.ErrorMessage);
        }
    }
}