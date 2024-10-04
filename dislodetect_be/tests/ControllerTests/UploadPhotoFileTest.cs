using Xunit;
using dislodetect_be.Controllers;
using Microsoft.AspNetCore.Mvc;
 using Moq;
using System.Text;

public class UploadPhotoFileControllerTests
{
    private Mock<IFormFile> _mockPhotoFile;
    private Mock<IFormCollection> _mockFormCollection;
    private Mock<HttpRequest> _mockHttpRequest;
    private Mock<HttpContext> _mockHttpContext;
    private UploadPhotoFileController _mockUploadPhotoFileController;

    

    public UploadPhotoFileControllerTests ()
    {
        _mockPhotoFile = new Mock<IFormFile>();
        _mockFormCollection = new Mock<IFormCollection>();
        _mockHttpRequest = new Mock<HttpRequest>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockUploadPhotoFileController = new UploadPhotoFileController();
        

    }
    private void SetUploadPhotoFile (string photoFileName, string? mockPhotoFileContent, bool usePhotoAllowed)
    {
        var photoContent = Array.Empty<byte>();
        if (mockPhotoFileContent !=null) // && mockPhotoFileContent != ""
        {
            photoContent = Encoding.UTF8.GetBytes(mockPhotoFileContent);
        };
       
        var stream = new MemoryStream(photoContent);
        _mockPhotoFile.Setup(f => f.FileName).Returns(photoFileName);
        _mockPhotoFile.Setup(f => f.Length).Returns(stream.Length); 
        _mockPhotoFile.Setup(f => f.ContentType).Returns("image/jpeg");
        _mockPhotoFile.Setup(f => f.OpenReadStream()).Returns(stream);
        _mockPhotoFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
        .Callback<Stream, CancellationToken>((s,t) => stream.CopyToAsync(s,t))
        .Returns(Task.CompletedTask);
        var fileCollection = new FormFileCollection {_mockPhotoFile.Object}; 
        _mockFormCollection.Setup(c => c.Files).Returns(fileCollection);

        
        _mockFormCollection.Setup(c => c["usePhotoAllowed"]).Returns(usePhotoAllowed.ToString().ToLower());
        _mockHttpRequest.Setup (r => r.ReadFormAsync(default)).ReturnsAsync(_mockFormCollection.Object);
        _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        _mockUploadPhotoFileController.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }
    
    
    [Fact]
    public async Task ReturnOK_FileNotEmpty_UsePhotoAllowed()
    {
        //Arrange
        SetUploadPhotoFile("test.jpg", "fake photo content", true);
        //Act
        var result = await _mockUploadPhotoFileController.Upload();
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Photo uploaded and will be used for training", okResult.Value);

    }

    [Fact]
    public async Task ReturnOK_FileNotEmpty_UsePhotoNotAllowed()
    {
        SetUploadPhotoFile("test.jpg", "fake photo content", false);
        var result = await _mockUploadPhotoFileController.Upload();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Photo uploaded", okResult.Value);
    }

    [Fact]
    public async Task BadRequest_FileEmptly()
    {
        SetUploadPhotoFile("test.jpg", null, true);
        var result = await _mockUploadPhotoFileController.Upload();
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File is empty.", badResult.Value);
    }
    

}