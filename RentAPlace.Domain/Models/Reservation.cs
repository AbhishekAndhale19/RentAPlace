using System;

namespace RentAPlace.Domain.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid PropertyId { get; set; }
        public Property? Property { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
