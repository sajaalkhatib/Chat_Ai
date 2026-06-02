using Chat_Ai.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Chat_Ai.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApi:ApiKey"] ?? "";
            _model = configuration["GeminiApi:Model"] ?? "gemini-2.0-flash";
        }

        public async Task<string> SendMessageAsync(string userMessage, List<Message> history)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey.Contains("YOUR_"))
            {
                return "⚠️ Gemini API Key is not configured. Please add your API key in appsettings.json under GeminiApi:ApiKey. You can get a free key from https://aistudio.google.com/apikey";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            // Build conversation contents from history
            var contents = new List<object>();

            foreach (var msg in history)
            {
                // SenderType: 0 = user, 1 = bot (model)
                var role = msg.SenderType == 0 ? "user" : "model";
                contents.Add(new
                {
                    role = role,
                    parts = new[] { new { text = msg.Content } }
                });
            }

            // Add the new user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = userMessage } }
            });

            var requestBody = new
            {
                contents = contents
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

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
                        return $"⚠️ Gemini API Error: {errorMsg}";
                    }
                    catch
                    {
                        return $"⚠️ Gemini API Error (HTTP {(int)response.StatusCode}): {responseJson}";
                    }
                }

                using var document = JsonDocument.Parse(responseJson);
                var reply = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return reply ?? "No response from Gemini.";
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
