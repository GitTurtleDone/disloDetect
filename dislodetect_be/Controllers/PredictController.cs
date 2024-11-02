using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Data.SqlTypes;
namespace dislodetect_be.Controllers;
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
        string? responseContent;
        try
        {   
            var formCollection = await Request.ReadFormAsync();
            string? requestURL = _predictRequestHandler.BuildRequestString(formCollection).RequestURL;
            var request = _predictRequestHandler.Create(requestURL);
            Console.WriteLine($"requestURL: {requestURL}");
            byte[]? data = _predictRequestHandler.GetImage().ImageData;
            // string? _message = null;
            var stream = _predictRequestHandler.GetRequestStream(request,data);
            
            var response = _predictRequestHandler.GetResponse(request);
            var responseStream = _predictRequestHandler.GetResponseStream(response);

            using StreamReader sr = new(responseStream);
            responseContent =  sr.ReadToEnd();
            //responseContent = _predictRequestHandler.GetResponseContent(responseStream);
            
                
            
            // string publicFolderPath = @"../Public/SavedImages";
            // string fileName = Directory.GetFiles(publicFolderPath)[0];
            // byte[] image = System.IO.File.ReadAllBytes(fileName);
            // string encoded = Convert.ToBase64String(image);
            // byte[] data = System.Text.Encoding.ASCII.GetBytes(encoded);
            // string[] lines = System.IO.File.ReadAllLines("../Roboflow.txt");
            // string API_KEY = lines[0];
            // string DATASET_NAME = lines[1];
            // string DATASET_VERSION = lines[2];
            // var formCollection = await Request.ReadFormAsync();
            // string confidence = formCollection["confidence"];
            // string overlap = formCollection["overlap"];
            
            // Console.WriteLine($"confidence: {confidence[..Math.Min(4, confidence.Length)]}");
            // Console.WriteLine($"overlap: {overlap[..Math.Min(4, overlap.Length)]}");
            
            
            // //Contruct the URL
            // string uploadURL =
            //         "https://detect.roboflow.com/" +
            //         DATASET_NAME + "/" + DATASET_VERSION +
            //         "?api_key=" + API_KEY +
            //         "&confidence=" + confidence[..Math.Min(4, confidence.Length)] +
            //         "&overlap=" + overlap[..Math.Min(4, overlap.Length)] +
            //         "&name=" + fileName;
            // // Service Request Config

            // Console.WriteLine($"predictURL: {uploadURL}");
            // ServicePointManager.Expect100Continue = true;
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            // //Configure Request
            // WebRequest request = WebRequest.Create(uploadURL);
            // request.Method = "POST";
            // request.ContentType = "application/x-www-form-urlencoded";
            // request.ContentLength = data.Length;

            // // Write Data
            // using (Stream stream = request.GetRequestStream())
            // {
            //     stream.Write(data, 0, data.Length);
            // }

            // // Get Response

            // string responseContent = null;
            // using (WebResponse response = request.GetResponse())
            // {
            //     using (Stream stream = response.GetResponseStream())
            //     {
            //         using (StreamReader sr99 = new StreamReader(stream))
            //         {
            //             responseContent = sr99.ReadToEnd();
            //         }
            //     }
            // }

            // // Console.WriteLine($"Image in the public folder: {fileName}");
            // // Console.WriteLine($"Response Content: {responseContent}");
            
            return Ok(responseContent);
        } 
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

    }
}

public interface IPredictRequestHandler
{
    string? Confidence {get; set;}
    string? Overlap {get; set;}
    string SavedImageFolderPath {get;}
    string CredentialFilePath {get;}
    (byte[]? ImageData, string? Message) GetImage();
    (string[]? Credentials, string Message) GetCredentials();
    void SetConfidenceAndOverlap(IFormCollection formCollection);
    (string? RequestURL, string Message) BuildRequestString(IFormCollection formCollection);
    WebRequest? Create(string? requestURL);
    Stream? GetRequestStream(WebRequest? request, byte[]? data);
    WebResponse? GetResponse(WebRequest? request);
    Stream? GetResponseStream(WebResponse response);
    string? GetResponseContent(Stream? responseStream);
}

public class PredictRequestHandler: IPredictRequestHandler
{
    public string SavedImageFolderPath {get;} = "../Public/SavedImages";
    public string CredentialFilePath {get;} = "../Public/Roboflow.txt";
    public string? FileName {get;set;}
    public string? Confidence {get; set;}
    public string? Overlap {get; set; }
    public (byte[]? ImageData, string? Message) GetImage()
    {
        byte[] imgData = null;
        string message = "Image read.";
        try
        {
            FileName = Directory.GetFiles(SavedImageFolderPath)[0];
            byte[]? imgFile = File.ReadAllBytes(FileName);
            string encoded = Convert.ToBase64String(imgFile);
            imgData = Encoding.ASCII.GetBytes(encoded);
        } catch (Exception ex)
        {
            message = $"An error occured when reading the saved image: {ex.Message}";
            Console.WriteLine(message);
        }
        return (imgData, message);
    }

    public (string[]? Credentials, string Message) GetCredentials()
    {
        string[] credentials = null;
        string message = "An error occure while reading credentials: ";
        try 
        {   
            credentials = File.ReadAllLines(CredentialFilePath);
            message = "Credentials obtained successfully";
        } catch (Exception ex)
        {
            message += ex.Message;
            Console.WriteLine(message);
        }

        return (credentials, message);
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
            
            string[] lines = GetCredentials().Credentials;
            string API_KEY = lines[0];
            string DATASET_NAME = lines[1];
            string DATASET_VERSION = lines[2];
            
            //Contruct the URL
            requestURL =
                    "https://detect.roboflow.com/" +
                    DATASET_NAME + "/" + DATASET_VERSION +
                    "?api_key=" + API_KEY +
                    "&confidence=" + Confidence[..Math.Min(4, Confidence.Length)] +
                    "&overlap=" + Overlap[..Math.Min(4, Overlap.Length)] +
                    "&name=" + FileName;
        } catch (Exception ex)
        {
            message = $"Exception occured when reading credentials: {ex.Message}";
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
    public Stream? GetRequestStream(WebRequest? request, byte[]? data)
    {    
        request.ContentLength = data.Length;
        Stream? stream = request.GetRequestStream();
        using (stream)
        {
            stream.Write(data, 0, data.Length);    
        }
        return stream;
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