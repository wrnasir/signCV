using Moq;
using SignLearn.Api.DTOs;
using SignLearn.Api.Services;
using Xunit;

namespace SignLearn.Api.Tests.Services
{
    public class ChallengeServiceTests
    {
        private readonly Mock<IGroqService> _mockGroqService;
        private readonly ChallengeService _challengeService;

        /// <summary>
        /// Sets up a mocked GroqService for each test so no actual
        /// API calls are made.
        /// </summary>
        public ChallengeServiceTests()
        {
            _mockGroqService = new Mock<IGroqService>();
            _challengeService = new ChallengeService(_mockGroqService.Object);
        }

        /// <summary>
        /// Verifies that a valid LLM response is correctly parsed
        /// into a ChallengeResponse.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_ValidResponse_ReturnsParsedChallenge()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            var fakeResponse = @"{
                ""targetWord"": ""HELLO"",
                ""hint"": ""A common greeting"",
                ""difficulty"": ""medium""
            }";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakeResponse);

            // Act
            var result = await _challengeService.GenerateChallenge(request);

            // Assert
            Assert.Equal("HELLO", result.TargetWord);
            Assert.Equal("A common greeting", result.Hint);
            Assert.Equal("medium", result.Difficulty);
        }

        /// <summary>
        /// Verifies that the target word is always returned in uppercase
        /// even if the LLM returns lowercase.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_LowercaseResponse_ReturnsUppercaseWord()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 3,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            var fakeResponse = @"{
                ""targetWord"": ""cat"",
                ""hint"": ""A small pet"",
                ""difficulty"": ""easy""
            }";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakeResponse);

            // Act
            var result = await _challengeService.GenerateChallenge(request);

            // Assert
            Assert.Equal("CAT", result.TargetWord);
        }

        /// <summary>
        /// Verifies that markdown backtick wrapping from the LLM
        /// is stripped before parsing.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_BacktickWrapped_StillParses()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            var fakeResponse = "```json\n{\"targetWord\": \"DOG\", \"hint\": \"Mans best friend\", \"difficulty\": \"easy\"}\n```";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakeResponse);

            // Act
            var result = await _challengeService.GenerateChallenge(request);

            // Assert
            Assert.Equal("DOG", result.TargetWord);
        }

        /// <summary>
        /// Verifies that skill levels below 1 are clamped to the default (5).
        /// The prompt should reflect default difficulty, not invalid input.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(11)]
        [InlineData(999)]
        public async Task GenerateChallenge_InvalidSkillLevel_ClampedToDefault(int invalidLevel)
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = invalidLevel,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            var fakeResponse = @"{
                ""targetWord"": ""BOOK"",
                ""hint"": ""You read this"",
                ""difficulty"": ""medium""
            }";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakeResponse);

            // Act
            var result = await _challengeService.GenerateChallenge(request);

            // Assert — should not throw, should return valid response
            Assert.NotNull(result);
            Assert.Equal("BOOK", result.TargetWord);
        }

        /// <summary>
        /// Verifies that when the LLM returns invalid JSON,
        /// the service throws a meaningful exception.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_InvalidJson_ThrowsException()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string>()
            };

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("this is not json at all");

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => _challengeService.GenerateChallenge(request)
            );
        }

        /// <summary>
        /// Verifies that used words are passed through to the prompt
        /// by checking that the GroqService receives a prompt containing them.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_WithUsedWords_IncludesThemInPrompt()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string>(),
                UsedWords = new List<string> { "HOUSE", "CAR", "DOG" }
            };

            string capturedPrompt = "";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((prompt, system) => capturedPrompt = prompt)
                .ReturnsAsync(@"{""targetWord"": ""TREE"", ""hint"": ""Grows in a forest"", ""difficulty"": ""easy""}");

            // Act
            await _challengeService.GenerateChallenge(request);

            // Assert — prompt should contain the blacklisted words
            Assert.Contains("HOUSE", capturedPrompt);
            Assert.Contains("CAR", capturedPrompt);
            Assert.Contains("DOG", capturedPrompt);
        }

        /// <summary>
        /// Verifies that mastered signs are included in the prompt.
        /// </summary>
        [Fact]
        public async Task GenerateChallenge_WithMasteredSigns_IncludesThemInPrompt()
        {
            // Arrange
            var request = new ChallengeRequest
            {
                SkillLevel = 5,
                Streak = 0,
                MasteredSigns = new List<string> { "A", "B", "L" },
                UsedWords = new List<string>()
            };

            string capturedPrompt = "";

            _mockGroqService
                .Setup(s => s.SendPrompt(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((prompt, system) => capturedPrompt = prompt)
                .ReturnsAsync(@"{""targetWord"": ""BALL"", ""hint"": ""Round object"", ""difficulty"": ""easy""}");

            // Act
            await _challengeService.GenerateChallenge(request);

            // Assert
            Assert.Contains("A", capturedPrompt);
            Assert.Contains("B", capturedPrompt);
            Assert.Contains("L", capturedPrompt);
        }
    }
}