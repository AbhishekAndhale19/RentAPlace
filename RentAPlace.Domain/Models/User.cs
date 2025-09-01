using System;
using System.Collections.Generic;

namespace RentAPlace.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Role
        public bool IsAdmin { get; set; } = false;

        // Navigation properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
