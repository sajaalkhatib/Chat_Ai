using Chat_Ai.DTOs;
using Chat_Ai.Models;

namespace Chat_Ai.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);
        Task<AuthResultDto> LoginAsync(LoginDto dto);
        Task<User> GetOrCreateExternalUserAsync(string email, string name);
    }
}
