using System;

namespace RentAPlace.Domain.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PropertyId { get; set; }
        public Property? Property { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
