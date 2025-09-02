using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentAPlace.Domain.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; } = string.Empty;

        // Role: false = Renter (User), true = Owner
        public bool IsOwner { get; set; } = false;

        // Navigation properties
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
