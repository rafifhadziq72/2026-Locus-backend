using System.ComponentModel.DataAnnotations;

namespace Locus.Api.Models
{
    public enum BookingStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
    }

    public class Booking
    {
        public int Id { get; set; }
        public int RoomId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BookerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string BookerEmail { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string? RejectionReason { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to link back to the Room
        public virtual Room Room { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
