using System.Net;
using System.Text;
using System.Text.Json;
using SignLearn.Api.Exceptions;
using SignLearn.Api.Services;
using Xunit;

namespace SignLearn.Api.Tests.Services
{
    public class GroqServiceTests
    {
        /// <summary>
        /// Creates a GroqService with a mocked HTTP handler
        /// that returns a predefined response.
        /// </summary>
        private static GroqService CreateServiceWithHandler(MockHttpHandler handler)
        {
            Environment.SetEnvironmentVariable("GROQ_API_KEY", "test-key-for-unit-tests");
            var client = new HttpClient(handler);
            return new GroqService(client);
        }

        /// <summary>
        /// Verifies that a successful Groq response is parsed
        /// and the message content is extracted correctly.
        /// </summary>
        [Fact]
        public async Task SendPrompt_SuccessfulResponse_ReturnsContent()
        {
            // Arrange
            var groqResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new { content = "Hello from Groq!" }
                    }
                }
            };

            var handler = new MockHttpHandler(
                HttpStatusCode.OK,
                JsonSerializer.Serialize(groqResponse)
            );

            var service = CreateServiceWithHandler(handler);

            // Act
            var result = await service.SendPrompt("Say hello");

            // Assert
            Assert.Equal("Hello from Groq!", result);
        }

        /// <summary>
        /// Verifies that a non-success status code from Groq
        /// throws a GroqException with the correct status code.
        /// </summary>
        [Fact]
        public async Task SendPrompt_ApiError_ThrowsGroqException()
        {
            // Arrange
            var handler = new MockHttpHandler(
                HttpStatusCode.TooManyRequests,
                "Rate limit exceeded"
            );

            var service = CreateServiceWithHandler(handler);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GroqException>(
                () => service.SendPrompt("test prompt")
            );
            Assert.Equal(429, ex.StatusCode);
        }

        /// <summary>
        /// Verifies that a 500 from Groq throws a GroqException
        /// containing the error body.
        /// </summary>
        [Fact]
        public async Task SendPrompt_ServerError_ThrowsWithBody()
        {
            // Arrange
            var handler = new MockHttpHandler(
                HttpStatusCode.InternalServerError,
                "Internal server error"
            );

            var service = CreateServiceWithHandler(handler);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<GroqException>(
                () => service.SendPrompt("test prompt")
            );
            Assert.Equal(500, ex.StatusCode);
            Assert.Contains("Internal server error", ex.ResponseBody);
        }

        /// <summary>
        /// Verifies that an empty content field in the Groq response
        /// throws an exception.
        /// </summary>
        [Fact]
        public async Task SendPrompt_EmptyContent_ThrowsException()
        {
            // Arrange
            var groqResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new { content = (string?)null }
                    }
                }
            };

            var handler = new MockHttpHandler(
                HttpStatusCode.OK,
                JsonSerializer.Serialize(groqResponse)
            );

            var service = CreateServiceWithHandler(handler);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => service.SendPrompt("test prompt")
            );
        }

        /// <summary>
        /// Verifies that the system prompt is customizable
        /// and still returns a valid response.
        /// </summary>
        [Fact]
        public async Task SendPrompt_CustomSystemPrompt_Succeeds()
        {
            // Arrange
            var groqResponse = new
            {
                choices = new[]
                {
                    new
                    {
                        message = new { content = "Custom response" }
                    }
                }
            };

            var handler = new MockHttpHandler(
                HttpStatusCode.OK,
                JsonSerializer.Serialize(groqResponse)
            );

            var service = CreateServiceWithHandler(handler);

            // Act
            var result = await service.SendPrompt("test", "You are an ASL teacher.");

            // Assert
            Assert.Equal("Custom response", result);
        }
    }

    /// <summary>
    /// Mock HTTP handler that returns a predefined status code and body.
    /// Used to test GroqService without making real API calls.
    /// </summary>
    public class MockHttpHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;

        public MockHttpHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}