using Locus.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Locus.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filters (keeps deleted items hidden from GET)
            modelBuilder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);

            // Set Default Values for new records
            modelBuilder.Entity<Room>().Property(r => r.IsDeleted).HasDefaultValue(false);
            modelBuilder.Entity<Booking>().Property(b => b.IsDeleted).HasDefaultValue(false);

            modelBuilder.Entity<Room>().HasIndex(r => r.RoomNumber).IsUnique();
        }

        // Industry Standard: Auto-update Timestamps
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                // Use 'dynamic' or check if property exists to prevent the crash you saw
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
