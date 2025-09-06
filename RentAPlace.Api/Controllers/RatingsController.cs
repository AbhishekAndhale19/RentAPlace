using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using RentAPlace.Application.DTOs.Ratings;
using System.Security.Claims;

namespace RentAPlace.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly RentAPlaceDbContext _context;

        public RatingsController(RentAPlaceDbContext context)
        {
            _context = context;
        }

        // POST /api/ratings/{propertyId}
        [HttpPost("{propertyId}")]
        public async Task<ActionResult<RatingResponse>> AddRating(Guid propertyId, [FromBody] RatingCreateRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            if (request.Stars < 1 || request.Stars > 5)
                return BadRequest("Stars must be between 1 and 5.");

            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                PropertyId = propertyId,
                UserId = userId,
                Stars = request.Stars,
                Comment = request.Comment ?? string.Empty
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return Ok(new RatingResponse
            {
                Id = rating.Id,
                Stars = rating.Stars,
                Comment = rating.Comment,
                UserName = user?.FullName ?? "Unknown",
                CreatedAt = rating.CreatedAt
            });
        }

        // GET /api/ratings/{propertyId}
        [HttpGet("{propertyId}")]
        public async Task<ActionResult<IEnumerable<RatingResponse>>> GetRatings(Guid propertyId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.PropertyId == propertyId)
                .Include(r => r.User)
                .Select(r => new RatingResponse
                {
                    Id = r.Id,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    UserName = r.User != null ? r.User.FullName : "Unknown",
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(ratings);
        }

        // PUT /api/ratings/{ratingId}  => Update only own rating
        [HttpPut("{ratingId}")]
        public async Task<IActionResult> UpdateRating(Guid ratingId, [FromBody] RatingCreateRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating == null)
                return NotFound();

            if (rating.UserId != userId)
                return Forbid();

            if (request.Stars < 1 || request.Stars > 5)
                return BadRequest("Stars must be between 1 and 5.");

            rating.Stars = request.Stars;
            rating.Comment = request.Comment ?? string.Empty;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Rating updated successfully." });
        }

        // DELETE /api/ratings/{ratingId}  => Delete only own rating
        [HttpDelete("{ratingId}")]
        public async Task<IActionResult> DeleteRating(Guid ratingId)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();

            var rating = await _context.Ratings.FindAsync(ratingId);
            if (rating == null)
                return NotFound();

            if (rating.UserId != userId)
                return Forbid();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Rating deleted successfully." });
        }
    }
}
