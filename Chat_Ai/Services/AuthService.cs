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

        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return AuthResultDto.Fail("البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            if (!isPasswordValid)
            {
                return AuthResultDto.Fail("البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            return AuthResultDto.Ok(user.Id, user.Name);
        }

        public async Task<User> GetOrCreateExternalUserAsync(string email, string name)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Generate a secure random password hash for external users
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"));

                user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Email = email,
                    Password = hashedPassword,
                    CreatedAt = DateTime.Now
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            return user;
        }
    }
}
