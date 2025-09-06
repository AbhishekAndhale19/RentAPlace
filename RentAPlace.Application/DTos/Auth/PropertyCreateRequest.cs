using System.ComponentModel.DataAnnotations;


namespace RentAPlace.Application.DTOs.Auth
{
    // For [FromForm] binding
    public class PropertyCreateRequest
    {
        [Required] public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public decimal Price { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;

        // Note: images are bound as IFormFile list in controller directly
    }
}
