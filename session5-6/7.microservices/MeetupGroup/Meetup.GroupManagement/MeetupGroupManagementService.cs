using System;
using System.Threading.Tasks;
using Grpc.Core;
using MediatR;
using Meetup.GroupManagement.Contracts.Commands.V1;

namespace Meetup.GroupManagement
{
    public class MeetupGroupManagementService : MeetupGroupManagement.MeetupGroupManagementBase
    {
        readonly IMediator Mediator;

        public MeetupGroupManagementService(IMediator mediator)
        {
            Mediator = mediator;
        }

        public override async Task<CommandReply> Create(CreateRequest command, ServerCallContext context)
        {
            var groupId     = ParseGuid(command.Id, "GroupId");
            var organizerId = ParseGuid(command.OrganizerId, nameof(command.OrganizerId));

            var result = await Mediator.Send(
                new Application.CreateRequest(groupId, organizerId, command.Slug,
                    command.Title, command.Description, command.Location)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> UpdateDetails(UpdateDetailsRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.Id, "GroupId");

            var result = await Mediator.Send(
                new Application.UpdateGroupDetailsRequest(groupId, command.Title, command.Description, command.Location)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> Join(JoinRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.GroupId, nameof(command.GroupId));
            var userId  = ParseGuid(command.UserId, nameof(command.UserId));

            var result = await Mediator.Send(
                new Application.JoinRequest(groupId, userId)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> Leave(LeaveRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.GroupId, nameof(command.GroupId));
            var userId  = ParseGuid(command.UserId, nameof(command.UserId));

            var result = await Mediator.Send(
                new Application.LeaveRequest(groupId, userId, command.Reason)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        static Guid ParseGuid(string id, string parameterName)
        {
            if (!Guid.TryParse(id, out var parsed))
                throw new ArgumentException($"Invalid {parameterName}:{id}");

            return parsed;
        }
    }
}