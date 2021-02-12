using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Framework
{
    public interface IApplicationService
    {
        Task<CommandResult> Handle(object command);
    }

    public abstract class ApplicationService<TAggregate> : IApplicationService where TAggregate : Aggregate, new()
    {
        readonly DbContext _repository;

        protected ApplicationService(DbContext dbContext) =>
            _repository = dbContext;

        public abstract Task<CommandResult> Handle(object command);

        protected async Task<CommandResult> HandleCreate(Guid id, Action<TAggregate> handler)
        {
            var loadedAggregate = await Load(id, _repository);
            if (loadedAggregate is not null)
                return new(id, $"Aggregate Already exists");

            var aggregate = new TAggregate();

            handler(aggregate);

            await _repository.AddAsync(aggregate);
            await Commit(aggregate);

            return new(id);
        }

        protected async Task<CommandResult> Handle(Guid id, Action<TAggregate> handler)
        {
            // load aggregate
            var aggregate = await Load(id, _repository);
            if (aggregate is null)
                return new(id, "Aggregate not found");

            // handle
            handler(aggregate);

            // commit
            await Commit(aggregate);
            return new(id);
        }

        protected virtual Task<TAggregate?> Load(Guid id, DbContext repository)
            => repository.Set<TAggregate>().SingleOrDefaultAsync(x => x.Id == id)!;

        async Task Commit(TAggregate aggregate)
        {
            if (_repository.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            await _repository.SaveChangesAsync();
        }
    }

    public record CommandResult(Guid Id, string ErrorMessage = "")
    {
        public bool Error => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}