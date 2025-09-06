namespace RentAPlace.Application.DTOs.Messages
{
    public class SendMessageRequest
    {
        public Guid ReceiverId { get; set; }
        public Guid? PropertyId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public Guid? PropertyId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
