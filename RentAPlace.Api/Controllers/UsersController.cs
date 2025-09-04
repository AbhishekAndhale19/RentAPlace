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
    public class UsersController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;

        public UsersController(RentAPlaceDbContext db)
        {
            _db = db;
        }

        // GET: api/users/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(userId, out var guidId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == guidId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.IsOwner));
        }

        // GET: api/users (Owner only)
        [Authorize(Roles = "Owner")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.IsOwner))
                .ToListAsync();

            return Ok(users);
        }

        // PATCH: api/users/change-password
        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(userId, out var guidId))
                return Unauthorized();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == guidId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { message = "Old password is incorrect." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }

    // Response record
    public record UserResponse(Guid Id, string FullName, string Email, bool IsOwner);
}
