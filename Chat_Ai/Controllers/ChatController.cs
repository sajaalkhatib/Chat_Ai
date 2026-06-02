using Chat_Ai.DTOs;
using Chat_Ai.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat_Ai.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            var (reply, chatId) = await _chatService.ProcessMessageAsync(userId, dto.Message, dto.ChatId);

            return Json(new { reply, chatId });
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var chats = await _chatService.GetUserChatsAsync(userId);

            var result = chats.Select(c => new
            {
                id = c.Id,
                title = c.Title,
                createdAt = c.CreatedAt
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Messages(int chatId)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var messages = await _chatService.GetChatMessagesAsync(chatId, userId);

            var result = messages.Select(m => new
            {
                id = m.Id,
                senderType = m.SenderType,
                content = m.Content,
                createdAt = m.CreatedAt
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var success = await _chatService.DeleteChatAsync(chatId, userId);

            if (!success)
            {
                return NotFound(new { error = "Chat not found" });
            }

            return Json(new { success = true });
        }
    }
}
