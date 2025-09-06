using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using RentAPlace.Application.DTOs.Auth;
using System.Security.Claims;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(RentAPlaceDbContext db, IWebHostEnvironment env, ILogger<PropertiesController> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        // GET: api/properties
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] string? type)
        {
            var query = _db.Properties.Include(p => p.Images).Include(p => p.Owner).Include(p => p.Ratings).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Title.Contains(q) || p.Location.Contains(q) || p.Features.Contains(q));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(p => p.Type == type);

            var list = await query.Select(p => new PropertyResponse
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null ? p.Owner.FullName : string.Empty,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Type = p.Type,
                Features = p.Features,
                Images = p.Images.Select(i => i.Url).ToList(),
                AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0
            }).ToListAsync();

            return Ok(list);
        }

        // GET: api/properties/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var p = await _db.Properties
                .Include(x => x.Images)
                .Include(x => x.Owner)
                .Include(x => x.Ratings)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            var resp = new PropertyResponse
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner?.FullName ?? string.Empty,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Type = p.Type,
                Features = p.Features,
                Images = p.Images.Select(i => i.Url).ToList(),
                AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0
            };

            return Ok(resp);
        }

        // POST: api/properties  (Owner or Admin)
        [Authorize(Roles = "Owner,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] PropertyCreateRequest dto, [FromForm] List<IFormFile>? images)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var ownerId))
                return Unauthorized();

            var p = new Property
            {
                OwnerId = ownerId,
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                Type = dto.Type,
                Features = dto.Features
            };

            _db.Properties.Add(p);
            await _db.SaveChangesAsync(); // persist to get property id

            // Handle images
            var savedImages = new List<PropertyImage>();
            if (images != null && images.Count > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                Directory.CreateDirectory(uploads);

                var existingCount = await _db.PropertyImages.CountAsync(pi => pi.PropertyId == p.Id);
                var allowed = Math.Max(0, 5 - existingCount);

                foreach (var file in images.Take(allowed))
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                        continue;

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploads, fileName);

                    using var fs = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(fs);

                    var url = $"/uploads/{fileName}";
                    var img = new PropertyImage { PropertyId = p.Id, Url = url };
                    _db.PropertyImages.Add(img);
                    savedImages.Add(img);
                }
                await _db.SaveChangesAsync();
            }

            var resp = new PropertyResponse
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner?.FullName ?? string.Empty,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Type = p.Type,
                Features = p.Features,
                Images = savedImages.Select(x => x.Url).ToList(),
                AverageRating = 0
            };

            return Ok(resp);
        }

        // PUT: api/properties/{id}
        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PropertyCreateRequest dto)
        {
            var property = await _db.Properties.FindAsync(id);
            if (property == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (property.OwnerId.ToString() != userId && role != "Admin")
                return Forbid();

            property.Title = dto.Title;
            property.Description = dto.Description;
            property.Price = dto.Price;
            property.Location = dto.Location;
            property.Type = dto.Type;
            property.Features = dto.Features;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Property updated" });
        }

        // DELETE: api/properties/{id}
        [Authorize(Roles = "Owner,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var property = await _db.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (property.OwnerId.ToString() != userId && role != "Admin")
                return Forbid();

            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            foreach (var img in property.Images)
            {
                try
                {
                    var filePath = Path.Combine(uploads, Path.GetFileName(img.Url));
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image file {Url}", img.Url);
                }
            }

            _db.Properties.Remove(property);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Property deleted" });
        }

        //  Search endpoint
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? location, [FromQuery] string? type, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] string? features)
        {
            var query = _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Ratings)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(p => p.Location.Contains(location));

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(p => p.Type == type);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(features))
                query = query.Where(p => p.Features.Contains(features));

            var results = await query.Select(p => new PropertyResponse
            {
                Id = p.Id,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner != null ? p.Owner.FullName : string.Empty,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Location = p.Location,
                Type = p.Type,
                Features = p.Features,
                Images = p.Images.Select(i => i.Url).ToList(),
                AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0
            }).ToListAsync();

            return Ok(results);
        }

        //  Top-rated properties
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRated([FromQuery] int count = 5)
        {
            var top = await _db.Properties
                .Include(p => p.Images)
                .Include(p => p.Ratings)
                .OrderByDescending(p => p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0)
                .Take(count)
                .Select(p => new PropertyResponse
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner != null ? p.Owner.FullName : string.Empty,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Location = p.Location,
                    Type = p.Type,
                    Features = p.Features,
                    Images = p.Images.Select(i => i.Url).ToList(),
                    AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0
                })
                .ToListAsync();

            return Ok(top);
        }

        //  Upload property images (max 5)
        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImages(Guid id, [FromForm] List<IFormFile> images)
        {
            var property = await _db.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (property.OwnerId.ToString() != userId && role != "Admin")
                return Forbid();

            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            Directory.CreateDirectory(uploads);

            var existingCount = property.Images.Count;
            var allowed = Math.Max(0, 5 - existingCount);

            var savedImages = new List<PropertyImage>();
            foreach (var file in images.Take(allowed))
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                    continue;

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);

                using var fs = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fs);

                var url = $"/uploads/{fileName}";
                var img = new PropertyImage { PropertyId = property.Id, Url = url };
                _db.PropertyImages.Add(img);
                savedImages.Add(img);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"{savedImages.Count} image(s) uploaded",
                images = savedImages.Select(x => x.Url).ToList()
            });
        }


        // GET: api/properties/owner
        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("owner")]
        public async Task<IActionResult> GetMyProperties()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var ownerId))
                return Unauthorized();

            var properties = await _db.Properties
                .Where(p => p.OwnerId == ownerId)
                .Include(p => p.Images)
                .Include(p => p.Ratings)
                .Select(p => new PropertyResponse
                {
                    Id = p.Id,
                    OwnerId = p.OwnerId,
                    OwnerName = p.Owner != null ? p.Owner.FullName : string.Empty,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Location = p.Location,
                    Type = p.Type,
                    Features = p.Features,
                    Images = p.Images.Select(i => i.Url).ToList(),
                    AverageRating = p.Ratings.Any() ? p.Ratings.Average(r => r.Stars) : 0
                })
                .ToListAsync();

            return Ok(properties);
        }
    }
}
