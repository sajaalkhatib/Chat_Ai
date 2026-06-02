using Chat_Ai.Models;

namespace Chat_Ai.Repositories
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetChatMessagesAsync(int chatId);
        Task AddAsync(Message message);
        Task SaveChangesAsync();
    }
}
