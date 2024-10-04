using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
        string? responseContent=null; //message = null
        try
        {   
            var formCollection = await Request.ReadFormAsync();
            
            var (stream, request, _) = _predictRequestHandler.PostRequest(formCollection);
            using (stream)
            {
                var response = _predictRequestHandler.GetResponse(request);
                responseContent = response.ResponseContent;
                //message = response.Message;
            }
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
    // (WebRequest? Request, string Message) CreateRequest();
    // (Stream? Stream, string Message) GetRequestStream();
    (Stream? PredictPostStream, WebRequest? PredictPostRequest, string? Message) PostRequest(IFormCollection formCollection);
    (string? ResponseContent, string? Message) GetResponse(WebRequest request);
}

public class PredictRequestHandler: IPredictRequestHandler
{
    public string SavedImageFolderPath {get;} = "../Public/SavedImages";
    public string CredentialFilePath {get;} = "../Roboflow.txt";
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
        Confidence = formCollection["confidence"];
        Overlap = formCollection["overlap"];
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
                    "&confidence=" + Confidence[..Math.Min(6, Confidence.Length)] +
                    "&overlap=" + Overlap[..Math.Min(6, Overlap.Length)] +
                    "&name=" + FileName;
        } catch (Exception ex)
        {
            message = $"Exception occured when reading credentials: {ex.Message}";
            Console.WriteLine(message);
        }
        return (requestURL, message);

    }

    
    public (Stream? PredictPostStream, WebRequest? PredictPostRequest, string? Message) PostRequest(IFormCollection formCollection)
    {
        Stream? stream = null;
        WebRequest? request = null;
        string? message = "Error occured when posting a request: ";
        try
        {
            byte[]? data = GetImage().ImageData;
            string? uploadURL = BuildRequestString(formCollection).RequestURL;
            request = WebRequest.Create(uploadURL);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            message = "Request posted successfully";
        } catch (Exception ex)
        {
            message += ex.Message;
            Console.WriteLine(message);
        }
        return (stream, request, message);
    }

    public (string? ResponseContent, string? Message) GetResponse(WebRequest request)
    {   
        string responseContent = null;
        string message = "Error when trying IRequestHandler GetResponse: ";
        try
        {
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
            message = "Successfully get response";
            
        } catch (Exception ex)
        {
            message = message + ex.Message;
            Console.WriteLine(message);
        }
        return (responseContent, message);        
    }
}