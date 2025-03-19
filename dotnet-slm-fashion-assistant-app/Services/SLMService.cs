using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace dotnetfashionassistant.Services
{
    public class SLMService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public SLMService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["FashionAssistantAPI:Url"] ?? "httpL//localhost:11434";
        }

        public async IAsyncEnumerable<string> StreamChatCompletionsAsync(string prompt)
        {
            var requestPayload = new
            {
                messages = new[]
                {
                        new { role = "system", content = "You are a helpful assistant." },
                        new { role = "user", content = prompt }
                    },
                stream = true,
                cache_prompt = false,
                n_predict = 150
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                line = line?.Replace("data: ", string.Empty).Trim();
                if (!string.IsNullOrEmpty(line) && line != "[DONE]")
                {
                    var jsonObject = JsonNode.Parse(line);
                    var responseContent = jsonObject?["choices"]?[0]?["delta"]?["content"]?.ToString();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        yield return responseContent;
                    }
                }
            }
        }
    }
}
