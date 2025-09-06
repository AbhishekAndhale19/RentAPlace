using System;

namespace RentAPlace.Domain.Models
{
    public class Rating
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int Stars { get; set; } // 1–5
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
