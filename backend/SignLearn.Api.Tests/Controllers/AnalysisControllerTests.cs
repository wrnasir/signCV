using Microsoft.AspNetCore.Mvc;
using Moq;
using SignLearn.Api.Controllers;
using SignLearn.Api.DTOs;
using SignLearn.Api.Services;
using Xunit;

namespace SignLearn.Api.Tests.Controllers
{
    public class AnalysisControllerTests
    {
        private readonly Mock<IAnalysisService> _mockService;
        private readonly AnalysisController _controller;

        public AnalysisControllerTests()
        {
            _mockService = new Mock<IAnalysisService>();
            _controller = new AnalysisController(_mockService.Object);
        }

        /// <summary>
        /// Verifies that a valid request returns 200 OK
        /// with the analysis result.
        /// </summary>
        [Fact]
        public void Recognize_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[63]
            };

            var expectedResponse = new AnalysisResponse
            {
                RecognizedSign = "A",
                Confidence = 0.95f
            };

            _mockService
                .Setup(s => s.Analyze(It.IsAny<AnalysisRequest>()))
                .Returns(expectedResponse);

            // Act
            var result = _controller.Analysis(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AnalysisResponse>(okResult.Value);
            Assert.Equal("A", response.RecognizedSign);
            Assert.Equal(0.95f, response.Confidence);
        }

        /// <summary>
        /// Verifies that when the service throws an ArgumentException
        /// (e.g., wrong landmark count), the controller returns 500.
        /// </summary>
        [Fact]
        public void Recognize_ServiceThrows_Returns500()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[10]
            };

            _mockService
                .Setup(s => s.Analyze(It.IsAny<AnalysisRequest>()))
                .Throws(new ArgumentException("Expected 63 landmarks, received 10"));

            // Act
            var result = _controller.Analysis(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        /// <summary>
        /// Verifies that the controller calls the service exactly once.
        /// </summary>
        [Fact]
        public void Recognize_ValidRequest_CallsServiceOnce()
        {
            // Arrange
            var request = new AnalysisRequest
            {
                Landmarks = new float[63]
            };

            _mockService
                .Setup(s => s.Analyze(It.IsAny<AnalysisRequest>()))
                .Returns(new AnalysisResponse
                {
                    RecognizedSign = "B",
                    Confidence = 0.99f
                });

            // Act
            _controller.Analysis(request);

            // Assert
            _mockService.Verify(s => s.Analyze(It.IsAny<AnalysisRequest>()), Times.Once);
        }
    }
}