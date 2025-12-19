#!/bin/bash

# Simple S3-only deployment (cheaper for testing)
BUCKET_NAME="dislodetect-simple-$(date +%s)"
REGION="us-east-1"

echo "🚀 Simple S3 deployment..."

# Build and deploy
npm run build

# Create bucket
aws s3 mb s3://$BUCKET_NAME --region $REGION

# Configure static hosting
aws s3 website s3://$BUCKET_NAME \
  --index-document index.html \
  --error-document index.html

# Make public
aws s3api put-bucket-policy --bucket $BUCKET_NAME --policy '{
  "Version": "2012-10-17",
  "Statement": [{
    "Sid": "PublicReadGetObject",
    "Effect": "Allow", 
    "Principal": "*",
    "Action": "s3:GetObject",
    "Resource": "arn:aws:s3:::'$BUCKET_NAME'/*"
  }]
}'

# Upload files
aws s3 sync build/ s3://$BUCKET_NAME --delete

echo "✅ Deployed to: http://$BUCKET_NAME.s3-website-$REGION.amazonaws.com"
echo "💰 Cost: ~$0.50/month for low traffic"