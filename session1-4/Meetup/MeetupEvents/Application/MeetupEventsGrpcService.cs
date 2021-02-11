using System;
using System.Threading.Tasks;
using Grpc.Core;
using MeetupEvents.Contracts.Commands.V1;
using MeetupEvents.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Application
{
    public class MeetupEventsGrpcService : MeetupEventsService.MeetupEventsServiceBase
    {
        readonly IApplicationService _appService;

        public MeetupEventsGrpcService(MeetupEventsApplicationService applicationService,
            ILogger<MeetupEventsApplicationService> logger)
        {
            _appService = new ExceptionLoggingMiddleware(logger, applicationService);
        }

        public override Task<CommandReply> CreateMeetup(Create command, ServerCallContext context) =>
            HandleGrpcCommand(command);

        public override Task<CommandReply> PublishMeetup(Publish command, ServerCallContext context) =>
            HandleGrpcCommand(command);

        public override Task<CommandReply> CancelMeetup(Cancel command, ServerCallContext context) =>
            HandleGrpcCommand(command);

        async Task<CommandReply> HandleGrpcCommand(object command)
        {
            try
            {
                var commandResult = await _appService.Handle(command);
                return commandResult.Error
                    ? throw new RpcException(new Status(StatusCode.InvalidArgument, commandResult.ErrorMessage))
                    : new CommandReply {Id = commandResult.Id.ToString()};
            }
            catch (InvalidOperationException e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
            }
            catch (ArgumentException e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message));
            }
        }
    }
}