using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;
using System.Text;
namespace dislodetect_be.Controllers;
[ApiController]
[Route("[controller]")]
public class PredictController : ControllerBase
{
    [HttpPost (Name = "PostPredict")]
    public async Task<IActionResult> Predict()
    {
        try
        {   
            
            string publicFolderPath = @"../Public/SavedImages";
            string fileName = Directory.GetFiles(publicFolderPath)[0];
            byte[] image = System.IO.File.ReadAllBytes(fileName);
            string encoded = Convert.ToBase64String(image);
            byte[] data = System.Text.Encoding.ASCII.GetBytes(encoded);
            string[] lines = System.IO.File.ReadAllLines("../Roboflow.txt");
            string API_KEY = lines[0];
            string DATASET_NAME = lines[1];
            string DATASET_VERSION = lines[2];
            
            
            //Contruct the URL
            string uploadURL =
                    "https://detect.roboflow.com/" +
                    DATASET_NAME + "/" + DATASET_VERSION +
                    "?api_key=" + API_KEY +
                    "&name=" + fileName;
            // Service Request Config
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            //Configure Request
            WebRequest request = WebRequest.Create(uploadURL);
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
                    using (StreamReader sr99 = new StreamReader(stream))
                    {
                        responseContent = sr99.ReadToEnd();
                    }
                }
            }


            Console.WriteLine($"Image in the public folder: {fileName}");
            Console.WriteLine($"Response Content: {responseContent}");
            return Ok(responseContent);
        } 
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

    }
}