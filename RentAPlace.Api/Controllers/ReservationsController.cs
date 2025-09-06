using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using System.Security.Claims;
using RentAPlace.Application.DTOs.Auth;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        public ReservationsController(RentAPlaceDbContext db) => _db = db;

        // POST: api/reservations
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reservation dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var uid)) return Unauthorized();

            dto.UserId = uid;
            dto.CreatedAt = DateTime.UtcNow;

            _db.Reservations.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(dto);
        }

        // GET: api/reservations/my
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var uid)) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);
            IQueryable<Reservation> query = _db.Reservations.Include(r => r.Property);

            if (role == "User")
                query = query.Where(r => r.UserId == uid);
            else if (role == "Owner")
                query = query.Where(r => r.Property.OwnerId == uid);

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
    var reservation = await _db.Reservations.Include(r => r.Property).FirstOrDefaultAsync(r => r.Id == id);
    if (reservation == null) return NotFound();

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var role = User.FindFirstValue(ClaimTypes.Role);

    // Only property owner or admin can update status
    if (reservation.Property.OwnerId.ToString() != userId && role != "Admin")
        return Forbid();

    // Validate status
    var allowedStatuses = new[] { "Pending", "Confirmed", "Cancelled" };
    if (!allowedStatuses.Contains(dto.Status))
        return BadRequest(new { message = "Invalid status" });

    reservation.Status = dto.Status;
    await _db.SaveChangesAsync();

    return Ok(new { message = $"Reservation status updated to {dto.Status}" });
}

    }
}
