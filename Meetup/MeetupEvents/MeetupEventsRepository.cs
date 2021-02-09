using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents
{
    public class MeetupEventsRepository
    {
        private MeetupEventsDbContext _dbContext;

        public MeetupEventsRepository(MeetupEventsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<MeetupEvent?> Get(Guid id)
            => _dbContext.MeetupEvents.SingleOrDefaultAsync(x => x.Id == id);

        public Task<List<MeetupEvent>> GetAll()
            => _dbContext.MeetupEvents.ToListAsync();

        public async Task<bool> Add(MeetupEvent meetupEvent)
        {
            var meetup = await Get(meetupEvent.Id);
            // already exists
            if (meetup is not null)
                return false;
                
            await _dbContext.MeetupEvents.AddAsync(meetupEvent);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public Task Commit() => _dbContext.SaveChangesAsync();

        public async Task<bool> Remove(Guid id)
        {
            var meetupEvent = await Get(id);

            if (meetupEvent == null)
                return false;

            _dbContext.MeetupEvents.Remove(meetupEvent);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}