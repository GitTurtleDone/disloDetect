using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using dislodetect_be.Controllers;
using System.Linq;
namespace dislodetect_be.Controllers;


[ApiController]
[Route("[controller]")] 


public class UploadPhotoFileController : ControllerBase
{
    private readonly IUploadPhotoFileRequestHandler _uploadPhotoFileRequestHandler;
    
    public UploadPhotoFileController(IUploadPhotoFileRequestHandler uploadPhotoFileRequestHandler)
    {
        _uploadPhotoFileRequestHandler = uploadPhotoFileRequestHandler;
    }
    
    [HttpPost(Name = "PostUploadPhotoFile")]
    public async Task<IActionResult> Upload()
    {
        try
        {
            if (!Request.HasFormContentType)
                return BadRequest("Unsupported content type.");
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files.GetFile("file");
            
            if (file == null ||file.Length == 0)
                return BadRequest("File is empty or missing.");

            // Use existing sessionId or generate new one
            string sessionId = string.IsNullOrEmpty(formCollection["sessionId"]) 
                ? Guid.NewGuid().ToString("N")[..12] 
                : formCollection["sessionId"].ToString();
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.FileName;
            
            // Clear existing images in session folder before uploading new one
            await _uploadPhotoFileRequestHandler.ClearSessionFolderAsync(sessionId);
            
            bool uploaded = await _uploadPhotoFileRequestHandler.SaveImageToS3Async(file, fileName, sessionId);
            
            var useForTraining = formCollection["usePhotoAllowed"] == "true";
            if (useForTraining)
            {
                await _uploadPhotoFileRequestHandler.SaveForTrainingImageToS3Async(file, fileName);
            }
            
            return uploaded ? Ok(new { 
                sessionId, 
                fileName, 
                photoUrl = $"https://{_uploadPhotoFileRequestHandler.DisloDetectBucket}.s3.ap-southeast-6.amazonaws.com/Public/SavedImages/Sessions/{sessionId}/{fileName}"
            }) : StatusCode(500, "Upload failed");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Upload error: {ex.Message}");
        }
    }
    
    [HttpDelete("session/{sessionId}")]
    public async Task<IActionResult> DeleteSession(string sessionId)
    {
        try
        {
            await _uploadPhotoFileRequestHandler.DeleteSessionAsync(sessionId);
            return Ok("Session deleted");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Delete error: {ex.Message}");
        }
    }

}
public interface IUploadPhotoFileRequestHandler
{   
    string DisloDetectBucket { get; }
    Task<bool> SaveForTrainingImageToS3Async(IFormFile file, string fileName);
    Task<bool> SaveImageToS3Async(IFormFile file, string fileName, string sessionId);
    Task ClearSessionFolderAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
}

public class UploadPhotoFileRequestHandler : IUploadPhotoFileRequestHandler
{
    private readonly IAmazonS3 _s3Client;
    public string DisloDetectBucket { get; } = Environment.GetEnvironmentVariable("DISLODETECT_BUCKET") ?? "dislodetect-1766191540";
    
    public UploadPhotoFileRequestHandler(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }
    
    public async Task<bool> SaveForTrainingImageToS3Async(IFormFile file, string fileName)
    {
        return await UploadToS3(file, $"Public/ForTrainingImages/Store/{fileName}");
    }
    
    public async Task<bool> SaveImageToS3Async(IFormFile file, string fileName, string sessionId)
    {
        return await UploadToS3(file, $"Public/SavedImages/Sessions/{sessionId}/{fileName}");
    }
    
    public async Task ClearSessionFolderAsync(string sessionId)
    {
        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = DisloDetectBucket,
                Prefix = $"Public/SavedImages/Sessions/{sessionId}/"
            };
            
            var response = await _s3Client.ListObjectsV2Async(listRequest);
            
            if (response.S3Objects.Count > 0)
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = DisloDetectBucket,
                    Objects = response.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }).ToList()
                };
                
                await _s3Client.DeleteObjectsAsync(deleteRequest);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing session folder: {ex.Message}");
        }
    }
    
    public async Task DeleteSessionAsync(string sessionId)
    {
        await ClearSessionFolderAsync(sessionId);
    }
    
    private async Task<bool> UploadToS3(IFormFile file, string key)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = DisloDetectBucket,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"S3 upload error: {ex.Message}");
            return false;
        }
    }
}

