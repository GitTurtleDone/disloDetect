using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Amazon.S3;
using Amazon.S3.Model;
namespace dislodetect_be.Controllers;
using System.Data.SqlTypes;
using System.Web;

[ApiController]
[Route("[controller]")]
public class PredictController : ControllerBase
{
    private IPredictRequestHandler _predictRequestHandler;
    public PredictController(IPredictRequestHandler predictRequestHandler)
    {
        _predictRequestHandler = predictRequestHandler;
    }
    
    
    [HttpPost (Name = "PostPredict")]
    public async Task<IActionResult> Predict()
    {
        try
        {   
            // Add CORS headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            var formCollection = await Request.ReadFormAsync();
            string photoUrl = formCollection["photoUrl"];
            if (string.IsNullOrEmpty(photoUrl))
                return BadRequest("photoUrl is required");

            
            // Get image from S3
            var (base64Data, message) = await _predictRequestHandler.GetImageFromS3Async(photoUrl);
            if (base64Data == null)
            {
                return BadRequest(message);
            }
            
            // Build request and send to Roboflow using WebRequest
            string? requestURL = _predictRequestHandler.BuildRequestString(formCollection).RequestURL;
            Console.WriteLine($"Request URL: {requestURL}");
            Console.WriteLine($"Base64 length: {base64Data.Length}");
            Console.WriteLine($"First 50 chars of base64: {base64Data.Substring(0, Math.Min(50, base64Data.Length))}");
            
            // Convert base64 string to byte array (as ASCII, not UTF-8)
            var data = Encoding.ASCII.GetBytes(base64Data);
            
            // Service Request Config
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            // Configure Request
            WebRequest request = WebRequest.Create(requestURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            
            // Write Data
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            
            // Get Response
            string responseContent = null;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        responseContent = sr.ReadToEnd();
                    }
                }
            }
            
            Console.WriteLine(responseContent);
            return Ok(responseContent);
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"General exception: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public interface IPredictRequestHandler
{
    string? Confidence {get; set;}
    string? Overlap {get; set;}
    Task<(string? Base64Data, string? Message)> GetImageFromS3Async(string photoUrl);
    
    void SetConfidenceAndOverlap(IFormCollection formCollection);
    (string? RequestURL, string Message) BuildRequestString(IFormCollection formCollection);
    WebRequest? Create(string? requestURL);
    WebResponse? GetResponse(WebRequest? request);
    Stream? GetResponseStream(WebResponse response);
    string? GetResponseContent(Stream? responseStream);
}

public class PredictRequestHandler: IPredictRequestHandler
{
    private readonly IAmazonS3 _s3Client;
    //public string SavedImageFolderPath {get;} = "/tmp/SavedImages";
    public string DisloDetectBucket {get;} = Environment.GetEnvironmentVariable("DISLODETECT_BUCKET") ?? "dislodetect-1766191540";
    public string? FileName {get;set;}
    public string? Confidence {get; set;}
    public string? Overlap {get; set; }
    
    public PredictRequestHandler(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }
    public async Task<(string? Base64Data, string? Message)> GetImageFromS3Async(string photoUrl)
    {
        try
        {
            // Extract S3 key from URL
            var uri = new Uri(photoUrl);
            var key = uri.AbsolutePath.TrimStart('/');
            
            Console.WriteLine($"Downloading from S3 - Bucket: {DisloDetectBucket}, Key: {key}");
            
            var request = new GetObjectRequest
            {
                BucketName = DisloDetectBucket,
                Key = key
            };
            
            using var response = await _s3Client.GetObjectAsync(request);
            
            // Read the entire stream into a MemoryStream first
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            
            Console.WriteLine($"Downloaded {imageBytes.Length} bytes from S3");
            Console.WriteLine($"First 10 bytes: {string.Join(",", imageBytes.Take(10))}");
            
            // Convert to base64 (no line breaks like CLI)
            var base64Image = Convert.ToBase64String(imageBytes, Base64FormattingOptions.None);
            
            Console.WriteLine($"Base64 first 50 chars: {base64Image.Substring(0, Math.Min(50, base64Image.Length))}");
            
            // Set filename for Roboflow
            FileName = Path.GetFileName(key);
            
            return (base64Image, "Image downloaded from S3");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"S3 download error: {ex.Message}");
            return (null, $"Error downloading from S3: {ex.Message}");
        }
    }

    public (string? ApiKey, string? DatasetName, string? DatasetVersion) GetCredentials()
    {
        var apiKey = Environment.GetEnvironmentVariable("ROBOFLOW_API_KEY");
        var datasetName = Environment.GetEnvironmentVariable("ROBOFLOW_DATASET_NAME");
        var datasetVersion = Environment.GetEnvironmentVariable("ROBOFLOW_DATASET_VERSION");
        
        return (apiKey, datasetName, datasetVersion);
    }

    public void SetConfidenceAndOverlap(IFormCollection formCollection)
    {
        var confidence = float.Parse(formCollection["confidence"])*100; 
        var overlap = float.Parse(formCollection["overlap"])*100;
        Confidence = confidence.ToString();
        Overlap = overlap.ToString();
    }
    
    

    public (string? RequestURL, string Message) BuildRequestString(IFormCollection formCollection)
    {
        SetConfidenceAndOverlap(formCollection);
        string? requestURL = null;
        string message = "Building the request string failed";
        try 
        {
            var (apiKey, datasetName, datasetVersion) = GetCredentials();
            Console.WriteLine($"Building request with FileName: '{FileName}'");
            
            requestURL =
                    "https://detect.roboflow.com/" +
                    datasetName + "/" + datasetVersion +
                    "?api_key=" + apiKey +
                    "&confidence=" + Confidence[..Math.Min(4, Confidence.Length)] +
                    "&overlap=" + Overlap[..Math.Min(4, Overlap.Length)];
            message = "Request URL built successfully";
        } catch (Exception ex)
        {
            message = $"Exception occured when building request: {ex.Message}";
            Console.WriteLine(message);
        }
        return (requestURL, message);
    }
    public WebRequest? Create(string? requestURL)
    {
        var request = WebRequest.Create(requestURL);
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        return request;
    }

    
    public WebResponse? GetResponse(WebRequest? request)
    {
        return request.GetResponse();
    }

    public Stream? GetResponseStream(WebResponse response)
    {
        return response.GetResponseStream();
        
    }

    public string? GetResponseContent(Stream? responseStream)
    {
            Console.WriteLine("went in Get Response Content");       
            using StreamReader sr1 = new(responseStream);
            return sr1.ReadToEnd();
    }
    
    
}

/*
    public (Stream? PredictPostStream, WebRequest? PredictPostRequest, string? Message) PostRequest(WebRequest? request, byte[]? data)
    {
        Stream? stream = null;
        string? message = "Error occured when posting a request: ";
        try
        {
            //byte[]? data = GetImage().ImageData;
            // string? uploadURL = BuildRequestString(formCollection).RequestURL;
            // request = WebRequest.Create(uploadURL);
            
            // request.Method = "POST";
            // request.ContentType = "application/x-www-form-urlencoded";
            // request.ContentLength = data.Length;
            // stream = request.GetRequestStream();
            // stream.Write(data, 0, data.Length);
            //request = CreateRequest(formCollection).Request;
            string? _message = null;
            (stream, request, _message) = GetRequestStream(request, data);
            Console.WriteLine($"stream: {stream}");
            message = "Request posted successfully";
        } catch (Exception ex)
        {
            message += ex.Message;
            Console.WriteLine(message);
        }
        return (stream, request, message);
    }
    */

    // public (string? ResponseContent, string? Message) GetResponse(WebRequest request)
    // {   
    //     string responseContent = null;
    //     string message = "Error when trying IRequestHandler GetResponse: ";
    //     try
    //     {
    //         using (WebResponse response = request.GetResponse())
    //         {
    //             using Stream stream = response.GetResponseStream();
    //             using StreamReader sr = new(stream);
    //             responseContent = sr.ReadToEnd();
    //         }
    //         message = "Successfully get response";
            
    //     } catch (Exception ex)
    //     {
    //         message += ex.Message;
    //         Console.WriteLine(message);
    //     }
    //     return (responseContent, message);        
    // }