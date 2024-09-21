using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
// using excel_upload_be.Models;
// using excel_upload_be.Services;
using System.Text;
namespace dislodetect_be.Controllers;


[ApiController]
[Route("[controller]")] 


public class UploadPhotoFileController : ControllerBase
{
    
    // private readonly IFolderTreeService _folderTreeService;
    // private readonly ExcelUploadContext _DBContext;
    // public UploadZipFileController(IFolderTreeService folderTreeService, ExcelUploadContext dbContext)
    // {
    //     _folderTreeService = folderTreeService;
    //     _DBContext = dbContext;
    // }
    
    [HttpPost(Name = "PostUploadPhotoFile")]
    
    public async Task<IActionResult> Upload()
    {
        string publicFolderPath = @"../Public/SavedImages";
        string publicTrainingFolderPath = @"../Public/ForTrainingImages/Store";
        
        if (!Directory.Exists(publicFolderPath) )
        {
            Directory.CreateDirectory(publicFolderPath); 
        };

        if (!Directory.Exists(publicTrainingFolderPath) )
        {
            Directory.CreateDirectory(publicTrainingFolderPath);  
        };

        int imageCounter = Directory.GetFiles(publicTrainingFolderPath).Length;
        
        try
        {
            
            
            foreach (string fileName in Directory.GetFiles(publicFolderPath))
            {
                try
                {
                    System.IO.File.Delete(fileName);
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Error deleting file {fileName}: {e.Message}");
                }
            }
            var formCollection = await Request.ReadFormAsync();
            
            var file = formCollection.Files[0];
            
            if (file.Length > 0)
            {
                
                var fileName = file.FileName;

                var filePath = Path.Combine( publicFolderPath, fileName);
                Console.WriteLine("File Path: " + filePath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                Console.WriteLine($"Copied photo file path: {filePath}");
                var usePhotoAllowed = formCollection["usePhotoAllowed"];  
                Console.WriteLine("Use Photo Allowed: " + usePhotoAllowed);
                if (usePhotoAllowed == "true")
                {
                    
                    string saveFileName = imageCounter.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")+ "_" + fileName;
                    var fileTrainingPath = Path.Combine( publicTrainingFolderPath, saveFileName);
                    using (var stream = new FileStream(fileTrainingPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    // string formattedCurrentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    Console.WriteLine($"Copied training photo file path: {fileTrainingPath}");
                    // Console.WriteLine($"at: {formattedCurrentTime}");
                    return Ok("Photo uploaded and will be used for training");
                }
                else
                {
                    return Ok("Photo uploaded");
                }
                
            }
            else
            {
                return BadRequest("File is empty.");
            }
            
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

