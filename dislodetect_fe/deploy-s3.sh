#!/bin/bash

# Configuration
BUCKET_NAME="dislodetect-frontend-$(date +%s)"  # Unique bucket name
REGION="us-east-1"  # Change to your preferred region
PROFILE="default"   # Change to your AWS profile if needed

echo "🚀 Deploying React app to S3 + CloudFront..."

# Build the React app
echo "📦 Building React app..."
npm run build

# Create S3 bucket
echo "🪣 Creating S3 bucket: $BUCKET_NAME"
aws s3 mb s3://$BUCKET_NAME --region $REGION --profile $PROFILE

# Configure bucket for static website hosting
echo "🌐 Configuring static website hosting..."
aws s3 website s3://$BUCKET_NAME \
  --index-document index.html \
  --error-document index.html \
  --profile $PROFILE

# Set bucket policy for public read access
echo "🔓 Setting bucket policy..."
cat > bucket-policy.json << EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "PublicReadGetObject",
      "Effect": "Allow",
      "Principal": "*",
      "Action": "s3:GetObject",
      "Resource": "arn:aws:s3:::$BUCKET_NAME/*"
    }
  ]
}
EOF

aws s3api put-bucket-policy \
  --bucket $BUCKET_NAME \
  --policy file://bucket-policy.json \
  --profile $PROFILE

# Upload build files to S3
echo "⬆️ Uploading files to S3..."
aws s3 sync build/ s3://$BUCKET_NAME \
  --delete \
  --cache-control "max-age=31536000" \
  --exclude "*.html" \
  --profile $PROFILE

# Upload HTML files with no-cache
aws s3 sync build/ s3://$BUCKET_NAME \
  --delete \
  --cache-control "no-cache" \
  --include "*.html" \
  --profile $PROFILE

# Create CloudFront distribution
echo "☁️ Creating CloudFront distribution..."
cat > cloudfront-config.json << EOF
{
  "CallerReference": "dislodetect-$(date +%s)",
  "Comment": "DisloDetect Frontend Distribution",
  "DefaultCacheBehavior": {
    "TargetOriginId": "$BUCKET_NAME",
    "ViewerProtocolPolicy": "redirect-to-https",
    "TrustedSigners": {
      "Enabled": false,
      "Quantity": 0
    },
    "ForwardedValues": {
      "QueryString": false,
      "Cookies": {
        "Forward": "none"
      }
    },
    "MinTTL": 0,
    "DefaultTTL": 86400,
    "MaxTTL": 31536000
  },
  "Origins": {
    "Quantity": 1,
    "Items": [
      {
        "Id": "$BUCKET_NAME",
        "DomainName": "$BUCKET_NAME.s3-website-$REGION.amazonaws.com",
        "CustomOriginConfig": {
          "HTTPPort": 80,
          "HTTPSPort": 443,
          "OriginProtocolPolicy": "http-only"
        }
      }
    ]
  },
  "Enabled": true,
  "DefaultRootObject": "index.html",
  "CustomErrorResponses": {
    "Quantity": 1,
    "Items": [
      {
        "ErrorCode": 404,
        "ResponsePagePath": "/index.html",
        "ResponseCode": "200",
        "ErrorCachingMinTTL": 300
      }
    ]
  },
  "PriceClass": "PriceClass_100"
}
EOF

DISTRIBUTION_ID=$(aws cloudfront create-distribution \
  --distribution-config file://cloudfront-config.json \
  --profile $PROFILE \
  --query 'Distribution.Id' \
  --output text)

echo "✅ Deployment complete!"
echo "📝 S3 Bucket: $BUCKET_NAME"
echo "🌍 S3 Website URL: http://$BUCKET_NAME.s3-website-$REGION.amazonaws.com"
echo "☁️ CloudFront Distribution ID: $DISTRIBUTION_ID"
echo "⏳ CloudFront URL will be available in 10-15 minutes"

# Save deployment info
cat > deployment-info.txt << EOF
Deployment Date: $(date)
S3 Bucket: $BUCKET_NAME
S3 Website URL: http://$BUCKET_NAME.s3-website-$REGION.amazonaws.com
CloudFront Distribution ID: $DISTRIBUTION_ID
Region: $REGION
EOF

# Cleanup temporary files
rm bucket-policy.json cloudfront-config.json

echo "💾 Deployment info saved to deployment-info.txt"