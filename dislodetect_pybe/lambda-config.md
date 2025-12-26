# Lambda Configuration Notes

## Binary Media Types for API Gateway
When deploying to Lambda, configure API Gateway with these binary media types:
- multipart/form-data
- image/jpeg
- image/png
- image/tiff
- application/octet-stream

## Environment Variables to Set:
- ALLOWED_ORIGINS=https://dislodetect-1766191540.s3.ap-southeast-6.amazonaws.com

## Lambda Function Configuration:
- Memory: 3008 MB (maximum)
- Timeout: 15 minutes (maximum)
- Ephemeral storage: 10240 MB (if needed for large models)