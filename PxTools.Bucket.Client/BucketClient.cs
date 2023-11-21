using System.Runtime.CompilerServices;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: InternalsVisibleTo("PxTools.Bucket.Client.Tests")]

namespace PxTools.Bucket.Client
{
    /// <summary>
    /// Provides functionality for interacting with S3 (or S3-compatible) services.
    /// </summary>
    public class BucketClient
    {
        internal readonly IAmazonS3 client;
        private readonly string? defaultBucketName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketClient"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint URL of the S3 service.</param>
        /// <param name="accessKey">The access key for the S3 service.</param>
        /// <param name="secretKey">The secret key for the S3 service.</param>
        /// <param name="sessionToken">An optional session token.Defaults to null.</param>
        /// <param name="useGCS">If set to true, will use Google Cloud Storage's signature version V4. Defaults to false.</param>
        /// <param name="defaultBucketName">The default bucket name to use if none is specified in methods. Defaults to null.</param>
        /// <param name="amazonS3">An optional custom S3 client instance. Defaults to null.</param>
        /// <param name="sessionToken">An optional session token.Defaults to null.</param>
        public BucketClient(string endpoint, string accessKey, string secretKey, string? sessionToken = null, bool useGCS = false, string? defaultBucketName = null, IAmazonS3? amazonS3 = null)
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

            if (sessionToken != null)
            {
                client = amazonS3 ?? new AmazonS3Client(accessKey, secretKey, sessionToken, clientConfig);
            }
            else
            {
                client = amazonS3 ?? new AmazonS3Client(accessKey, secretKey, clientConfig);
            }

            this.defaultBucketName = defaultBucketName;
        }


        /// <summary>
        /// Creates a new bucket.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to create. If not provided, uses the default bucket name.</param>
        public void CreateBucket(string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new PutBucketRequest
            {
                BucketName = bucket
            };
            client.PutBucketAsync(request).Wait();
        }

        /// <summary>
        /// Lists all files in a bucket.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to list files from. If not provided, uses the default bucket name.</param>
        /// <returns>A list of <see cref="S3Object"/> representing the files in the bucket.</returns>
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

        /// <summary>
        /// Uploads a file to a bucket.
        /// </summary>
        /// <param name="key">The key (path) where the file should be stored in the bucket.</param>
        /// <param name="fileStream">The stream containing the file content to upload.</param>
        /// <param name="bucketName">The name of the bucket to upload to. If not provided, uses the default bucket name.</param>
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

        /// <summary>
        /// Deletes a file from a bucket.
        /// </summary>
        /// <param name="key">The key (path) of the file to delete.</param>
        /// <param name="bucketName">The name of the bucket to delete from. If not provided, uses the default bucket name.</param>
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

        /// <summary>
        /// Deletes a bucket.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to delete. If not provided, uses the default bucket name.</param>
        public void DeleteBucket(string? bucketName = null)
        {
            var bucket = bucketName ?? defaultBucketName;
            var request = new DeleteBucketRequest
            {
                BucketName = bucket
            };
            client.DeleteBucketAsync(request).Wait();
        }

        /// <summary>
        /// Reads a file from a bucket and processes its content using a provided action.
        /// </summary>
        /// <param name="key">The key (path) of the file to read.</param>
        /// <param name="processStream">The action to execute on the file's content stream.</param>
        /// <param name="bucketName">The name of the bucket to read from. If not provided, uses the default bucket name.</param>
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
