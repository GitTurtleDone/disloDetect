using dislodetect_be.Controllers;
using Amazon.S3;

namespace dislodetect_be;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IUploadPhotoFileRequestHandler, UploadPhotoFileRequestHandler>();
        services.AddScoped<IPredictRequestHandler, PredictRequestHandler>();
        
        // CORS for Lambda
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("https://dislodetect-1766191540.s3.ap-southeast-6.amazonaws.com")
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}