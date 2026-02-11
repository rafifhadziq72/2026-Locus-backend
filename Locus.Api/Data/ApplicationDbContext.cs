using Locus.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Required for Identity
using Microsoft.EntityFrameworkCore;

namespace Locus.Api.Data
{
    // Inherit from IdentityDbContext to enable User and Role management
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Crucial: Call the base method to configure Identity tables
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<Room>().Property(r => r.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<Booking>().Property(b => b.IsDeleted).HasDefaultValue(false);

            modelBuilder.Entity<Room>().HasIndex(r => r.RoomNumber).IsUnique();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Booking || entityEntry.Entity is Room)
                {
                    entityEntry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;

                    if (entityEntry.State == EntityState.Added)
                    {
                        entityEntry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}