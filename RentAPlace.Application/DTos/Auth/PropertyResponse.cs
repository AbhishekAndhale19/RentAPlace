using System;
using System.Collections.Generic;

namespace RentAPlace.Application.DTOs.Auth
{
    public class PropertyResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
    }
}
