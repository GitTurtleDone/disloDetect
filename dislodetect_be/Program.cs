using dislodetect_be.Controllers;
// using Microsoft.AspNetCore.Server.Kestrel.Core;
using Azure.Storage.Blobs;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddScoped<IPredictRequestHandler, PredictRequestHandler>();

var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
                    ?? "http://localhost:3000,https://localhost:3000,http://dislodetect.azurewebsites.net:3000,https://dislodetect.azurewebsites.net:3000,http://dislodetect.azurewebsites.net,https://dislodetect.azurewebsites.net";
var originArray = allowedOrigins.Split(",", StringSplitOptions.RemoveEmptyEntries);
Console.WriteLine("Allowed Origins:");
foreach (var origin in originArray)
{
    Console.WriteLine(origin);
}
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

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();


