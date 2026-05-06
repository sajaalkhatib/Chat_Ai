using Chat_Ai.DTOs;
using Chat_Ai.Models;
using Chat_Ai.Repositories;

namespace Chat_Ai.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            bool emailExists = await _userRepository.EmailExistsAsync(dto.Email);
            if (emailExists)
            {
                return AuthResultDto.Fail("البريد الإلكتروني مسجل مسبقاً");
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create the user entity
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword,
                CreatedAt = DateTime.Now
            };

            // Save to database
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return AuthResultDto.Ok(user.Id, "تم إنشاء الحساب بنجاح");
        }
    }
}
