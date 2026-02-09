using Locus.Api.Models;
using Locus.Api.Data;
using System.Linq;

namespace Locus.Api.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static void SeedRooms(ApplicationDbContext context)
        {
            if (context.Rooms.Any()) return;

            var rooms = new List<Room>
            {
                new Room { RoomNumber = "R101", Name = "Meeting Room A", Capacity = 10, Facilities = "Projector, AC", IsAvailable = true },
                new Room { RoomNumber = "R102", Name = "Meeting Room B", Capacity = 20, Facilities = "Video Conference, AC", IsAvailable = true },
                new Room { RoomNumber = "LAB1", Name = "Computer Lab 1", Capacity = 30, Facilities = "30 PCs, Projector", IsAvailable = true }
            };

            context.Rooms.AddRange(rooms);
            context.SaveChanges();
        }
    }
}