#nullable disable
using System;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeetupEvents.Infrastructure
{
    public class MeetupEventsDbContext : DbContext
    {
        public MeetupEventsDbContext(DbContextOptions<MeetupEventsDbContext> options) : base(options)
        {
        }

        public DbSet<MeetupEventAggregate> MeetupEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventAggregate>(b =>
            {
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
            });

            modelBuilder.Entity<MeetupEventAggregate>().ToTable("MeetupEvent");

            modelBuilder.Entity<Attendant>(b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .ValueGeneratedOnAdd();

                b.HasKey("Id");
            });
        }
    }
}