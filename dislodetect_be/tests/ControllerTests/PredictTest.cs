using Moq;
using Xunit;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using dislodetect_be.Controllers;
using System.Net;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http.Features;
using System.Data.SqlTypes;
using System.Text;
public class PredictControllerTests
{
    private Mock<IFormFile> _mockFormFile;
    private Mock<IFormFileCollection> _mockFileCollection;
    private Mock<HttpRequest> _mockHttpRequest;
    private Mock<HttpContext> _mockHttpContext;
    private Mock<IFormCollection> _mockFormCollection;
    private Mock<IPredictRequestHandler> _mockPredictRequestHandler;
    private PredictController _mockPredictController;
    private PredictRequestHandler _predictHandler;
    private byte[]? _mockImage;
    public PredictControllerTests()
    {
        _mockFormFile = new Mock<IFormFile>();
        _mockFormCollection = new Mock<IFormCollection>();
        _mockPredictRequestHandler = new Mock<IPredictRequestHandler>();
        _predictHandler = new PredictRequestHandler();
        _mockPredictController = new PredictController(_mockPredictRequestHandler.Object);
        _mockImage = Array.Empty<byte>();
    }
    private void SetUpTests(string? savedImageFolderPath, string? credentialFilePath, string? mockImageFilePath, string[]? credentials, string? confidence, string? overlap)
    {
        // set up tests for helping methods in the PredictRequestHandler Class 
        _mockImage = Encoding.ASCII.GetBytes(Convert.ToBase64String(File.ReadAllBytes(mockImageFilePath)));
        foreach (string fileName in Directory.GetFiles(_predictHandler.SavedImageFolderPath))
        {
            File.Delete(fileName);
        }
        File.Copy(mockImageFilePath, _predictHandler.SavedImageFolderPath + "/test.jpg");
        File.WriteAllLines(credentialFilePath, credentials);
        var stream = new MemoryStream(_mockImage);
        var fileCollection = new FormFileCollection {_mockFormFile.Object};
        _mockFormCollection.Setup(c => c.Files).Returns(fileCollection);
        _mockFormCollection.Setup(c => c["confidence"]).Returns(confidence);
        _mockFormCollection.Setup(c => c["overlap"]).Returns(overlap); 
        _predictHandler.SetConfidenceAndOverlap(_mockFormCollection.Object);
        
        // set up tests for the IPredictRequestHandler interface
        // _mockPredictRequestHandler.Setup(h => h.SavedImageFolderPath).Returns(savedImageFolderPath);
        // _mockPredictRequestHandler.Setup(h => h.CredentialFilePath).Returns(credentialFilePath);
        // _mockHttpRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(_mockFormCollection.Object);
        // _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        // _mockPredictController.ControllerContext = new ControllerContext
        // {
        //     HttpContext = _mockHttpContext.Object
        // };
        // _mockPredictRequestHandler.Setup(h => h.GetImage().ImageData).Returns(_mockImage);
        // string mockURL = "https://someURL.com";
        // _mockPredictRequestHandler.Setup(h => h.BuildRequestString(_mockFormCollection.Object).RequestURL).Returns(mockURL);

        // _mockFormFile.Setup(f => f.FileName).Returns("Ref03_Fig4b_Rot45.jpg");
        // _mockFormFile.Setup(f => f.Length).Returns(_mockImage.Length);
        // _mockFormFile.Setup(f => f.ContentType).Returns("image/jpeg");
        // _mockFormFile.Setup(f => f.OpenReadStream()).Returns(stream);
        
        
        // _mockHttpRequest.Setup(r => r.ReadFormAsync(default)).ReturnsAsync(_mockFormCollection.Object);
        // _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        // _controller.ControllerContext = new ControllerContext
        // {
        //     HttpContext = _mockHttpContext.Object
        // };

        // _mockPredictRequestHandler.Setup(h => h.Confidence).Returns(confidence);
        // _mockPredictRequestHandler.Setup(h => h.Overlap).Returns(overlap);


    }
    [Fact]
    public async Task ReturnCorerct_SavedImageFolderPath()
    {
        Assert.Equal("../Public/SavedImages", _predictHandler.SavedImageFolderPath);
    }
    [Fact]
    public async Task ReturnCorrect_CredentialFilePath()
    {
        Assert.Equal("../Roboflow.txt", _predictHandler.CredentialFilePath);
    }
    
    [Fact]
    public async Task Test_GetImage()
    {
        SetUpTests(_predictHandler.SavedImageFolderPath,_predictHandler.CredentialFilePath, "../Ref06_Fig4e.jpg", ["first_line", "second_line", "third_line"], "0.5", "0.5");
        var result = _predictHandler.GetImage();
        Assert.Equal(_mockImage, result.ImageData);
        Assert.Equal("Image read.", result.Message);
        File.Delete(_predictHandler.SavedImageFolderPath+"/test.jpg");
        result = _predictHandler.GetImage();
        Assert.True(result.Message.Contains("An error occured when reading the saved image: "));
    }
    [Fact]
    public async Task Test_GetCredentials()
    {
        SetUpTests(_predictHandler.SavedImageFolderPath,_predictHandler.CredentialFilePath, "../Ref06_Fig4e.jpg", ["first_line", "second_line", "third_line"], "0.5", "0.5");
        var result = _predictHandler.GetCredentials();
        Assert.Equal("first_line", result.Credentials[0]);
        Assert.Equal("second_line", result.Credentials[1]);
        Assert.Equal("third_line", result.Credentials[2]);
        Assert.Equal("Credentials obtained successfully", result.Message);
        File.Delete(_predictHandler.CredentialFilePath);
        result = _predictHandler.GetCredentials();
        Assert.True(result.Message.Contains("An error occure while reading credentials: "));
    }
    [Fact]
    public async Task Test_SetConfidenceAndOverlap()
    {
        SetUpTests(_predictHandler.SavedImageFolderPath,_predictHandler.CredentialFilePath, "../Ref06_Fig4e.jpg", ["first_line", "second_line", "third_line"], "0.8", "0.9");
        Assert.Equal("0.8", _predictHandler.Confidence);
        Assert.Equal("0.9", _predictHandler.Overlap);
    }

    [Fact]
    public async Task Test_BuildRequestString()
    {
        SetUpTests(_predictHandler.SavedImageFolderPath,_predictHandler.CredentialFilePath, "../Ref06_Fig4e.jpg", ["myKey", "myDataset", "myVersion"], "0.256789", "0.7");
        var result = _predictHandler.BuildRequestString(_mockFormCollection.Object);
        string strExpected = "https://detect.roboflow.com/myDataset/myVersion?api_key=myKey&confidence=0.2567&overlap=0.7&name=";
        Assert.True(result.RequestURL.Contains(strExpected));
    }
    [Fact]
    public async Task Test_Predict()
    {

    }

}