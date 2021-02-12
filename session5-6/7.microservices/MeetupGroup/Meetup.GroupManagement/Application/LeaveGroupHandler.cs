using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Application
{
    public class LeaveGroupHandler : IRequestHandler<LeaveRequest, CommandResult>
    {
        readonly MeetupGroupManagementDbContext DbContext;
        readonly IMediator                      Mediator;

        public LeaveGroupHandler(MeetupGroupManagementDbContext dbContext, IMediator mediator)
        {
            DbContext = dbContext;
            Mediator  = mediator;
        }

        public LeaveGroupHandler(MeetupGroupManagementDbContext dbContext) => DbContext = dbContext;

        public async Task<CommandResult> Handle(LeaveRequest request, CancellationToken cancellationToken)
        {
            // member does not exists
            var member =
                await DbContext.Members.SingleOrDefaultAsync(x =>
                    x.UserId == request.UserId && x.GroupId == request.GroupId, cancellationToken: cancellationToken);

            if (member is null)
                throw new ApplicationException($"User {request.UserId} is not member of group {request.GroupId}");

            DbContext.Members.Remove(member);

            // notify event
            await Mediator.Publish(new MemberLeft(member.GroupId, member.UserId, DateTimeOffset.Now), cancellationToken);

            return new CommandResult(member.GroupId, "");
        }
    }

    public record LeaveRequest(Guid GroupId, Guid UserId, string Reason) : IRequest<CommandResult>;

    public record MemberLeft(Guid GroupId, Guid UserId, DateTimeOffset LeftAt) : INotification;
    
    public class LeaveRequestValidator : AbstractValidator<LeaveRequest>
    {
        public LeaveRequestValidator()
        {
            RuleFor(x => x.GroupId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}