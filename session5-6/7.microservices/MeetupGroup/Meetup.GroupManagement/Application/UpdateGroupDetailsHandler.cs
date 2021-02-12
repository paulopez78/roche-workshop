using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Application
{
    public class UpdateGroupDetailsHandler : IRequestHandler<UpdateGroupDetailsRequest, CommandResult>
    {
        readonly MeetupGroupManagementDbContext DbContext;

        public UpdateGroupDetailsHandler(MeetupGroupManagementDbContext dbContext) => DbContext = dbContext;

        public async Task<CommandResult> Handle(UpdateGroupDetailsRequest request, CancellationToken cancellationToken)
        {
            // load meetup group
            var meetupGroup =
                await DbContext.MeetupGroups.SingleOrDefaultAsync(
                    x => x.Id == request.Id, cancellationToken: cancellationToken
                );
            if (meetupGroup is null)
                throw new ApplicationException($"meetup group {request.Id} not found");

            meetupGroup.Title       = request.Title;
            meetupGroup.Description = request.Description;
            meetupGroup.Location    = request.Location;

            return new CommandResult(meetupGroup.Id, meetupGroup.Slug);
        }
    }

    public record UpdateGroupDetailsRequest
        (Guid Id, string Title, string Description, string Location) : IRequest<CommandResult>;

    public class UpdateRequestValidator : AbstractValidator<UpdateGroupDetailsRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(255);
        }
    }
}