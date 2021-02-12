using System;
using System.Collections.Generic;
using System.Linq;
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

            return await Commit(aggregate);
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
            return await Commit(aggregate);
        }

        protected async Task<CommandResult> Handle(Task<Guid> getId, Action<TAggregate> handler) =>
            await Handle(await getId, handler);

        protected virtual Task<TAggregate?> Load(Guid id, DbContext repository)
            => repository.Set<TAggregate>().SingleOrDefaultAsync(x => x.Id == id)!;

        async Task<CommandResult> Commit(TAggregate aggregate)
        {
            if (_repository.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            await _repository.SaveChangesAsync();

            var result = new CommandResult(aggregate.Id)
            {
                Changes = aggregate.Changes.ToList()
            };

            aggregate.ClearChanges();
            return result;
        }
    }

    public record CommandResult(Guid Id, string ErrorMessage = "")
    {
        public IEnumerable<object> Changes { get; init; } = Enumerable.Empty<object>();

        public bool Error => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}