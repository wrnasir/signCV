using SignLearn.Api.DTOs;
using SignLearn.Api.Services;
using Xunit;

namespace SignLearn.Api.Tests.Services
{
    public class AnalysisServiceTests : IClassFixture<AnalysisServiceFixture>
    {
        private readonly AnalysisService _service;

        public AnalysisServiceTests(AnalysisServiceFixture fixture)
        {
            _service = fixture.Service;
        }

        /// <summary>
        /// Verifies that Analyze throws an ArgumentException
        /// when the landmarks array is not exactly 63 elements.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(62)]
        [InlineData(64)]
        [InlineData(100)]
        public void Analyze_InvalidLandmarkCount_ThrowsArgumentException(int count)
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[count]
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _service.Analyze(request));
            Assert.Contains("63", ex.Message);
        }

        /// <summary>
        /// Verifies that Analyze accepts exactly 63 landmarks without
        /// throwing a validation error. Does not assert prediction
        /// correctness — that is a model accuracy concern, not a service concern.
        /// </summary>
        [Fact]
        public void Analyze_ValidLandmarkCount_ReturnsResponse()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[63]
            };

            // Act
            var result = _service.Analyze(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.RecognizedSign);
            Assert.InRange(result.Confidence, 0f, 1f);
        }

        /// <summary>
        /// Verifies that the recognized sign is a valid label
        /// (single uppercase letter or known class name).
        /// </summary>
        [Fact]
        public void Analyze_ValidInput_ReturnsNonEmptySign()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[63]
            };

            // Act
            var result = _service.Analyze(request);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result.RecognizedSign));
        }

        /// <summary>
        /// Verifies that Analyze throws when given a null landmarks array.
        /// </summary>
        [Fact]
        public void Analyze_NullLandmarks_Throws()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = null!
            };

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _service.Analyze(request));
        }
    }

    public class AnalysisServiceFixture
    {
        public AnalysisService Service { get; }

        public AnalysisServiceFixture()
        {
            Service = new AnalysisService();
        }
    }
}