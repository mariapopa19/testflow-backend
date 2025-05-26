using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TestFlow.Application.Interfaces.Services;

namespace TestFlow.Application.Services
{
    public class AIClientService : IAIClientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiApiKey;

        public AIClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _aiApiKey = configuration["HuggingFace:ApiKey"]!;
        }

        public async Task<string> GetPromptResponseAsync(string prompt)
        {
            var routerBody = new
            {
                model = "mistralai/Mistral-7B-Instruct-v0.3",
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.3/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(routerBody), Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _aiApiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(content).RootElement;
            var rawJson = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return rawJson ?? string.Empty;
        }
    }
}

