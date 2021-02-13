using Meetup.GroupManagement.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable
namespace Meetup.GroupManagement.Data
{
    public class MeetupGroupManagementDbContext : DbContext
    {
        public MeetupGroupManagementDbContext(DbContextOptions<MeetupGroupManagementDbContext> options) : base(options)
        {
        }

        public DbSet<MeetupGroup> MeetupGroups { get; set; }
        public DbSet<GroupMember> Members      { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeetupGroup>(b =>
            {
                //https://docs.microsoft.com/en-us/ef/core/modeling/concurrency?tabs=data-annotations
                //https://www.npgsql.org/efcore/modeling/concurrency.html
                b.HasIndex(p => p.Slug).IsUnique();
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<GroupStatus>());
                b.UseXminAsConcurrencyToken();
            });

            modelBuilder.Entity<GroupMember>(b =>
            {
                b.UseXminAsConcurrencyToken();
                b.Property(p => p.Role).HasConversion(new EnumToStringConverter<Role>());
                b.Property(p => p.Status).HasConversion(new EnumToStringConverter<MemberStatus>());
                b.HasIndex(p => p.GroupId);
                b.HasIndex(p => new {p.GroupId, p.UserId}).IsUnique();
            });

            modelBuilder.Entity<Outbox>(b =>
            {
                b.Property<int>("Id")
                    .HasColumnType("int")
                    .ValueGeneratedOnAdd();
                b.HasKey("Id");
            });
        }
    }

    public class MeetupGroupManagementDbContextFactory : IDesignTimeDbContextFactory<MeetupGroupManagementDbContext>
    {
        public MeetupGroupManagementDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MeetupGroupManagementDbContext>();
            optionsBuilder.UseNpgsql(
                @"Host=localhost;Database=meetup;Username=meetup;Password=password;SearchPath=group_management");
            return new MeetupGroupManagementDbContext(optionsBuilder.Options);
        }
    }
}