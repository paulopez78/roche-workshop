using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application.AttendantList
{
    public class AttendantListApplicationService : ApplicationService<AttendantListAggregate>
    {
        readonly Func<DateTimeOffset> _getUtcNow;
        readonly GetMappedId          _getMappedId;

        public AttendantListApplicationService(MeetupEventsDbContext db, Func<DateTimeOffset> getUtcNow,
            GetMappedId getMappedId
        ) : base(db)
        {
            _getUtcNow   = getUtcNow;
            _getMappedId = getMappedId;
        }

        public override Task<CommandResult> Handle(object command) =>
            command switch
            {
                CreateAttendantList create =>
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
                        MapId(increase.MeetupId),
                        attendantList => attendantList.IncreaseCapacity(increase.ByNumber)
                    ),

                ReduceCapacity reduce =>
                    Handle(
                        MapId(reduce.MeetupId),
                        attendantList => attendantList.ReduceCapacity(reduce.ByNumber)
                    ),

                Open open =>
                    Handle(
                        MapId(open.MeetupId),
                        attendantList => attendantList.Open(_getUtcNow())
                    ),
                Close close =>
                    Handle(
                        MapId(close.MeetupId),
                        attendantList => attendantList.Close(_getUtcNow())
                    ),

                Archive archive =>
                    Handle(
                        MapId(archive.MeetupId),
                        attendantList => attendantList.Archive(_getUtcNow())
                    ),

                Attend attend =>
                    Handle(
                        MapId(attend.MeetupId),
                        attendantList => attendantList.Attend(attend.MemberId, _getUtcNow())
                    ),

                CancelAttendance cancelAttendance =>
                    Handle(
                        MapId(cancelAttendance.MeetupId),
                        attendantList => attendantList.CancelAttendance(cancelAttendance.MemberId)
                    ),

                _ => throw new InvalidOperationException("Command handler does not exist")
            };

        async Task<Guid> MapId(Guid id)
        {
            var mapId = await _getMappedId(id);
            if (!mapId.HasValue)
                throw new ArgumentException($"Can not found mapped id {id}");

            return mapId.Value;
        }

        protected override Task<AttendantListAggregate?> Load(Guid id, DbContext repository) =>
            repository.Set<AttendantListAggregate>()
                .Include(x => x.Attendants)
                .SingleOrDefaultAsync(x => x.Id == id)!;
    }
}