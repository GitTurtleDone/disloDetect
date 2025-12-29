using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 
using dislodetect_be.Controllers;
using Amazon.S3;

namespace dislodetect_be.tests.HandlerTests
{
    public class PredictHandlerTests
    {
        private readonly PredictRequestHandler _handler;
        private readonly Mock<IAmazonS3> _mockS3Client;

        public PredictHandlerTests()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _handler = new PredictRequestHandler(_mockS3Client.Object);
        }

        [Fact]
        public async Task GetImageFromS3Async_ValidUrl_ReturnsBase64String()
        {
            // Arrange
            string testUrl = "https://s3.amazonaws.com/bucketname/testimage.jpg";
            string expectedBase64 = "dGVzdGltYWdlYmFzZTY0"; // base64 for "testimagebase64"

            // Mock S3 client behavior
            _mockS3Client.Setup(s3 => s3.GetObjectAsync(It.IsAny<Amazon.S3.Model.GetObjectRequest>(), default))
                         .ReturnsAsync(new Amazon.S3.Model.GetObjectResponse
                         {
                             ResponseStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("testimagebase64"))
                         });

            // Act
            var (base64Image, message) = await _handler.GetImageFromS3Async(testUrl);

            // Assert
            Assert.Equal(expectedBase64, base64Image);
            Assert.Equal("Image downloaded from S3", message);
        }

        [Fact]
        public async Task GetImageFromS3Async_S3Exception_ReturnsErrorMessage()
        {
            // Arrange
            string testUrl = "https://s3.amazonaws.com/bucketname/testimage.jpg";
            _mockS3Client.Setup(s3 => s3.GetObjectAsync(It.IsAny<Amazon.S3.Model.GetObjectRequest>(), default))
                         .ThrowsAsync(new System.Exception("S3 error"));

            // Act
            var (base64Image, message) = await _handler.GetImageFromS3Async(testUrl);

            // Assert
            Assert.Null(base64Image);
            Assert.Equal("Error downloading from S3: S3 error", message);
        }
        [Fact]
        public void SetConfidenceAndOverlap_ReturnsCorrectValues()
        {
            //Arange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {   
                ["confidence"] = "0.6",
                ["overlap"] = "0.8"
            });
            _handler.SetConfidenceAndOverlap(formCollection);
            Assert.Equal("60.0", _handler.Confidence);
            Assert.Equal("80.0", _handler.Overlap);
        }
        
    }
}