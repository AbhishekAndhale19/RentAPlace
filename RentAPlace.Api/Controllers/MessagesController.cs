using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAPlace.Domain.Models;
using RentAPlace.Application.DTOs.Messages;
using RentAPlace.Api.Services;
using System.Security.Claims;

namespace RentAPlace.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly RentAPlaceDbContext _db;
        private readonly Email _emailService;

        public MessagesController(RentAPlaceDbContext db, Email emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        // Send a message
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest dto)
        {
            var senderIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(senderIdStr, out var senderId))
                return Unauthorized();

            var sender = await _db.Users.FindAsync(senderId);
            var receiver = await _db.Users.FindAsync(dto.ReceiverId);
            if (receiver == null)
                return NotFound(new { message = "Receiver not found." });

            var message = new Message
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                PropertyId = dto.PropertyId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            // Email notification
            await _emailService.SendEmailAsync(
                receiver.Email,
                "New message received",
                $"Hello {receiver.FullName},<br>You received a new message from {sender.FullName}:<br><b>{dto.Content}</b>"
            );

            return Ok(new { message = "Message sent successfully." });
        }

        // Get inbox messages
        [HttpGet("inbox")]
        public async Task<IActionResult> Inbox()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var messages = await _db.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.FullName,
                    PropertyId = m.PropertyId,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        // Get sent messages
        [HttpGet("sent")]
        public async Task<IActionResult> Sent()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var messages = await _db.Messages
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MessageResponse
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver.FullName,
                    PropertyId = m.PropertyId,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages);
        }

        // Mark a message as read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var message = await _db.Messages.FindAsync(id);
            if (message == null)
                return NotFound(new { message = "Message not found." });

            message.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Message marked as read." });
        }
    }
}
