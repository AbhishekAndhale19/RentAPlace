using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        public UsersController(RentAPlaceDbContext db) { _db = db; }

        // Public: get own profile (requires auth)
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (sub == null) return Unauthorized();
            var id = Guid.Parse(sub);
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.FullName, user.Email, user.IsAdmin });
        }

        // Admin only: list all users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users.Select(u => new { u.Id, u.FullName, u.Email, u.IsAdmin }).ToListAsync();
            return Ok(users);
        }
    }
}
