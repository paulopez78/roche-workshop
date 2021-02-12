using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Framework;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.MeetupCommands.V1;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : ApplicationService<MeetupEventAggregate>
    {
        readonly Func<DateTimeOffset> _getUtcNow;

        public MeetupEventsApplicationService(MeetupEventsDbContext db, Func<DateTimeOffset> getUtcNow) : base(db) =>
            _getUtcNow = getUtcNow;

        public override Task<CommandResult> Handle(object command) =>
            command switch
            {
                Create create =>
                    HandleCreate(
                        create.Id,
                        meetup => meetup.Create(create.Id, create.GroupId,
                            Details.From(create.Title, create.Description))
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

                Publish publish =>
                    Handle(
                        publish.Id,
                        meetup => meetup.Publish()
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
    }
}