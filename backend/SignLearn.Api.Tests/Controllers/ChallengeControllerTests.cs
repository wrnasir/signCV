using Microsoft.AspNetCore.Mvc;
using Moq;
using SignLearn.Api.Controllers;
using SignLearn.Api.DTOs;
using SignLearn.Api.Exceptions;
using SignLearn.Api.Services;
using Xunit;

namespace SignLearn.Api.Tests.Controllers
{
    public class ChallengeControllerTests
    {
        private readonly Mock<IChallengeService> _mockService;
        private readonly ChallengeController _controller;

        public ChallengeControllerTests()
        {
            _mockService = new Mock<IChallengeService>();
            _controller = new ChallengeController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that a valid request returns 200 OK
        /// with the generated challenge.
        /// </summary>
        [Fact]
        public async Task Generate_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            var expectedResponse = new ChallengeResponse
            {
                TargetWord = "HELLO",
                Hint = "A common greeting",
                Difficulty = "medium"
            };

            _mockService
                .Setup(s => s.GenerateChallenge(It.IsAny<ChallengeRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Generate(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ChallengeResponse>(okResult.Value);
            Assert.Equal("HELLO", response.TargetWord);
            Assert.Equal("medium", response.Difficulty);
        }

        /// <summary>
        /// Verifies that when the service throws (e.g., Groq failure),
        /// the controller returns 500 with the error message.
        /// </summary>
        [Fact]
        public async Task Generate_ServiceThrows_Returns500()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            _mockService
                .Setup(s => s.GenerateChallenge(It.IsAny<ChallengeRequest>()))
                .ThrowsAsync(new Exception("LLM parsing failed"));

            // Act
            var result = await _controller.Generate(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        /// <summary>
        /// Verifies that a GroqException from the service is caught
        /// and returns 500. In the future this could return a more
        /// specific status code (e.g., 503 for rate limiting).
        /// </summary>
        [Fact]
        public async Task Generate_GroqException_Returns500()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            _mockService
                .Setup(s => s.GenerateChallenge(It.IsAny<ChallengeRequest>()))
                .ThrowsAsync(new GroqException(429, "Rate limit exceeded"));

            // Act
            var result = await _controller.Generate(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        /// <summary>
        /// Verifies that the controller passes the request through
        /// to the service exactly once.
        /// </summary>
        [Fact]
        public async Task Generate_ValidRequest_CallsServiceOnce()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 3,
                Streak = 2,
                MasteredSigns = new List<string> { "A", "B" },
                UsedWords = new List<string> { "CAT" }
            };

            _mockService
                .Setup(s => s.GenerateChallenge(It.IsAny<ChallengeRequest>()))
                .ReturnsAsync(new ChallengeResponse
                {
                    TargetWord = "DOG",
                    Hint = "A pet",
                    Difficulty = "easy"
                });

            // Act
            await _controller.Generate(request);

            // Assert
            _mockService.Verify(
                s => s.GenerateChallenge(It.IsAny<ChallengeRequest>()),
                Times.Once
            );
        }
    }
}