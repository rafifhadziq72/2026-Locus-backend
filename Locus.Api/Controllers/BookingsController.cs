using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Locus.Api.Data;
using Locus.Api.Models;
using Locus.Api.DTOs;

namespace Locus.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

 [HttpGet]
public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookings(
    [FromQuery] string? name, 
    [FromQuery] BookingStatus? status)
{
    var query = _context.Bookings.Include(b => b.Room).AsQueryable();

    // Fix 1: Handle null or empty name with case-insensitive search
    if (!string.IsNullOrWhiteSpace(name))
    {
        // Use ILike (Postgres specific) or ToLower for universal support
        query = query.Where(b => b.BookerName.ToLower().Contains(name.ToLower()));
    }

    // Fix 2: Explicitly check the enum value
    if (status.HasValue)
    {
        query = query.Where(b => b.Status == status.Value);
    }

    var results = await query.Select(b => new BookingResponse
    {
        Id = b.Id,
        RoomId = b.RoomId,
        RoomName = b.Room != null ? b.Room.Name : "Unknown Room", // Safety check
        BookerName = b.BookerName,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        Status = b.Status.ToString(),
        RejectionReason = b.RejectionReason
    }).ToListAsync();

    return Ok(results);
}
        // PATCH: api/Bookings/{id}/status (For Issue #2: Approval Workflow)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.Status = request.Status;
            
            if (request.Status == BookingStatus.Rejected)
            {
                booking.RejectionReason = request.Reason;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponse>> CreateBooking(CreateBookingRequest request)
        {
            // Verify room exists
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId);
            if (!roomExists) return BadRequest("Room does not exist.");

            var booking = new Booking
            {
                RoomId = request.RoomId,
                BookerName = request.BookerName,
                BookerEmail = request.BookerEmail,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Booking created successfully", BookingId = booking.Id });
        }
    }
}