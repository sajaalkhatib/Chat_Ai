using Chat_Ai.Models;

namespace Chat_Ai.Services
{
    public interface IChatService
    {
        Task<(string Reply, int ChatId)> ProcessMessageAsync(string userId, string message, int? chatId);
        Task<List<Chat>> GetUserChatsAsync(string userId);
        Task<List<Message>> GetChatMessagesAsync(int chatId, string userId);
        Task<int> CreateNewChatAsync(string userId);
        Task<bool> DeleteChatAsync(int chatId, string userId);
    }
}
