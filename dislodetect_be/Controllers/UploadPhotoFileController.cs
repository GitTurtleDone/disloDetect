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
        string publicTrainingFolderPath = @"../Public/ForTrainingImages";
        // FolderNode folderTree;
        // Console.WriteLine("Went in here");
        try
        {
            
            var formCollection = await Request.ReadFormAsync();
            
            var file = formCollection.Files[0];

            // Console.WriteLine(f"file: {file}", );
            
            
            if (file.Length > 0)
            {
                // Console.WriteLine("Went in if");
                var fileName = file.FileName;//.Trim('"')
                
                // var fileExtension = Path.GetExtension(fileName);

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
                    var fileTrainingPath = Path.Combine( publicTrainingFolderPath, fileName);
                    using (var stream = new FileStream(fileTrainingPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    Console.WriteLine($"Copied training photo file path: {fileTrainingPath}");
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

