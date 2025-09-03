using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using System.Security.Claims;

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

        // GET: api/users/me (profile of logged-in user)
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
    }

    // DTO
    public record UserResponse(Guid Id, string FullName, string Email, bool IsOwner);
}
