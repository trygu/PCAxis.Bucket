# S3Client Library

The `S3Client` library provides a simple wrapper around the Amazon S3 SDK to interact with Amazon S3 buckets and objects. It includes methods for creating buckets, listing files, uploading files, deleting files, deleting buckets, and reading file contents.

## Usage

```csharp
using PCAxis.S3.Client;

class Program
{
    static void Main()
    {
        // Initialize S3Client
        var s3Client = new S3Client("http://s3.dapla.ssb.no", "fakeKey", "fakeSecret", "fakePxBucket");

        // List files in the default bucket
        var files = s3Client.ListFiles();
        foreach (var file in files)
        {
            Console.WriteLine($"File Key: {file.Key}");
        }

        // Create a new bucket
        s3Client.CreateBucket("newBucket");

        // Upload a file to the default bucket
        using (var fileStream = File.OpenRead("file.txt"))
        {
            s3Client.UploadFile("fileKey", fileStream);
        }

        // Delete a file from the default bucket
        s3Client.DeleteFile("fileKey");

        // Delete a bucket
        s3Client.DeleteBucket("newBucket");

        // Read file content
        s3Client.ReadFile("fileKey", stream =>
        {
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            Console.WriteLine($"File Content: {content}");
        });
    }
}
```
