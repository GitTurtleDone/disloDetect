using dislodetect_be.Controllers;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var connectionString = Environment.GetEnvironmentVariable("AZ_BLOB_CONNECTION_STRING");
        services.AddSingleton(new BlobServiceClient(connectionString));
        services.AddScoped<IPredictRequestHandler, PredictRequestHandler>();
        services.AddScoped<IUploadPhotoFileRequestHandler, UploadPhotoFileRequestHandler>();
    })
    .Build();
host.Run();


