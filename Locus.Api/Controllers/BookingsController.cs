using Locus.Api.Data;
using Locus.Api.DTOs;
using Locus.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            [FromQuery] BookingStatus? status
        )
        {
            var query = _context.Bookings.Include(b => b.Room).AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(b => b.BookerName.ToLower().Contains(name.ToLower()));
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            var bookings = await query.ToListAsync();

            var results = bookings
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    RoomName = b.Room?.Name ?? "Unknown Room",
                    BookerName = b.BookerName,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Status = b.Status.ToString(),
                    RejectionReason = b.RejectionReason,
                })
                .ToList();

            return Ok(results);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateBookingStatusRequest request
        )
        {
            var booking = await _context
                .Bookings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            if (request.Status == BookingStatus.Approved)
            {
                var isAlreadyOccupied = await _context.Bookings.AnyAsync(b =>
                    b.RoomId == booking.RoomId
                    && b.Id != id
                    && b.Status == BookingStatus.Approved
                    && booking.StartTime < b.EndTime
                    && booking.EndTime > b.StartTime
                );

                if (isAlreadyOccupied)
                {
                    return BadRequest(
                        "Cannot approve this booking because the room is already occupied by another approved booking during this time."
                    );
                }
            }

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
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId);
            if (!roomExists)
                return BadRequest("Room does not exist.");

            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId
                && b.IsDeleted == false
                && request.StartTime < b.EndTime
                && request.EndTime > b.StartTime
            );

            if (isOverlapping)
            {
                return BadRequest(
                    "This room is already reserved or has a pending request for the selected time slot."
                );
            }

            var booking = new Booking
            {
                RoomId = request.RoomId,
                BookerName = request.BookerName,
                BookerEmail = request.BookerEmail,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = BookingStatus.Pending,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(
                new { Message = "Booking request submitted successfully.", BookingId = booking.Id }
            );
        }
    }
}
