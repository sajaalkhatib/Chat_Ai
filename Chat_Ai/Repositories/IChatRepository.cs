using Chat_Ai.Models;

namespace Chat_Ai.Repositories
{
    public interface IChatRepository
    {
        Task<Chat?> GetByIdWithMessagesAsync(int chatId);
        Task<List<Chat>> GetUserChatsAsync(string userId);
        Task AddAsync(Chat chat);
        Task DeleteAsync(Chat chat);
        Task SaveChangesAsync();
    }
}
