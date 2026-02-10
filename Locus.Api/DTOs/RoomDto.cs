namespace Locus.Api.DTOs
{
    // What the frontend sends when creating a room
    public class CreateRoomRequest
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Facilities { get; set; }
    }

    // What the API sends back to the frontend
    public class RoomResponse
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Facilities { get; set; }
        public bool IsAvailable { get; set; }
    }
}