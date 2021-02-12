using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    public class AttendantListApplicationService : ApplicationService<AttendantListAggregate>
    {
        readonly Func<DateTimeOffset> _getUtcNow;

        public AttendantListApplicationService(MeetupEventsDbContext db, Func<DateTimeOffset> getUtcNow) : base(db) =>
            _getUtcNow = getUtcNow;

        public override Task<CommandResult> Handle(object command) =>
            command switch
            {
                Create create =>
                    HandleCreate(
                        create.Id,
                        attendantList => attendantList.Create(
                            create.Id,
                            create.MeetupId,
                            create.Capacity
                        )
                    ),

                IncreaseCapacity increase =>
                    Handle(
                        increase.Id,
                        attendantList => attendantList.IncreaseCapacity(increase.ByNumber)
                    ),

                ReduceCapacity reduce =>
                    Handle(
                        reduce.Id,
                        attendantList => attendantList.ReduceCapacity(reduce.ByNumber)
                    ),

                Open open =>
                    Handle(
                        open.Id,
                        attendantList => attendantList.Open(_getUtcNow())
                    ),
                Close close =>
                    Handle(
                        close.Id,
                        attendantList => attendantList.Open(_getUtcNow())
                    ),

                Archive archive =>
                    Handle(
                        archive.Id,
                        attendantList => attendantList.Open(_getUtcNow())
                    ),

                Attend attend =>
                    Handle(
                        attend.Id,
                        attendantList => attendantList.Attend(attend.MemberId, _getUtcNow())
                    ),

                CancelAttendance cancelAttendance =>
                    Handle(
                        cancelAttendance.Id,
                        attendantList => attendantList.CancelAttendance(cancelAttendance.MemberId)
                    ),
                
                _ => throw new InvalidOperationException("Command handler does not exist")
            };

        protected override Task<AttendantListAggregate?> Load(Guid id, DbContext repository) =>
            repository.Set<AttendantListAggregate>()
                .Include(x => x.Attendants)
                .SingleOrDefaultAsync(x => x.Id == id)!;
    }
}