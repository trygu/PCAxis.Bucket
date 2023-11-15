using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Amazon.S3;
using Amazon.S3.Model;

namespace PxTools.Bucket.Client.Tests
{
    [TestClass]
    public class BucketClientTests
    {
        private BucketClient? bucketClient;
        private Mock<IAmazonS3>? mockAmazonS3;

        public TestContext? TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            // This will detect the method that called it and initialize accordingly
            if (TestContext!.TestName.Contains("_GCS_"))
                SetupForGCS();
            else
                SetupForAWS();
        }
        private void SetupForAWS()
        {
            CommonSetup(useGCS: false);
        }

        private void SetupForGCS()
        {
            CommonSetup(useGCS: true);
        }

        private void CommonSetup(bool useGCS)
        {
            // Mock setup
            mockAmazonS3 = new Mock<IAmazonS3>();

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

            bucketClient = new BucketClient("http://s3.dapla.ssb.no", "fakeKey", "fakeSecret", useGCS, "fakePxBucket", mockAmazonS3!.Object);
        }

        [TestMethod]
        public void ListFiles_ReturnsExpectedFiles()
        {
            var files = bucketClient!.ListFiles();

            Assert.AreEqual(2, files.Count);
            Assert.AreEqual("file1.px", files.First().Key);
            Assert.AreEqual("file2.px", files.Last().Key);
        }

        [TestMethod]
        public void CreateBucket_CreatesExpectedBucket()
        {
            mockAmazonS3!.Setup(client => client.PutBucketAsync(It.IsAny<PutBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutBucketResponse());

            bucketClient!.CreateBucket("testBucket");

            mockAmazonS3.Verify(client => client.PutBucketAsync(It.Is<PutBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        // ... (rest of your methods with the same adjustments for nullability)

        [TestMethod]
        public void UploadFile_UploadsExpectedFile()
        {
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));

            mockAmazonS3!.Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutObjectResponse());

            bucketClient!.UploadFile("fileKey", mockStream);

            mockAmazonS3.Verify(client => client.PutObjectAsync(It.Is<PutObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteFile_DeletesExpectedFile()
        {
            mockAmazonS3!.Setup(client => client.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteObjectResponse());

            bucketClient!.DeleteFile("fileKey");

            mockAmazonS3.Verify(client => client.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteBucket_DeletesExpectedBucket()
        {
            mockAmazonS3!.Setup(client => client.DeleteBucketAsync(It.IsAny<DeleteBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteBucketResponse());

            bucketClient!.DeleteBucket("testBucket");

            mockAmazonS3.Verify(client => client.DeleteBucketAsync(It.Is<DeleteBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void ReadFile_ReadsExpectedFile()
        {
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));
            var mockGetObjectResponse = new GetObjectResponse { ResponseStream = mockStream };

            mockAmazonS3!.Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(mockGetObjectResponse);

            string content = "";
            bucketClient!.ReadFile("fileKey", stream =>
            {
                using var reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            });

            Assert.AreEqual("Test Content", content);
        }

        [TestMethod]
        public void ListFiles_GCS_ReturnsExpectedFiles()
        {
            SetupForGCS();
            var files = bucketClient!.ListFiles();

            Assert.AreEqual(2, files.Count);
            Assert.AreEqual("file1.px", files.First().Key);
            Assert.AreEqual("file2.px", files.Last().Key);
        }

        [TestMethod]
        public void CreateBucket_GCS_CreatesExpectedBucket()
        {
            SetupForGCS();
            mockAmazonS3!.Setup(client => client.PutBucketAsync(It.IsAny<PutBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutBucketResponse());

            bucketClient!.CreateBucket("testBucket");

            mockAmazonS3.Verify(client => client.PutBucketAsync(It.Is<PutBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void UploadFile_GCS_UploadsExpectedFile()
        {
            SetupForGCS();
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));

            mockAmazonS3!.Setup(client => client.PutObjectAsync(It.IsAny<PutObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new PutObjectResponse());

            bucketClient!.UploadFile("fileKey", mockStream);

            mockAmazonS3.Verify(client => client.PutObjectAsync(It.Is<PutObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteFile_GCS_DeletesExpectedFile()
        {
            SetupForGCS();
            mockAmazonS3!.Setup(client => client.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteObjectResponse());

            bucketClient!.DeleteFile("fileKey");

            mockAmazonS3.Verify(client => client.DeleteObjectAsync(It.Is<DeleteObjectRequest>(req => req.Key == "fileKey"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void DeleteBucket_GCS_DeletesExpectedBucket()
        {
            SetupForGCS();
            mockAmazonS3!.Setup(client => client.DeleteBucketAsync(It.IsAny<DeleteBucketRequest>(), default(CancellationToken)))
                .ReturnsAsync(new DeleteBucketResponse());

            bucketClient!.DeleteBucket("testBucket");

            mockAmazonS3.Verify(client => client.DeleteBucketAsync(It.Is<DeleteBucketRequest>(req => req.BucketName == "testBucket"), default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public void ReadFile_GCS_ReadsExpectedFile()
        {
            SetupForGCS();
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("Test Content"));
            var mockGetObjectResponse = new GetObjectResponse { ResponseStream = mockStream };

            mockAmazonS3!.Setup(client => client.GetObjectAsync(It.IsAny<GetObjectRequest>(), default(CancellationToken)))
                .ReturnsAsync(mockGetObjectResponse);

            string content = "";
            bucketClient!.ReadFile("fileKey", stream =>
            {
                using var reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            });

            Assert.AreEqual("Test Content", content);
        }
    }
}
