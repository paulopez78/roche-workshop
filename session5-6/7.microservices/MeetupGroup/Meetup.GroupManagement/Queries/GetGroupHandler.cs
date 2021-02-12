using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Dapper;

namespace Meetup.GroupManagement.Queries
{
    public class GetGroupByIdHandler : IRequestHandler<GetGroupById, Group>, IRequestHandler<GetGroupBySlug, Group>
    {
        const string BaseQuery =
            "SELECT G.\"Id\", g.\"Title\", G.\"Slug\", G.\"Description\", G.\"Location\", G.\"OrganizerId\", G.\"FoundedAt\", M.\"Id\", M.\"UserId\",  M.\"JoinedAt\"  " +
            "FROM \"MeetupGroups\" G  LEFT JOIN \"Members\" M on M.\"GroupId\" = G.\"Id\" ";

        readonly Func<IDbConnection> GetConnection;

        public GetGroupByIdHandler(Func<IDbConnection> getConnection)
            => GetConnection = getConnection;

        public async Task<Group> Handle(GetGroupById request, CancellationToken cancellationToken)
        {
            using var connection = GetConnection();

            Group result = null;

            await connection.QueryAsync<Group, Member, Group>($"{BaseQuery} WHERE G.\"Id\"=@id",
                (group, member) =>
                {
                    result ??= group;
                    if (member is not null) result.Members.Add(member);
                    return result;
                },
                new {Id = request.Id});

            return result;
        }

        public async Task<Group> Handle(GetGroupBySlug request, CancellationToken cancellationToken)
        {
            using var connection = GetConnection();

            Group result = null;

            await connection.QueryAsync<Group, Member, Group>($"{BaseQuery} WHERE G.\"Slug\"=@slug",
                (group, member) =>
                {
                    result ??= group;
                    if (member is not null) result.Members.Add(member);
                    return result;
                },
                new {Slug = request.Slug});

            return result;
        }
    }

    public record GetGroupById(Guid Id) : IRequest<Group>;

    public record GetGroupBySlug(string Slug) : IRequest<Group>;

    public record Group(Guid Id, string Title, string Slug, string Description, string Location, Guid OrganizerId, DateTime FoundedAt)
    {
        public List<Member> Members { get; set; } = new();
    }

    public record Member (int Id, Guid UserId, DateTime JoinedAt);

    public class GetGroupRequestValidator : AbstractValidator<GetGroupById>
    {
        public GetGroupRequestValidator() => RuleFor(x => x.Id).NotEmpty();
    }

    public class GetGroupBySlugRequestValidator : AbstractValidator<GetGroupBySlug>
    {
        public GetGroupBySlugRequestValidator() => RuleFor(x => x.Slug).Matches(@"^[a-z\d](?:[a-z\d_-]*[a-z\d])?$");
    }
}