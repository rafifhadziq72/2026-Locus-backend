using Locus.Api.Data;
using Locus.Api.DTOs;
using Locus.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Locus.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponse>>> GetRooms()
        {
            return await _context
                .Rooms.Select(r => new RoomResponse
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Name = r.Name,
                    Capacity = r.Capacity,
                    Facilities = r.Facilities,
                    IsAvailable = r.IsAvailable,
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<RoomResponse>> CreateRoom(CreateRoomRequest request)
        {
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Name = request.Name,
                Capacity = request.Capacity,
                Facilities = request.Facilities,
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRooms),
                new { id = room.Id },
                new RoomResponse
                {
                    Id = room.Id,
                    RoomNumber = room.RoomNumber,
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Facilities = room.Facilities,
                    IsAvailable = room.IsAvailable,
                }
            );
        }
    }
}
