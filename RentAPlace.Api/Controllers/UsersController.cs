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

            return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role.ToString()));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.Role.ToString()))
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditUser(Guid id, [FromBody] EditUserRequest dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            if (dto.Role != null)
            {
                if (Enum.TryParse<UserRole>(dto.Role, out var role))
                    user.Role = role;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "User updated successfully." });
        }

        // DTO for editing user
        public class EditUserRequest
        {
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
        }


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
        
        [Authorize]
[HttpPatch("me")]
public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest dto)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub");

    if (!Guid.TryParse(userId, out var guidId))
        return Unauthorized(new { message = "Invalid or missing token." });

    var user = await _db.Users.FindAsync(guidId);
    if (user == null)
        return NotFound(new { message = "User not found." });

    // Update only allowed fields
    if (!string.IsNullOrWhiteSpace(dto.FullName))
        user.FullName = dto.FullName;

    if (!string.IsNullOrWhiteSpace(dto.Email))
    {
        // check if email is already used
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != guidId);
        if (exists)
            return Conflict(new { message = "Email already in use." });

        user.Email = dto.Email;
    }

    await _db.SaveChangesAsync();

    return Ok(new { message = "Profile updated successfully." });
}

    }

    

    public record UserResponse(Guid Id, string FullName, string Email, string Role);
}
