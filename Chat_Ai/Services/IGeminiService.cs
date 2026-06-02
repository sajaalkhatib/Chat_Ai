using Chat_Ai.Models;

namespace Chat_Ai.Services
{
    public interface IGeminiService
    {
        Task<string> SendMessageAsync(string userMessage, List<Message> history);
    }
}
