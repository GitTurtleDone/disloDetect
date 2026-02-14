using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using System.IO;
using System.Net;
using System.Text;
using Azure.Storage.Blobs;
namespace dislodetect_be.Controllers;

public class PredictFunction
{
    private readonly IPredictRequestHandler _predictRequestHandler;

    public PredictFunction(IPredictRequestHandler predictRequestHandler)
    {
        _predictRequestHandler = predictRequestHandler;
    }

    [Function("Predict")]
    public async Task<IActionResult> Predict(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "predict")] HttpRequest req
    )
    {
        try
        {   
            var formCollection = await req.ReadFormAsync();
            string photoUrl = formCollection["photoUrl"];
            if (string.IsNullOrEmpty(photoUrl))
                return new BadRequestObjectResult("photoUrl is required");

            // Get image from Azure Blob Storage
            var (base64Data, message) = await _predictRequestHandler.GetImageFromBlobAsync(photoUrl);
            if (base64Data == null)
            {
                return new BadRequestObjectResult(message);
            }
            
            // Build request and send to Roboflow
            string? requestURL = _predictRequestHandler.BuildRequestString(formCollection).RequestURL;
            Console.WriteLine($"Request URL: {requestURL}");
            Console.WriteLine($"Base64 length: {base64Data.Length}");
            
            var data = Encoding.ASCII.GetBytes(base64Data);
            
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            WebRequest request = WebRequest.Create(requestURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            
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
            return new OkObjectResult(responseContent);
        } 
        catch (Exception ex)
        {
            return new ObjectResult($"Internal server error: {ex.Message}") { StatusCode = 500 };
        }
    }
}

public interface IPredictRequestHandler
{
    string? Confidence {get; set;}
    string? Overlap {get; set;}
    Task<(string? Base64Data, string? Message)> GetImageFromBlobAsync(string photoUrl);
    
    void SetConfidenceAndOverlap(IFormCollection formCollection);
    (string? RequestURL, string Message) BuildRequestString(IFormCollection formCollection);
    WebRequest? Create(string? requestURL);
    WebResponse? GetResponse(WebRequest? request);
    Stream? GetResponseStream(WebResponse response);
    string? GetResponseContent(Stream? responseStream);
}

public class PredictRequestHandler: IPredictRequestHandler
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    public string? FileName {get;set;}
    public string? Confidence {get; set;}
    public string? Overlap {get; set; }
    
    public PredictRequestHandler(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = Environment.GetEnvironmentVariable("AZ_BLOB_CONTAINER") ?? "dislodetect";
    }
    
    public async Task<(string? Base64Data, string? Message)> GetImageFromBlobAsync(string photoUrl)
    {
        try
        {
            // Extract blob path from URL
            // URL format: https://dislodetectstor.blob.core.windows.net/dislodetect/Public/SavedImages/Sessions/{sessionId}/{fileName}
            var uri = new Uri(photoUrl);
            var pathSegments = uri.AbsolutePath.TrimStart('/');
            // Remove container name from path to get the blob name
            var containerPrefix = _containerName + "/";
            var blobName = pathSegments.StartsWith(containerPrefix) 
                ? pathSegments.Substring(containerPrefix.Length) 
                : pathSegments;
            
            Console.WriteLine($"Downloading from Blob - Container: {_containerName}, Blob: {blobName}");
            
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            var response = await blobClient.DownloadContentAsync();
            var imageBytes = response.Value.Content.ToArray();
            
            Console.WriteLine($"Downloaded {imageBytes.Length} bytes from Blob Storage");
            
            var base64Image = Convert.ToBase64String(imageBytes, Base64FormattingOptions.None);
            
            return (base64Image, "Image downloaded from Blob Storage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Blob download error: {ex.Message}");
            return (null, $"Error downloading from Blob Storage: {ex.Message}");
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
        Confidence = confidence.ToString("F1");
        Overlap = overlap.ToString("F1");
    }

    public (string? RequestURL, string Message) BuildRequestString(IFormCollection formCollection)
    {
        SetConfidenceAndOverlap(formCollection);
        string? requestURL = null;
        string message = "Building the request string failed";
        try 
        {
            var (apiKey, datasetName, datasetVersion) = GetCredentials();
            
            requestURL =
                    "https://detect.roboflow.com/" +
                    datasetName + "/" + datasetVersion +
                    "?api_key=" + apiKey +
                    "&confidence=" + Confidence +
                    "&overlap=" + Overlap;
            message = "Request URL built successfully";
        } catch (Exception ex)
        {
            message = $"Exception occurred when building request: {ex.Message}";
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
        // Console.WriteLine("went in Get Response Content");       
        using StreamReader sr1 = new(responseStream);
        return sr1.ReadToEnd();
    }
}
