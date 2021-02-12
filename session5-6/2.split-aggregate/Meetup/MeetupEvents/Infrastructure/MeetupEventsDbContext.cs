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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupEventAggregate>(b =>
            {
                b.ToTable("MeetupEvent");
                b.OwnsOne(p => p.Details, d =>
                {
                    d.Property(p => p.Title).HasColumnName("Title");
                    d.Property(p => p.Description).HasColumnName("Description");
                });
                b.OwnsOne(p => p.ScheduleDateTime, d =>
                {
                    d.Property(p => p.Start).HasColumnName("Start");
                    d.Property(p => p.End).HasColumnName("End");
                });
                b.OwnsOne(p => p.Location,
                    l =>
                    {
                        l.Property(p => p.Url).HasColumnName("Url");
                        l.Property(p => p.IsOnline).HasColumnName("IsOnline");
                        l.OwnsOne(p => p.Address, a => a.Property(p => p.Value).HasColumnName("Address"));
                    }
                );
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MeetupEventStatus>());
                b.Property(p => p.Version).IsConcurrencyToken();
            });

            modelBuilder.Entity<AttendantListAggregate>(b =>
            {
                b.ToTable("AttendantList");
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<AttendantListStatus>());
                b.OwnsOne(p => p.Capacity, d => d.Property(p => p.Value).HasColumnName("Capacity"));
                b.Property(p => p.Version).IsConcurrencyToken();
            });

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