using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: InternalsVisibleTo("PCAxis.S3.Client.Tests")]

namespace PCAxis.S3.Client
{
    public class S3Client
    {
        internal readonly IAmazonS3 client;
        private readonly string? defaultBucketName;

        public S3Client(string endpoint, string accessKey, string secretKey, bool useGCS = false, string? defaultBucketName = null, IAmazonS3? amazonS3 = null)
        {
            var clientConfig = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true,
                UseHttp = true
            };

            if (useGCS)
            {
                clientConfig.SignatureVersion = "V4";
            }

            client = amazonS3 ?? new AmazonS3Client(accessKey, secretKey, clientConfig);
            this.defaultBucketName = defaultBucketName;
        }

        public void CreateBucket(string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new PutBucketRequest
            {
                BucketName = bucket
            };
            client.PutBucketAsync(request).Wait();
        }

        public List<S3Object> ListFiles(string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var allFiles = new List<S3Object>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucket
            };

            ListObjectsV2Response response;
            do
            {
                response = client.ListObjectsV2Async(request).Result;
                allFiles.AddRange(response.S3Objects);
                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);

            return allFiles;
        }

        public void UploadFile(string key, Stream fileStream, string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = fileStream
            };
            client.PutObjectAsync(request).Wait();
        }

        public void DeleteFile(string key, string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            };
            client.DeleteObjectAsync(request).Wait();
        }

        public void DeleteBucket(string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new DeleteBucketRequest
            {
                BucketName = bucket
            };
            client.DeleteBucketAsync(request).Wait();
        }

        public void ReadFile(string key, Action<Stream> processStream, string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            using (var response = client.GetObjectAsync(request).Result)
            {
                processStream(response.ResponseStream);
            }
        }
    }
}
