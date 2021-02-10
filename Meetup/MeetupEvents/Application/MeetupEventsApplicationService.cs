using System;
using System.Threading.Tasks;
using MeetupEvents.Contracts;
using MeetupEvents.Contracts.Commands.V1;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Application
{
    public interface IApplicationService
    {
        Task<CommandResult> Handle(object command);
    }

    public class MeetupEventsApplicationService : IApplicationService
    {
        readonly MeetupEventsDbContext _repository;

        public MeetupEventsApplicationService(MeetupEventsDbContext db)
        {
            _repository = db;
        }

        public Task<CommandResult> Handle(object command) =>
            command switch
            {
                Create create =>
                    HandleCreateCommand(create.Id, meetup =>
                    {
                        // dbcontext, erp, email, 
                        meetup.Create(Parse(create.Id), create.Title, create.Capacity);
                    }),

                Publish publish =>
                    HandleCommand(publish.Id, meetup=> meetup.Publish()),

                Cancel cancel =>
                    HandleCommand(cancel.Id, meetup => meetup.Cancel()),

                _ => throw new InvalidOperationException("Command handler does not exist")
            };

        async Task<CommandResult> HandleCreateCommand(string id, Action<MeetupEventEntity> handler)
        {
            var guid = Parse(id);
            // idempotency check
            var loadedMeetup = await Load(guid);
            if (loadedMeetup is not null)
                return new(guid, $"Meetup already exists");

            var meetup = new MeetupEventEntity();

            // handle
            handler(meetup);

            await _repository.AddAsync(meetup);
            await _repository.SaveChangesAsync();

            return new(guid);
        }

        async Task<CommandResult> HandleCommand(string id, Action<MeetupEventEntity> handler)
        {
            var guid = Parse(id);
            // load entity
            var meetup = await Load(guid);
            if (meetup is null)
                return new(guid, "Not found");

            // handle
            handler(meetup);

            // commit
            await _repository.SaveChangesAsync();
            return new(guid);
        }

        Task<MeetupEventEntity?> Load(Guid id)
            => _repository.MeetupEvents.SingleOrDefaultAsync(x => x.Id == id)!;

        Guid Parse(string id)
        {
            if (!Guid.TryParse(id, out var uuid))
                throw new ArgumentException(nameof(id));

            return uuid;
        }
    }


    public record CommandResult(Guid Id, string ErrorMessage = "")
    {
        public bool Error => !string.IsNullOrWhiteSpace(ErrorMessage);
    }


}