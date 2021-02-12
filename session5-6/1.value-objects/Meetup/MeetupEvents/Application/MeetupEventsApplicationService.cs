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
                    HandleCreate(
                        create.Id,
                        meetup => meetup.Create(
                            create.Id,
                            Details.From(create.Title, create.Description),
                            create.Capacity
                        )
                    ),

                UpdateDetails details =>
                    Handle(
                        details.Id,
                        meetup => meetup.UpdateDetails(Details.From(details.Title, details.Description))
                    ),

                Schedule schedule =>
                    Handle(
                        schedule.Id,
                        meetup => meetup.Schedule(ScheduleDateTime.From(_getUtcNow, schedule.Start, schedule.End))
                    ),

                MakeOnline online =>
                    Handle(
                        online.Id,
                        meetup => meetup.MakeOnline(online.Url)
                    ),

                MakeOnsite onsite =>
                    Handle(
                        onsite.Id,
                        meetup => meetup.MakeOnsite(onsite.Address)
                    ),

                IncreaseCapacity increase =>
                    Handle(
                        increase.Id,
                        meetup => meetup.IncreaseCapacity(increase.ByNumber)
                    ),

                ReduceCapacity reduce =>
                    Handle(
                        reduce.Id,
                        meetup => meetup.ReduceCapacity(reduce.ByNumber)
                    ),

                Publish publish =>
                    Handle(
                        publish.Id,
                        meetup => meetup.Publish()
                    ),

                Attend attend =>
                    Handle(
                        attend.Id,
                        meetup => meetup.Attend(attend.MemberId, _getUtcNow())
                    ),

                CancelAttendance cancelAttendance =>
                    Handle(
                        cancelAttendance.Id,
                        meetup => meetup.CancelAttendance(cancelAttendance.MemberId)
                    ),

                Cancel cancel =>
                    Handle(
                        cancel.Id,
                        meetup => meetup.Cancel(cancel.Reason)
                    ),

                Start start =>
                    Handle(
                        start.Id,
                        meetup => meetup.Start()
                    ),

                Finish finish =>
                    Handle(
                        finish.Id,
                        meetup => meetup.Finish()
                    ),

                _ => throw new InvalidOperationException("Command handler does not exist")
            };

        async Task<CommandResult> HandleCreate(Guid id, Action<MeetupEventAggregate> handler)
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

        async Task<CommandResult> Handle(Guid id, Action<MeetupEventAggregate> handler)
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
            => _repository.MeetupEvents.Include(x => x.Attendants).SingleOrDefaultAsync(x => x.Id == id)!;
    }

    public record CommandResult(Guid Id, string ErrorMessage = "")
    {
        public bool Error => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}