using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Application
{
    public class JoinGroupHandler : IRequestHandler<JoinRequest, CommandResult>
    {
        readonly MeetupGroupManagementDbContext DbContext;
        readonly IMediator                      Mediator;

        public JoinGroupHandler(MeetupGroupManagementDbContext dbContext, IMediator mediator)
        {
            DbContext = dbContext;
            Mediator  = mediator;
        }

        public async Task<CommandResult> Handle(JoinRequest request, CancellationToken cancellationToken)
        {
            // already joined
            var loadedMember =
                await DbContext.Members.SingleOrDefaultAsync(x =>
                    x.UserId == request.UserId && x.GroupId == request.GroupId, cancellationToken: cancellationToken);

            if (loadedMember is not null)
                throw new ApplicationException($"User {request.UserId} is already member of group {request.GroupId}");

            var member = new GroupMember
            {
                GroupId  = request.GroupId,
                UserId   = request.UserId,
                Role     = request.Role,
                Status   = MemberStatus.Active,
                JoinedAt = DateTimeOffset.UtcNow
            };

            await DbContext.Members.AddAsync(member, cancellationToken);

            if (request.Role != Role.Organizer)
            {
                // notify event
                await Mediator.Publish(
                    new MemberJoined(member.GroupId, member.UserId, member.JoinedAt), cancellationToken
                );
            }

            return new CommandResult(member.GroupId, "");
        }
    }

    public record JoinRequest(Guid GroupId, Guid UserId, Role Role = Role.Member) : IRequest<CommandResult>;

    public record MemberJoined(Guid GroupId, Guid UserId, DateTimeOffset JoinedAt) : INotification;

    public class JoinRequestValidator : AbstractValidator<JoinRequest>
    {
        public JoinRequestValidator()
        {
            RuleFor(x => x.GroupId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}