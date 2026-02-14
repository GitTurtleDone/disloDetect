using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
// using System.Linq;

namespace dislodetect_be.Controllers;

public class UploadPhotoFileFunction
{
    private readonly IUploadPhotoFileRequestHandler _uploadPhotoFileRequestHandler;
    
    public UploadPhotoFileFunction(IUploadPhotoFileRequestHandler uploadPhotoFileRequestHandler)
    {
        _uploadPhotoFileRequestHandler = uploadPhotoFileRequestHandler;
    }
    
    [Function("UploadPhotoFile")]
    public async Task<IActionResult> Upload(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "UploadPhotoFile")] HttpRequest Request
    )
    {
        try
        {
            if (!Request.HasFormContentType)
                return new BadRequestObjectResult("Unsupported content type.");
            var formCollection = await Request.ReadFormAsync();
            var file = formCollection.Files.GetFile("file");
            
            if (file == null || file.Length == 0)
                return new BadRequestObjectResult("File is empty or missing.");

            // Use existing sessionId or generate new one
            string sessionId = string.IsNullOrEmpty(formCollection["sessionId"]) 
                ? Guid.NewGuid().ToString("N")[..12] 
                : formCollection["sessionId"].ToString();
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.FileName;
            
            // Clear existing images in session folder before uploading new one
            await _uploadPhotoFileRequestHandler.ClearSessionFolderAsync(sessionId);
            
            bool uploaded = await _uploadPhotoFileRequestHandler.SaveImageToBlobAsync(file, fileName, sessionId);
            
            var useForTraining = formCollection["usePhotoAllowed"] == "true";
            if (useForTraining)
            {
                uploaded = await _uploadPhotoFileRequestHandler.SaveForTrainingImageToBlobAsync(file, fileName);
            }
            
            return uploaded ? new OkObjectResult(new { 
                sessionId, 
                fileName, 
                photoUrl = $"https://{_uploadPhotoFileRequestHandler.StorageAccountName}.blob.core.windows.net/{_uploadPhotoFileRequestHandler.ContainerName}/Public/SavedImages/Sessions/{sessionId}/{fileName}"
            }) : new ObjectResult("Upload failed"){ StatusCode = 500 };
        }
        catch (Exception ex)
        {
            return new ObjectResult($"Upload error: {ex.Message}") { StatusCode = 500 };
        }
    }
    
    [Function("DeleteSession")]
    public async Task<IActionResult> DeleteSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "session/{sessionId}")] HttpRequest Request,
        string sessionId
    )
    {
        try
        {
            await _uploadPhotoFileRequestHandler.DeleteSessionAsync(sessionId);
            return new OkObjectResult("Session deleted");
        }
        catch (Exception ex)
        {
            return new ObjectResult($"Delete error: {ex.Message}") { StatusCode = 500 };
        }
    }
}

public interface IUploadPhotoFileRequestHandler
{   
    string StorageAccountName { get; }
    string ContainerName { get; }
    Task<bool> SaveForTrainingImageToBlobAsync(IFormFile file, string fileName);
    Task<bool> SaveImageToBlobAsync(IFormFile file, string fileName, string sessionId);
    Task ClearSessionFolderAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
}

public class UploadPhotoFileRequestHandler : IUploadPhotoFileRequestHandler
{
    private readonly BlobContainerClient _containerClient;
    public string StorageAccountName { get; }
    public string ContainerName { get; }
    
    public UploadPhotoFileRequestHandler(BlobServiceClient blobServiceClient)
    {
        ContainerName = Environment.GetEnvironmentVariable("AZ_BLOB_CONTAINER") ?? "dislodetect";
        StorageAccountName = blobServiceClient.AccountName;
        _containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
    }
    
    public async Task<bool> SaveForTrainingImageToBlobAsync(IFormFile file, string fileName)
    {
        return await UploadToBlob(file, $"Public/ForTrainingImages/Store/{fileName}");
    }
    
    public async Task<bool> SaveImageToBlobAsync(IFormFile file, string fileName, string sessionId)
    {
        return await UploadToBlob(file, $"Public/SavedImages/Sessions/{sessionId}/{fileName}");
    }
    
    public async Task ClearSessionFolderAsync(string sessionId)
    {
        try
        {
            var prefix = $"Public/SavedImages/Sessions/{sessionId}/";
            
            await foreach (var blobItem in _containerClient.GetBlobsAsync(prefix: prefix))
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                await blobClient.DeleteIfExistsAsync();
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
    
    private async Task<bool> UploadToBlob(IFormFile file, string blobName)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Blob upload error: {ex.Message}");
            return false;
        }
    }
}
