using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentAPlace.Domain.Models;
using RentAPlace.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthController(RentAPlaceDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        // Admin-only endpoint to view all users
        [Authorize(Roles = "Admin")]
        [HttpGet("all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/auth/register
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
        return Conflict(new { message = "Email already registered." });

    // Assign role based on IsOwner checkbox
    var role = dto.IsOwner ? UserRole.Owner : UserRole.User;

    var user = new User
    {
        Id = Guid.NewGuid(),
        FullName = dto.FullName,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Role = role  // <-- FIXED HERE
    };

    _db.Users.Add(user);
    await _db.SaveChangesAsync();

    return Ok(new
    {
        message = "Registered successfully",
        userId = user.Id,
        role = user.Role.ToString()
    });
}


        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = CreateToken(user);

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            return Ok(new
            {
                accessToken = token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    role = user.Role.ToString()
                }
            });
        }

        private string CreateToken(User user)
        {
            var jwtCfg = _cfg.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtCfg["Issuer"],
                audience: jwtCfg["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtCfg["AccessTokenMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password reset token generated.", token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.ResetToken == dto.Token && u.ResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest(new { message = "Invalid or expired reset token." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
