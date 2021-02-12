using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents.Framework
{
    public static class ApplicationServiceHttpExtensions
    {
        public static async Task<IActionResult> HandleHttp(this IApplicationService appService, object command)
        {
            try
            {
                var commandResult = await appService.Handle(command);
                return commandResult.Error
                    ? new BadRequestObjectResult(commandResult.ErrorMessage)
                    : new OkObjectResult(commandResult.Id);
            }
            catch (InvalidOperationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (ArgumentException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}