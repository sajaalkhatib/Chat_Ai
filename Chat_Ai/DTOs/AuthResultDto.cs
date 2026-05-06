namespace Chat_Ai.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }

        public static AuthResultDto Ok(string userId, string? message = null)
        {
            return new AuthResultDto
            {
                Success = true,
                UserId = userId,
                Message = message
            };
        }

        public static AuthResultDto Fail(string message)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = message
            };
        }
    }
}
