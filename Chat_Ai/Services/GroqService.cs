using Chat_Ai.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Chat_Ai.Services
{
    public class GroqService : IGroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"] ?? "";
            _model = configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";
        }

        public async Task<string> SendMessageAsync(string userMessage, List<Message> history)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey.Contains("YOUR_"))
            {
                return "⚠️ Groq API Key is not configured. Please add your API key in appsettings.json under Groq:ApiKey. You can get a free key from https://console.groq.com/keys";
            }

            var url = "https://api.groq.com/openai/v1/chat/completions";

            // Build conversation messages from history
            var messages = new List<object>();

            // Add a system message for context
            messages.Add(new
            {
                role = "system",
                content = "You are a helpful AI assistant. Respond clearly and concisely."
            });

            foreach (var msg in history)
            {
                // SenderType: 0 = user, 1 = bot (assistant)
                var role = msg.SenderType == 0 ? "user" : "assistant";
                messages.Add(new
                {
                    role = role,
                    content = msg.Content
                });
            }

            // Add the new user message
            messages.Add(new
            {
                role = "user",
                content = userMessage
            });

            var requestBody = new
            {
                model = _model,
                messages = messages,
                max_tokens = 2048,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Set Authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Try to extract error message
                    try
                    {
                        using var doc = JsonDocument.Parse(responseJson);
                        var errorMsg = doc.RootElement
                            .GetProperty("error")
                            .GetProperty("message")
                            .GetString();
                        return $"⚠️ Groq API Error: {errorMsg}";
                    }
                    catch
                    {
                        return $"⚠️ Groq API Error (HTTP {(int)response.StatusCode}): {responseJson}";
                    }
                }

                using var document = JsonDocument.Parse(responseJson);
                var reply = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return reply ?? "No response from Groq.";
            }
            catch (HttpRequestException ex)
            {
                return $"⚠️ Connection error: {ex.Message}";
            }
            catch (TaskCanceledException)
            {
                return "⚠️ Request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                return $"⚠️ Unexpected error: {ex.Message}";
            }
        }
    }
}
