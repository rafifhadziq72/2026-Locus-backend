using Locus.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Locus.Api.DTOs
{
    public class CreateBookingRequest
    {
        [Required]
        public int RoomId { get; set; }
        [Required]
        public string BookerName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string BookerEmail { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }

    // NEW: DTO for updating status
    public class UpdateBookingStatusRequest
    {
        [Required]
        public BookingStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class BookingResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string BookerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }
}