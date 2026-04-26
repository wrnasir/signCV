using System.Text;
using System.Text.Json;
using SignLearn.Api.Exceptions;

namespace SignLearn.Api.Services
{
    /// <summary>
    /// Generic service to handle all HTTP communication with the Groq API.
    /// </summary>
    public class GroqService : IGroqService
    {
        private readonly HttpClient _client;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidOperationException("GROQ_API_KEY environment variable not set");
        private readonly string _url = "https://api.groq.com/openai/v1/chat/completions";
        private readonly string _model = "llama-3.1-8b-instant";

        /// <summary>
        /// Initializes GroqService with an injected HttpClient.
        /// </summary>
        public GroqService(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Sends a prompt to the Groq API and returns the response content as a string.
        /// </summary>
        /// <param name="prompt">The user prompt to send.</param>
        /// <param name="systemPrompt">Optional system prompt to set context.</param>
        /// <returns>The raw text content from the LLM response.</returns>
        public async Task<string> SendPrompt(string prompt, string systemPrompt = "You are a helpful assistant.")
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 1024
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _client.PostAsync(_url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new GroqException((int)response.StatusCode, errorBody);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse out just the message content from the Groq response
            using var doc = JsonDocument.Parse(responseBody);
            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? throw new Exception("Groq returned empty response");
        }
    }
}