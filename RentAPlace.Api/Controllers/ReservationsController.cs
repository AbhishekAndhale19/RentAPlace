using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using System.Security.Claims;
using RentAPlace.Application.DTOs.Auth;
using RentAPlace.Api.Services;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        private readonly Email _email;
        public ReservationsController(RentAPlaceDbContext db, Email email)
        {
            _db = db;
            _email = email;
        }

        // POST: api/reservations
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reservation dto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Unauthorized();

            dto.UserId = uid;
            dto.CreatedAt = DateTime.UtcNow;

            _db.Reservations.Add(dto);
            await _db.SaveChangesAsync();

            // Send email to owner
            var property = await _db.Properties.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == dto.PropertyId);
            if (property?.Owner != null)
            {
                await _email.SendEmailAsync(
                    property.Owner.Email,
                    "New Reservation Received",
                    $"Hello {property.Owner.FullName},<br>" +
                    $"A new reservation has been made by user ID {dto.UserId} for your property <b>{property.Title}</b>." +
                    $"<br>Check your dashboard for details."
                );
            }

            return Ok(dto);
        }

        // GET: api/reservations/my
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyReservations()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            IQueryable<Reservation> query = _db.Reservations.Include(r => r.Property);

            query = role switch
            {
                "User" => query.Where(r => r.UserId == uid),
                "Owner" => query.Where(r => r.Property.OwnerId == uid),
                _ => query
            };

            var reservations = await query.ToListAsync();
            return Ok(reservations);
        }

        // GET: api/reservations/property/{propertyId}
        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetReservationsForProperty(Guid propertyId)
        {
            var property = await _db.Properties
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return NotFound(new { message = "Property not found." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            // Only owner of property or admin can see
            if (property.OwnerId.ToString() != userId && role != "Admin")
                return Forbid();

            var reservations = await _db.Reservations
                .Where(r => r.PropertyId == propertyId)
                .Include(r => r.User)
                .ToListAsync();

            return Ok(reservations);
        }


        // DELETE: api/reservations/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var reservation = await _db.Reservations.Include(r => r.Property).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (reservation.UserId.ToString() != userId &&
                reservation.Property.OwnerId.ToString() != userId &&
                role != "Admin")
                return Forbid();

            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Reservation cancelled" });
        }

        // PUT: api/reservations/{id}/status
        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ReservationStatusUpdateRequest dto)
        {
            var reservation = await _db.Reservations.Include(r => r.Property).Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (reservation.Property.OwnerId.ToString() != userId && role != "Admin")
                return Forbid();

            var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
            if (!allowedStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status" });

            reservation.Status = dto.Status;
            await _db.SaveChangesAsync();

            // Send email to user if confirmed
            if (dto.Status == "Confirmed" && reservation.User != null)
            {
                await _email.SendEmailAsync(
                    reservation.User.Email,
                    "Reservation Confirmed",
                    $"Hello {reservation.User.FullName},<br>" +
                    $"Your reservation for <b>{reservation.Property.Title}</b> has been confirmed by the owner."
                );
            }

            return Ok(new { message = $"Reservation status updated to {dto.Status}" });
        }

    }
}
