using Microsoft.EntityFrameworkCore;
using Work360.Domain.Entity;

namespace Work360.Infrastructure.Context
{
    public class Work360Context : DbContext
    {
        public Work360Context(DbContextOptions<Work360Context> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<Tasks> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Work360_User");
            modelBuilder.Entity<Events>().ToTable("Work360_Events");
            modelBuilder.Entity<Tasks>().ToTable("Work360_Tasks");
            modelBuilder.Entity<Meeting>().ToTable("Work360_Meetings");

            // Convertendo enums para string NVARCHAR no banco
            modelBuilder.Entity<Events>()
                .Property(e => e.EventType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Tasks>()
                .Property(t => t.Priority)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Tasks>()
                .Property(t => t.TaskSituation)
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}
