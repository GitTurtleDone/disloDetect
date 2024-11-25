// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Http;
// using System;
// using System.IO;
// using System.Threading.Tasks;
// using System.IO.Compression;
// using System.Text.Json;;
// using System.Net.Http.Headers;
// // using excel_upload_be.Models;
// // using excel_upload_be.Services;
// using System.Text;
// using System.Web;
// using Microsoft.AspNetCore.Http; 
// using Microsoft.AspNetCore.Http.HttpResults;
using dislodetect_be.Controllers;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(5226);
//     options.ListenAnyIP(7226, ListenOptions =>{
//         ListenOptions.UseHttps("./app/certs/cert.pfx", "dotnetpass");
//     });
// });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<IPredictRequestHandler, PredictRequestHandler>();

var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
                    ?? "http://localhost:3000,https://localhost:3000,http://dislodetect.azurewebsites.net:3000,https://dislodetect.azurewebsites.net:3000,http://dislodetect.azurewebsites.net,https://dislodetect.azurewebsites.net";
var originArray = allowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options=>
{
    options.AddDefaultPolicy(builder =>
    {
        // builder.WithOrigins(originArray).AllowAnyHeader().AllowAnyMethod();
        builder.WithOrigins(originArray).AllowAnyHeader().AllowAnyMethod();
    }
    );
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}



// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 3).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

// app.MapPost("/uploadphotofile", async (HttpContext context) =>
// {
//     string publicFolderPath = @"..\..\Public";
//         // FolderNode folderTree;
        
//         try
//         {
//             var formCollection = await context.Request.ReadFormAsync();
//             var file = formCollection.Files[0];

//             if (file.Length > 0)
//             {
//                 var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
//                 var fileExtension = Path.GetExtension(fileName);

//                 var filePath = Path.Combine( publicFolderPath, fileName);

//                 using (var stream = new FileStream(filePath, FileMode.Create))
//                 {
//                     await file.CopyToAsync(stream);
//                 }
//                 Console.WriteLine($"Copied photo file path: {filePath}");  
//                 TypedResults.Ok(new Message() { Text = "Photo was uploaded!" });
//             }
//             else
//             {
//                 TypedResults.BadRequest(new Message() { Text = "File is empty" });;
//             }
//         }
//         catch (Exception ex)
//         {
//             return StatusCode(500, $"Internal server error: {ex.Message}");
//         }
// }
// )

// .WithName("UploadPhotoFile")
// .WithOpenApi();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
