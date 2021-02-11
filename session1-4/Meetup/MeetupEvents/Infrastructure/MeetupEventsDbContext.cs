#nullable disable
using MeetupEvents.Application;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class MeetupEventsDbContext : DbContext
    {
        public MeetupEventsDbContext(DbContextOptions<MeetupEventsDbContext> options) : base(options)
        {
        }
        public DbSet<MeetupEventEntity> MeetupEvents { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<MeetupEventAggregate>(b =>
            // {
            //     b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatusStatus>());
            //     b.Property(p => p.Version).IsConcurrencyToken();
            // });
            //
            // modelBuilder.Entity<MeetupEventAggregate>().ToTable("MeetupEvent");
            //
            // modelBuilder.Entity<Attendant>(b =>
            // {
            //     b.Property<Guid>("Id")
            //         .HasColumnType("uuid")
            //         .ValueGeneratedOnAdd();
            //
            //     b.HasKey("Id");
            // });
        }
    }
}