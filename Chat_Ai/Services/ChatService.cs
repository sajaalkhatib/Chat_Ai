using Chat_Ai.Models;
using Chat_Ai.Repositories;

namespace Chat_Ai.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IGeminiService _geminiService;

        public ChatService(IChatRepository chatRepository, IMessageRepository messageRepository, IGeminiService geminiService)
        {
            _chatRepository = chatRepository;
            _messageRepository = messageRepository;
            _geminiService = geminiService;
        }

        public async Task<(string Reply, int ChatId)> ProcessMessageAsync(string userId, string message, int? chatId)
        {
            Chat? chat;

            if (chatId.HasValue && chatId.Value > 0)
            {
                // Load existing chat with its messages
                chat = await _chatRepository.GetByIdWithMessagesAsync(chatId.Value);

                // Verify this chat belongs to the user
                if (chat == null || chat.UserId != userId)
                {
                    // Create a new chat if the provided chatId is invalid
                    chat = null;
                }
            }
            else
            {
                chat = null;
            }

            // Create new chat if needed
            if (chat == null)
            {
                // Use first 50 chars of the message as the chat title
                var title = message.Length > 50 ? message.Substring(0, 50) + "..." : message;

                chat = new Chat
                {
                    UserId = userId,
                    Title = title,
                    CreatedAt = DateTime.Now,
                    Messages = new List<Message>()
                };

                await _chatRepository.AddAsync(chat);
                await _chatRepository.SaveChangesAsync();
            }

            // Get history for this chat
            var history = chat.Messages?.OrderBy(m => m.CreatedAt).ToList() ?? new List<Message>();

            // Send to Gemini with history
            var reply = await _geminiService.SendMessageAsync(message, history);

            // Save user message
            var userMsg = new Message
            {
                ChatId = chat.Id,
                SenderType = 0, // user
                Content = message,
                CreatedAt = DateTime.Now
            };
            await _messageRepository.AddAsync(userMsg);

            // Save bot reply
            var botMsg = new Message
            {
                ChatId = chat.Id,
                SenderType = 1, // bot
                Content = reply,
                CreatedAt = DateTime.Now
            };
            await _messageRepository.AddAsync(botMsg);

            await _messageRepository.SaveChangesAsync();

            return (reply, chat.Id);
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _chatRepository.GetUserChatsAsync(userId);
        }

        public async Task<List<Message>> GetChatMessagesAsync(int chatId, string userId)
        {
            // Verify the chat belongs to the user
            var chat = await _chatRepository.GetByIdWithMessagesAsync(chatId);
            if (chat == null || chat.UserId != userId)
            {
                return new List<Message>();
            }

            return chat.Messages.OrderBy(m => m.CreatedAt).ToList();
        }

        public async Task<int> CreateNewChatAsync(string userId)
        {
            var chat = new Chat
            {
                UserId = userId,
                Title = "New Chat",
                CreatedAt = DateTime.Now
            };

            await _chatRepository.AddAsync(chat);
            await _chatRepository.SaveChangesAsync();

            return chat.Id;
        }

        public async Task<bool> DeleteChatAsync(int chatId, string userId)
        {
            var chat = await _chatRepository.GetByIdWithMessagesAsync(chatId);
            if (chat == null || chat.UserId != userId)
            {
                return false;
            }

            await _chatRepository.DeleteAsync(chat);
            await _chatRepository.SaveChangesAsync();
            return true;
        }
    }
}
