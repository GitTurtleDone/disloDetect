using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; 
using dislodetect_be.Controllers;
using Amazon.S3;
using Amazon.S3.Model;
using System.Runtime.CompilerServices;

namespace dislodetect_be.tests.HandlerTests
{
    public class UploadPhotoFileRequestHandlerTest
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly UploadPhotoFileRequestHandler _requestHandler;
        public UploadPhotoFileRequestHandlerTest()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _requestHandler = new UploadPhotoFileRequestHandler(_mockS3Client.Object);
        }

        [Fact]
        public async Task SaveImageToS3Async_Success_ReturnsTrue ()
        {
            //Arrange
            var mockFile = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var sessionId = "XXX";
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default)).ReturnsAsync(new PutObjectResponse());

            //Act
            var result = await _requestHandler.SaveImageToS3Async(mockFile.Object, fileName, sessionId);

            //Assert
            Console.WriteLine(result);
            Assert.True(result);
        }

        [Fact]
        public async Task SaveImageToS3Async_Exception_ReturnsFalse ()
        {
            //Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<MemoryStream>(), default)).ThrowsAsync(new System.Exception("Copy to async error"));
            var fileName = "test.jpg";
            var sessionId = "XXX";
            // Act
            var result = await _requestHandler.SaveImageToS3Async(mockFile.Object, fileName, sessionId);
            // Assert
            Console.WriteLine(result);
            Assert.False(result);
            
        }

        [Fact]
        public async Task ClearSessionFolderAsync_Success_DeleteSome()
        {
            //Arrange
            var sessionId = "XXX";
            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ReturnsAsync(new ListObjectsV2Response
                         {
                             S3Objects = new List<S3Object>
                             {
                                 new S3Object { Key = "XXX/image1.jpg" },
                                 new S3Object { Key = "XXX/image2.jpg" }
                             }
                         });
            _mockS3Client.Setup(s3 => s3.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default))
                         .ReturnsAsync(new DeleteObjectsResponse());
            //Act
            await _requestHandler.ClearSessionFolderAsync(sessionId);
            //Assert
            _mockS3Client.Verify(s3 => s3.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Once);
        }
        [Fact]
        public async Task ClearSessionFolderAsync_Success_DeleteNone()
        {
            //Arrange
            var sessionId = "XXX";
            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ReturnsAsync(new ListObjectsV2Response
                         {
                             S3Objects = new List<S3Object>()
                         });
            //Act
            await _requestHandler.ClearSessionFolderAsync(sessionId);
            _mockS3Client.Verify(s3 => s3.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Never);
        }
        [Fact]
        public async Task ClearSessionFolderAsync_NoExecution()
        {
            //Arrange
            var sessionId = "XXX";
            _mockS3Client.Setup(s3 => s3.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default))
                         .ThrowsAsync(new System.Exception("List objects error"));
            //Act
            await _requestHandler.ClearSessionFolderAsync(sessionId);
            //Assert
            _mockS3Client.Verify(s3 => s3.DeleteObjectsAsync(It.IsAny<DeleteObjectsRequest>(), default), Times.Never);
        }
    }
}

   