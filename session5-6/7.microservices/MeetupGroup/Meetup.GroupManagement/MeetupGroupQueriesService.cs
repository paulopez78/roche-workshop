using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Meetup.GroupManagement.Contracts.Queries.V1;

namespace Meetup.GroupManagement
{
    public class MeetupGroupQueriesService : MeetupGroupQueries.MeetupGroupQueriesBase
    {
        readonly IMediator Mediator;

        public MeetupGroupQueriesService(IMediator mediator)
        {
            Mediator = mediator;
        }
        
        public override async Task<GetGroup.Types.GetGroupReply> Get(GetGroup query, ServerCallContext context)
        {
            var result = query.IdCase switch
            {
                GetGroup.IdOneofCase.GroupId
                    => await Mediator.Send(new Queries.GetGroupById(ParseGuid(query.GroupId, nameof(query.GroupId)))),
                GetGroup.IdOneofCase.GroupSlug
                    => await Mediator.Send(new Queries.GetGroupBySlug(query.GroupSlug)),
                _
                    => throw new ArgumentException(nameof(query.IdCase)),
            };

            if (result is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Group {query.GroupId}-{query.GroupSlug} not found"));

            return new()
            {
                Group = new GetGroup.Types.Group()
                {
                    Id          = result.Id.ToString(),
                    Slug        = result.Slug,
                    OrganizerId = result.OrganizerId.ToString(),
                    Title       = result.Title,
                    Description = result.Description,
                    Members =
                    {
                        result.Members.Select(x => new GetGroup.Types.Member()
                        {
                            UserId   = x.UserId.ToString(),
                            JoinedAt = new DateTimeOffset(x.JoinedAt).ToTimestamp()
                        })
                    }
                }
            };
        }

        static Guid ParseGuid(string id, string parameterName)
        {
            if (!Guid.TryParse(id, out var parsed))
                throw new ArgumentException($"Invalid {parameterName}:{id}");

            return parsed;
        }
    }
}