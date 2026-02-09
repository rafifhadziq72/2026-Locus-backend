using System.ComponentModel.DataAnnotations;

namespace Locus.Api.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Capacity { get; set; }

        public string? Facilities { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Soft Delete Property
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for Bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}