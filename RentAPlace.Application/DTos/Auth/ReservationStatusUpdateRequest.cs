using System.ComponentModel.DataAnnotations;
using System;

namespace RentAPlace.Application.DTOs.Auth
{
    public class ReservationStatusUpdateRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Pending, Confirmed, Cancelled
    }
}
