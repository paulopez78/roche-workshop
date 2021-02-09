using Microsoft.EntityFrameworkCore;

namespace MeetupEvents
{
    public class MeetupEventsDbContext : DbContext
    {
        public MeetupEventsDbContext(DbContextOptions<MeetupEventsDbContext> options) : base(options)
        {
        }
        
        public DbSet<MeetupEvent> MeetupEvents { get; set; }
    }
}