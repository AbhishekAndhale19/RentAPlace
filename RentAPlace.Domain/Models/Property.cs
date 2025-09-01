using System;

namespace RentAPlace.Domain.Models
{
    public class Property
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Foreign key → User (Owner)
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
    }
}
