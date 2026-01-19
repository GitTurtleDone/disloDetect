using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using dislodetect_be.Controllers;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Amazon.S3;
using Microsoft.Extensions.Primitives;

namespace dislodetect_be.tests.ControllerTests
{
    public class UploadPhotoFileControllerTests
    {
        private readonly Mock<IUploadPhotoFileRequestHandler> _mockHandler;
        private readonly UploadPhotoFileController _controller;
        public UploadPhotoFileControllerTests()
        {
            _mockHandler = new Mock<IUploadPhotoFileRequestHandler>();
            _controller = new UploadPhotoFileController(_mockHandler.Object);
        }
        [Fact]
        public async Task Upload_NoContentRequest_ReturnsBadRequest()
        {
            //Small change to test GitHub actions
            //Arrange
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.HasFormContentType).Returns(false);
            var context = new Mock<HttpContext>();  
            context.Setup(c => c.Request).Returns(request.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context.Object
            };
            //Act
            var result = await _controller.Upload();
            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unsupported content type.", badRequestResult.Value);
        }
        [Fact]
        public async Task Upload_EmptyFile_ReturnsBadRequest()
        {
            //Arrange
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.HasFormContentType).Returns(true);
            var formFileCollection = new FormFileCollection()
                {
                    new FormFile(Stream.Null, 0, 0, "file", "empty.jpg") // Empty file
                };
    
            var formCollection = new FormCollection(new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), formFileCollection);
            request.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);
            
            var context = new Mock<HttpContext>();  
            context.Setup(c => c.Request).Returns(request.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context.Object
            };
            //Act
            var result = await _controller.Upload();
            //Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("File is empty or missing.", badRequestResult.Value);
        }
        [Fact]
        public async Task Upload_ValidFile_FalseUsePhotoAllowed_ReturnsOk()
        {
            //Arrange
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.HasFormContentType).Returns(true);
            var fileContent = "This is a test file";
            var fileName = "test.jpg";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            var formFileCollection = new FormFileCollection()
                {
                    new FormFile(stream, 0, stream.Length, "file", fileName)
                };
    
            var formCollection = new FormCollection(
                new System.Collections.Generic.Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
                {
                    { "usePhotoAllowed", "false" }
                }, 
                formFileCollection);
            request.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);
            
            var context = new Mock<HttpContext>();  
            context.Setup(c => c.Request).Returns(request.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context.Object
            };
            _mockHandler.Setup(h => h.ClearSessionFolderAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockHandler.Setup(h => h.SaveImageToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockHandler.Setup(h => h.DisloDetectBucket).Returns("test-bucket");
            //Act
            var result = await _controller.Upload();
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseMessage = okResult.Value as dynamic;
            var fileNameResponse = responseMessage.fileName as string;
            var sessionId = responseMessage.sessionId as string;
            var photoUrl = responseMessage.photoUrl as string;

            Assert.NotNull(fileNameResponse);
            Assert.NotNull(sessionId);
            Assert.Contains(fileName, fileNameResponse);
            Assert.Contains("test-bucket", photoUrl);
        }
        
        [Fact]
        public async Task Upload_ValidFile_TrueUsePhotoAllowed_ReturnsOk()
        {
            //Arrange

            var request = new Mock<HttpRequest>();
            request.Setup(r => r.HasFormContentType).Returns(true);
            var fileName = "test.jpg";
            var fileContent = "This is a test file";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            var formFile = new FormFile(stream, 0, stream.Length, "file", fileName);
               
            var formCollection = new FormCollection
            (
                new Dictionary<string, StringValues>
                {
                    { "usePhotoAllowed", "true" }
                },
                new FormFileCollection { formFile }
                
            );

            request.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(formCollection);
            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request).Returns(request.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context.Object
            };
            _mockHandler.Setup(h => h.SaveImageToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>() )).ReturnsAsync(true);
            _mockHandler.Setup(h => h.SaveForTrainingImageToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockHandler.Setup(h => h.DisloDetectBucket).Returns("test-bucket");

            // Act
            var result = await _controller.Upload();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockHandler.Verify(h => h.SaveForTrainingImageToS3Async(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Once);
        }
    }
}