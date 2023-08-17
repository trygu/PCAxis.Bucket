using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Amazon.S3;
using Amazon.S3.Model;
using PCAxis.S3.Client;

namespace PCAxis.S3.Client.Tests
{
    [TestClass]
    public class S3ClientTests
    {
        private S3Client? s3Client;
        private Mock<IAmazonS3>? mockAmazonS3;

        [TestInitialize]
        public void Setup()
        {
            // Mock setup
            mockAmazonS3 = new Mock<IAmazonS3>();

            // Setup for ListFiles
            var mockListObjectsResponse = new ListObjectsV2Response
            {
                S3Objects = new System.Collections.Generic.List<S3Object>
                {
                    new S3Object { Key = "file1.px" },
                    new S3Object { Key = "file2.px" }
                }
            };
            mockAmazonS3.Setup(client => client.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default(CancellationToken)))
                .ReturnsAsync(mockListObjectsResponse);

            // Initialize S3Client with mocked IAmazonS3
            s3Client = new S3Client("http://s3.dapla.ssb.no", "fakeKey", "fakeSecret", "fakePxBucket", mockAmazonS3.Object);
        }

        [TestMethod]
        public void ListFiles_ReturnsExpectedFiles()
        {
            var files = s3Client.ListFiles();

            Assert.AreEqual(2, files.Count);
            Assert.AreEqual("file1.px", files.First().Key);
            Assert.AreEqual("file2.px", files.Last().Key);
        }

        [TestMethod]
        public void CreateBucket_CreatesExpectedBucket()
        {
            mockAmazonS3.Setup(client => client.PutBucketAsync(It.IsAny<PutBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutBucketResponse()); // Simulate successful bucket creation

            s3Client.CreateBucket("testBucket");

            mockAmazonS3.Verify(client => client.PutBucketAsync(It.Is<PutBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void UploadFile_UploadsExpectedFile()
        {
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));

            mockAmazonS3.Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutObjectResponse()); // Simulate successful file upload

            s3Client.UploadFile("fileKey", mockStream);

            mockAmazonS3.Verify(client => client.PutObjectAsync(It.Is<PutObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteFile_DeletesExpectedFile()
        {
            mockAmazonS3.Setup(client => client.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteObjectResponse()); // Simulate successful file deletion

            s3Client.DeleteFile("fileKey");

            mockAmazonS3.Verify(client => client.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteBucket_DeletesExpectedBucket()
        {
            mockAmazonS3.Setup(client => client.DeleteBucketAsync(It.IsAny<DeleteBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteBucketResponse()); // Simulate successful bucket deletion

            s3Client.DeleteBucket("testBucket");

            mockAmazonS3.Verify(client => client.DeleteBucketAsync(It.Is<DeleteBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void ReadFile_ReadsExpectedFile()
        {
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));
            var mockGetObjectResponse = new GetObjectResponse { ResponseStream = mockStream };

            mockAmazonS3.Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(mockGetObjectResponse); // Simulate successful file read

            string content = "";
            s3Client.ReadFile("fileKey", stream =>
            {
                using var reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            });

            Assert.AreEqual("Test Content", content);
        }
    }
}
