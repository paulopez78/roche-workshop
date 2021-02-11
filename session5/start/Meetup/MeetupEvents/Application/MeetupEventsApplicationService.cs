using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.Commands.V1;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : IApplicationService
    {
        readonly MeetupEventsDbContext _repository;
        readonly Func<DateTimeOffset>  _getUtcNow;

        public MeetupEventsApplicationService(MeetupEventsDbContext db, Func<DateTimeOffset> getUtcNow)
        {
            _repository = db;
            _getUtcNow  = getUtcNow;
        }

        public Task<CommandResult> Handle(object command) =>
            command switch
            {
                Create create =>
                    HandleCreateCommand(
                        create.Id,
                        meetup => meetup.Create(create.Id, create.Title, create.Description, create.Capacity)
                    ),

                UpdateDetails details =>
                    HandleCommand(
                        details.Id,
                        meetup => meetup.UpdateDetails(details.Title, details.Description)
                    ),

                Schedule schedule =>
                    HandleCommand(
                        schedule.Id,
                        meetup => meetup.Schedule(schedule.Start, schedule.End, _getUtcNow())
                    ),

                MakeOnline online =>
                    HandleCommand(
                        online.Id,
                        meetup => meetup.MakeOnline(online.Url)
                    ),

                MakeOnsite onsite =>
                    HandleCommand(
                        onsite.Id,
                        meetup => meetup.MakeOnsite(onsite.Address)
                    ),

                IncreaseCapacity increase =>
                    HandleCommand(
                        increase.Id,
                        meetup => meetup.IncreaseCapacity(increase.ByNumber)
                    ),

                ReduceCapacity reduce =>
                    HandleCommand(
                        reduce.Id,
                        meetup => meetup.ReduceCapacity(reduce.ByNumber)
                    ),

                Publish publish =>
                    HandleCommand(
                        publish.Id,
                        meetup => meetup.Publish()
                    ),

                Attend attend =>
                    HandleCommand(
                        attend.Id,
                        meetup => meetup.Attend(attend.MemberId, _getUtcNow())
                    ),

                CancelAttendance cancelAttendance =>
                    HandleCommand(
                        cancelAttendance.Id,
                        meetup => meetup.CancelAttendance(cancelAttendance.MemberId)
                    ),

                Cancel cancel =>
                    HandleCommand(
                        cancel.Id,
                        meetup => meetup.Cancel(cancel.Reason)
                    ),

                Start start =>
                    HandleCommand(
                        start.Id,
                        meetup => meetup.Start()
                    ),

                Finish finish =>
                    HandleCommand(
                        finish.Id,
                        meetup => meetup.Finish()
                    ),

                _ => throw new InvalidOperationException("Command handler does not exist")
            };

        async Task<CommandResult> HandleCreateCommand(Guid id, Action<MeetupEventAggregate> handler)
        {
            // idempotency check
            var loadedAggregate = await Load(id);
            if (loadedAggregate is not null)
                return new(id, $"Aggregate Already exists");

            var aggregate = new MeetupEventAggregate();

            // handle
            handler(aggregate);

            await _repository.AddAsync(aggregate);
            await _repository.SaveChangesAsync();

            return new(id);
        }

        async Task<CommandResult> HandleCommand(Guid id, Action<MeetupEventAggregate> handler)
        {
            // load aggregate
            var aggregate = await Load(id);
            if (aggregate is null)
                return new(id, "Aggregate Not found");

            // handle
            handler(aggregate);

            // commit
            await _repository.SaveChangesAsync();
            return new(id);
        }

        Task<MeetupEventAggregate?> Load(Guid id)
            => _repository.MeetupEvents.SingleOrDefaultAsync(x => x.Id == id)!;
    }

    public record CommandResult(Guid Id, string ErrorMessage = "")
    {
        public bool Error => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}