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
            // Filter out Soft Deleted records
            var query = _context
                .Bookings.Include(b => b.Room)
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(b => b.BookerName.ToLower().Contains(name.ToLower()));

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var bookings = await query.ToListAsync();
            return Ok(
                bookings.Select(b => new BookingResponse
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    RoomName = b.Room?.Name ?? "Unknown Room",
                    BookerName = b.BookerName,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    Status = b.Status.ToString(),
                })
            );
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateBookingStatusRequest request
        )
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
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
                    && !b.IsDeleted
                );

                if (isAlreadyOccupied)
                    return BadRequest("Room occupied during this time.");
            }

            booking.Status = request.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(
            int id,
            [FromBody] CreateBookingRequest request
        )
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return NotFound();

            // 1. Force conversion to UTC before validation
            var startUtc = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);

            // 2. Validate for overlaps using the UTC dates
            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId
                && b.Id != id
                && !b.IsDeleted
                && startUtc < b.EndTime
                && endUtc > b.StartTime
            );

            if (isOverlapping)
                return BadRequest("The updated time slot overlaps with another existing booking.");

            // 3. Update the record
            booking.BookerName = request.BookerName;
            booking.BookerEmail = request.BookerEmail;
            booking.StartTime = startUtc;
            booking.EndTime = endUtc;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Booking updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return NotFound();

            booking.IsDeleted = true;
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponse>> CreateBooking(CreateBookingRequest request)
        {
            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.RoomId == request.RoomId
                && !b.IsDeleted
                && request.StartTime < b.EndTime
                && request.EndTime > b.StartTime
            );

            if (isOverlapping)
                return BadRequest("Room already reserved.");

            var booking = new Booking
            {
                RoomId = request.RoomId,
                BookerName = request.BookerName,
                BookerEmail = request.BookerEmail,
                StartTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc),
                Status = BookingStatus.Pending,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Booking created.", BookingId = booking.Id });
        }
    }
}
