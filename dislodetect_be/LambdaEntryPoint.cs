using Amazon.Lambda.AspNetCoreServer;

namespace dislodetect_be;

public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        // Tell the Lambda bridge to treat this content type as binary
        RegisterResponseContentEncodingForContentType("multipart/form-data", ResponseContentEncoding.Base64);
        builder.UseStartup<Startup>();
    }
}