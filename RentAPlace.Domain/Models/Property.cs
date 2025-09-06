using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentAPlace.Domain.Models
{
    public class Property
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty; // flat, villa, apartment

        public string Features { get; set; } = string.Empty; // csv/json if needed

        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}
