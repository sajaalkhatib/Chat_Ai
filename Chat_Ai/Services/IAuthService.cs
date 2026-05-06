using Chat_Ai.DTOs;

namespace Chat_Ai.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    }
}
