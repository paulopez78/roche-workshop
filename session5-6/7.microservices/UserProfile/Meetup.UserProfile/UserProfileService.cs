using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Meetup.UserProfile.Contracts;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static MongoDB.Driver.Builders<Meetup.UserProfile.Data.UserProfile>;

namespace Meetup.UserProfile
{
    public class UserProfileService : Contracts.UserProfile.UserProfileBase
    {
        public UserProfileService(IMongoDatabase database) =>
            DbCollection = database.GetCollection<Data.UserProfile>(nameof(UserProfile));

        readonly IMongoCollection<Data.UserProfile> DbCollection;

        public override async Task<CreateOrUpdateRequest.Types.CreateOrUpdateReply> CreateOrUpdate(
            CreateOrUpdateRequest request, ServerCallContext context)
        {
            await DbCollection.UpdateOneAsync(
                x => x.Id == request.UserId,
                Update
                    .SetOnInsert(x => x.Id, request.UserId)
                    .Set(x => x.FirstName, request.FirstName)
                    .Set(x => x.FirstName, request.FirstName)
                    .Set(x => x.LastName, request.LastName)
                    .Set(x => x.Email, request.Email)
                    .AddToSetEach(x => x.Interests, request.Interests)
                ,
                new UpdateOptions {IsUpsert = true},
                context.CancellationToken);

            return new() {UserId = request.UserId};
        }

        public override async Task<DeleteRequest.Types.DeleteReply> Delete(DeleteRequest request,
            ServerCallContext context)
        {
            await DbCollection.DeleteOneAsync(x => x.Id == request.UserId);
            return new() {UserId = request.UserId};
        }

        public override async Task<GetRequest.Types.GetReply> Get(GetRequest request, ServerCallContext context)
        {
            var userProfile = await DbCollection.AsQueryable().Where(x => x.Id == request.UserId).SingleAsync();
            return new() {User = Map(userProfile)};
        }

        public override async Task<GetUsersRequest.Types.GetUsersReply> GetUsers(GetUsersRequest request,
            ServerCallContext context)
        {
            var users = await DbCollection.Find(Filter.In(x => x.Id, request.Users)).ToListAsync();
            return new()
            {
                Users = {users.Select(Map)}
            };
        }

        User Map(Data.UserProfile userProfile)
            => new()
            {
                UserId    = userProfile.Id,
                FirstName = userProfile.FirstName,
                LastName  = userProfile.LastName,
                Email     = userProfile.Email,
                Interests = {userProfile.Interests}
            };
    }
}