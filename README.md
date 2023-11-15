# BucketClient Library

The `BucketClient` library provides a simple wrapper around the Amazon S3 SDK and Google Cloud Storage to interact with Amazon S3 buckets, Google Cloud Storage buckets, and their objects. It includes methods for creating buckets, listing files, uploading files, deleting files, deleting buckets, and reading file contents.

## Usage

You can use the `BucketClient` for both Amazon S3 and Google Cloud Storage based on the provided configuration.

### For Amazon S3:

```csharp
using PxTools.Bucket.Client;

class Program
{
    static void Main()
    {
        // Initialize BucketClient for Amazon S3
        var bucketClient = new BucketClient("http://s3.dapla.ssb.no", "fakeKey", "fakeSecret", false, "fakePxBucket");

        // List files in the default bucket
        var files = bucketClient.ListFiles();
        foreach (var file in files)
        {
            Console.WriteLine($"File Key: {file.Key}");
        }

        // Create a new bucket
        bucketClient.CreateBucket("newBucket");

        // Upload a file to the default bucket
        using (var fileStream = File.OpenRead("file.txt"))
        {
            bucketClient.UploadFile("fileKey", fileStream);
        }

        // Delete a file from the default bucket
        bucketClient.DeleteFile("fileKey");

        // Delete a bucket
        bucketClient.DeleteBucket("newBucket");

        // Read file content
        bucketClient.ReadFile("fileKey", stream =>
        {
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Console.WriteLine($"File Content: {content}");
        });
    }
}
```
### For Google Cloud Storage:

```csharp
using PCAxis.S3.Client;

class Program
{
    static void Main()
    {
        // Initialize BucketClient for Google Cloud Storage
        var bucketClient = new BucketClient("http://storage.googleapis.com", "fakeKey", "fakeSecret", true, "fakePxBucket");
        // List files in the default bucket
        var files = bucketClient.ListFiles();
        foreach (var file in files)
        {
            Console.WriteLine($"File Key: {file.Key}");
        }

        // Create a new bucket
        bucketClient.CreateBucket("newBucket");

        // Upload a file to the default bucket
        using (var fileStream = File.OpenRead("file.txt"))
        {
            bucketClient.UploadFile("fileKey", fileStream);
        }

        // Delete a file from the default bucket
        bucketClient.DeleteFile("fileKey");

        // Delete a bucket
        bucketClient.DeleteBucket("newBucket");

        // Read file content
        bucketClient.ReadFile("fileKey", stream =>
        {
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Console.WriteLine($"File Content: {content}");
        });
    }
}

```
