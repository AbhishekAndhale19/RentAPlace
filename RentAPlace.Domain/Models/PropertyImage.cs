using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAPlace.Domain.Models
{
    public class PropertyImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid PropertyId { get; set; }
        public Property? Property { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty; // relative URL, e.g. /uploads/abc.jpg
    }
}
