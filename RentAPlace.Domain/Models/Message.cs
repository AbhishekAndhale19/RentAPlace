using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAPlace.Domain.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public User? Receiver { get; set; }

        public Guid? PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property? Property { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
