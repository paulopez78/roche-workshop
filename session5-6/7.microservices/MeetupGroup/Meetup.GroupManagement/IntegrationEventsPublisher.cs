using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Meetup.GroupManagement.Application;
using Meetup.GroupManagement.Contracts;

namespace Meetup.GroupManagement
{
    public class IntegrationEventsPublisher : IConsumer<GroupCreated>, IConsumer<MemberJoined>, IConsumer<MemberLeft>
    {
        private readonly IMediator Mediator;

        public IntegrationEventsPublisher(IMediator mediator)
        {
            Mediator = mediator;
        }

        public async Task Consume(ConsumeContext<GroupCreated> context)
        {
            var created = context.Message;
            var group   = await Mediator.Send(new Queries.GetGroupById(created.Id));

            // enrich domain event
            await context.Publish(
                new Events.V1.MeetupGroupFounded(created.Id, group.Slug, group.Title, group.FoundedAt)
            );
        }

        public async Task Consume(ConsumeContext<MemberJoined> context)
        {
            var joined = context.Message;
            var group  = await Mediator.Send(new Queries.GetGroupById(joined.GroupId));

            // enrich domain event
            await context.Publish(
                new Events.V1.MeetupGroupMemberJoined(joined.GroupId, group.Slug, joined.UserId, joined.JoinedAt)
            );
        }

        public async Task Consume(ConsumeContext<MemberLeft> context)
        {
            var left  = context.Message;
            var group = await Mediator.Send(new Queries.GetGroupById(left.GroupId));

            // enrich domain event
            await context.Publish(
                new Events.V1.MeetupGroupMemberLeft(left.GroupId, group.Slug, left.UserId, left.LeftAt)
            );
        }
    }
}