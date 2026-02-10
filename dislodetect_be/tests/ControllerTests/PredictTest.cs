using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using dislodetect_be.Controllers;
// using System.Threading.Tasks;

namespace dislodetect_be.tests.ControllerTests
{
    public class PredictControllerTests
    {
        private readonly Mock<IPredictRequestHandler> _mockHandler;
        private readonly PredictController _controller;

        public PredictControllerTests()
        {
            _mockHandler = new Mock<IPredictRequestHandler>();
            _controller = new PredictController(_mockHandler.Object);
        }

        [Fact]
        public async Task Predict_WithValidPhotoUrl_ReturnsOk()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["photoUrl"] = "https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg",
                ["confidence"] = "0.5",
                ["overlap"] = "0.7"
            });

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);
            
            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            _mockHandler.Setup(h => h.GetImageFromBlobAsync("https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg"))
                       .ReturnsAsync(("base64data", "Success"));
            
            _mockHandler.Setup(h => h.BuildRequestString(It.IsAny<IFormCollection>()))
                       .Returns(("https://api.roboflow.com/test", "Success"));

            // Act
            var result = await _controller.Predict();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Predict_WithMissingPhotoUrl_ReturnsBadRequest()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);

            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            // Act
            var result = await _controller.Predict();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("photoUrl is required", badRequestResult.Value);
        }

        [Fact]
        public async Task Predict_WithBlobError_ReturnsBadRequest()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["photoUrl"] = "https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg"
            });

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);

            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            _mockHandler.Setup(h => h.GetImageFromBlobAsync("https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg"))
                       .ReturnsAsync((null, "Blob storage error"));

            // Act
            var result = await _controller.Predict();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Blob storage error", badRequestResult.Value);
        }

        [Fact]
        public async Task Predict_WithHandlerException_Returns500()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                ["photoUrl"] = "https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg"
            });

            var mockRequest = new Mock<HttpRequest>();
            mockRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);
            
            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            _mockHandler.Setup(h => h.GetImageFromBlobAsync("https://dislodetectstor.blob.core.windows.net/dislodetect/image.jpg"))
                    .ThrowsAsync(new Exception("Unexpected error"));
            
            // Act
            var result = await _controller.Predict();
            
            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal("Internal server error: Unexpected error", objectResult.Value);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
