using Chat_Ai.Models;

namespace Chat_Ai.Services
{
    public interface IOpenAiService
    {
        Task<string> SendMessageAsync(string userMessage, List<Message> history);
    }
}
