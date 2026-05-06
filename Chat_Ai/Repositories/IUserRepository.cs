using Chat_Ai.Models;

namespace Chat_Ai.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
