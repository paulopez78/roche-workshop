using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Application
{
    public class CreateGroupHandler : IRequestHandler<CreateRequest, CommandResult>
    {
        readonly MeetupGroupManagementDbContext DbContext;
        readonly IMediator                      Mediator;

        public CreateGroupHandler(IMediator mediator, MeetupGroupManagementDbContext dbContext)
        {
            DbContext = dbContext;
            Mediator  = mediator;
        }

        public async Task<CommandResult> Handle(CreateRequest request, CancellationToken cancellationToken)
        {
            // check if group slug already taken
            var sameGroupSlug = await DbContext.MeetupGroups.SingleOrDefaultAsync(
                x => x.Slug == request.GroupSlug, cancellationToken: cancellationToken
            );
            if (sameGroupSlug is not null)
                throw new ApplicationException($"Group slug {request.GroupSlug} already taken");

            var group = new MeetupGroup
            {
                Id = request.Id, OrganizerId = request.OrganizerId, Slug = request.GroupSlug, Title = request.Title,
                Description = request.Description, Location = request.Location, FoundedAt = DateTimeOffset.UtcNow
            };

            await DbContext.MeetupGroups.AddAsync(group, cancellationToken);

            // join organizer as a group member
            await Mediator.Send(
                new JoinRequest(group.Id, group.OrganizerId, Role.Organizer), cancellationToken
            );

            // notify event
            await Mediator.Publish(
                new GroupCreated(group.Id, group.OrganizerId), cancellationToken
            );

            return new CommandResult(group.Id, group.Slug);
        }
    }

    public record GroupCreated(Guid Id, Guid OrganizerId) : INotification;

    public record CreateRequest(Guid Id, Guid OrganizerId, string GroupSlug, string Title, string Description,
        string Location) : IRequest<CommandResult>;

    public class CreateRequestValidator : AbstractValidator<CreateRequest>
    {
        public CreateRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.OrganizerId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.GroupSlug).Matches(@"^[a-z\d](?:[a-z\d_-]*[a-z\d])?$");
            RuleFor(x => x.Location).NotEmpty().MaximumLength(255);
        }
    }
}