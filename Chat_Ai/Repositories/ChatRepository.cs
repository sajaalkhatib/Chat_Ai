using Chat_Ai.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat_Ai.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly MyDbContext _context;

        public ChatRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Chat?> GetByIdWithMessagesAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _context.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
        }

        public async Task DeleteAsync(Chat chat)
        {
            // Delete all messages first, then the chat
            var messages = await _context.Messages.Where(m => m.ChatId == chat.Id).ToListAsync();
            _context.Messages.RemoveRange(messages);
            _context.Chats.Remove(chat);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
