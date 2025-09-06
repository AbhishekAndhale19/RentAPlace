using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using RentAPlace.Application.DTOs.Auth;
using RentAPlace.Api.Services;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        private readonly Email _emailService;

        public UsersController(RentAPlaceDbContext db, Email emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userId, out var guidId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == guidId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new UserResponse(user.Id, user.FullName, user.Email, user.Role.ToString()));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict(new { message = "Email already registered." });

            if (!Enum.TryParse<UserRole>(dto.Role, out var role))
                return BadRequest(new { message = "Invalid role." });

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                Role = role,
                ResetToken = Guid.NewGuid().ToString("N"),
                ResetTokenExpires = DateTime.UtcNow.AddHours(1)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Send email to user to set their password
            var resetLink = $"http://localhost:4200/set-password?token={user.ResetToken}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Set Your RentAPlace Password",
                $"Hello {user.FullName},<br>Please set your password by clicking <a href='{resetLink}'>here</a>. The link will expire in 1 hour."
            );

            return Ok(new
            {
                message = "User created successfully. Password setup email sent.",
                userId = user.Id,
                role = user.Role.ToString()
            });
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
            if (dto.Role != null && Enum.TryParse<UserRole>(dto.Role, out var role))
                user.Role = role;

            await _db.SaveChangesAsync();
            return Ok(new { message = "User updated successfully." });
        }

        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!Guid.TryParse(userId, out var guidId))
                return Unauthorized(new { message = "Invalid or missing token." });

            var user = await _db.Users.FindAsync(guidId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != guidId);
                if (exists)
                    return Conflict(new { message = "Email already in use." });
                user.Email = dto.Email;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok(new { message = "If the email exists, a reset link has been sent." });

            user.ResetToken = Guid.NewGuid().ToString("N");
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync();

            var resetLink = $"http://localhost:4200/set-password?token={user.ResetToken}";
            await _emailService.SendEmailAsync(
                user.Email,
                "Reset Your RentAPlace Password",
                $"Click <a href='{resetLink}'>here</a> to reset your password. The link expires in 1 hour."
            );

            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }
    }

    public record UserResponse(Guid Id, string FullName, string Email, string Role);

    public class EditUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class AdminCreateUserRequest
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required][MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = "User";
    }
}
