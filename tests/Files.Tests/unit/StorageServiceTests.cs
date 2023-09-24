using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Files.Interfaces;
using Files.Services;
using NSubstitute;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net.Http;
using Files.Clients;
using System.Text.Json;
public class StorageServiceTests
{
    private readonly IStorageService _service;
    private readonly IConfiguration _config;
    private readonly ILogger<IStorageService> _logger;

    public StorageServiceTests()
    {
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();
        _logger = Substitute.For<ILogger<IStorageService>>();
        _service = new StorageService(_config, _logger);
        
    }
    [Fact]
    public void WithStorageService_ReadsEnvironmentVariables()
    {
        // Given
        var expectedUrlDomain = "fakeurl_domain";
        var expectedAccessKey = "fake_aws_key";
        var expetectedSecretKey = "fake_aws_secret_key";
        var expectedBucketName = "fake_aws_bucket_name";
        var expectedBucketRegion = "us-west-1"; //Just for tests purpose
        // When
        var service = new StorageService(_config, _logger);
        // Then
        Assert.Equal(expectedUrlDomain, service.UrlDomain);
        Assert.Equal(expectedAccessKey, service.AccessKey);
        Assert.Equal(expetectedSecretKey, service.SecretKey);
        Assert.Equal(expectedBucketName, service.BucketName);
        Assert.Equal(expectedBucketRegion, service.BucketRegion);
    }
    [Fact]
    public async void WithItemNameNotInStorage_CheckIfExistItem_False()
    {
        // Given
        string itemName = "falseFileName";
        var region = RegionEndpoint.GetBySystemName(_config["AWS_BUCKET_REGION"]);
        //Mock o aws client
        var awsClient = Substitute.For<IAmazonS3>();
        awsClient.When(x => x.GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(v => v.Key == itemName))).Do(
            x =>  {throw new AmazonS3Exception("The specified key does not exist.", null)
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                ErrorCode = "NoSuchKey",
                RequestId = "1234567890"
            };}
        );
        var service = new StorageService(_config, awsClient, _logger);
        // When
        var response = await service.CheckIfExistsItem(itemName);
        // Then
        Assert.False(response);
    }    
    [Fact]
    public async void WithItemInStorage_CheckIfExistsItem_True()
    {
        // Given
        string itemName = "trueFileName";
        var awsClient = Substitute.For<IAmazonS3>();
        awsClient.GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(v => v.Key == itemName)).
        Returns(new GetObjectMetadataResponse());
        var service = new StorageService(_config, awsClient, _logger);
        // When
        var response = await service.CheckIfExistsItem(itemName);
        // Then
        Assert.True(response);
    }   
    [Fact]
    public async void WithExistenceItemId_GetItemById_ReturnsStringBase64()
    {
        // Given
        string itemName = "fileName";
        string expectedBase64File = "SGVsbG8gV29ybGQh"; // Ejemplo de archivo en base64
        byte[] fileBytes = Convert.FromBase64String(expectedBase64File);
        var clientMoq = Substitute.For<IAmazonS3>();
        clientMoq.GetObjectAsync(Arg.Is<GetObjectRequest>(v => v.Key == itemName)).
        Returns(
            new GetObjectResponse
            {
                ResponseStream = new MemoryStream(fileBytes),
                ContentLength = fileBytes.Length,
                HttpStatusCode = System.Net.HttpStatusCode.OK,
            }
        );
        var service = new StorageService(_config, clientMoq, _logger);
        // When
        var response = await service.GetFileById(itemName);
        // Then
        Assert.Equal(expectedBase64File, response);

    }
    [Fact]
    public async void WithNotExistenceItemId_GetItemById_ThrowsError()
    {
        // Given
        string itemName = "fileName";
        string expectedBase64File = "SGVsbG8gV29ybGQh"; // Ejemplo de archivo en base64
        byte[] fileBytes = Convert.FromBase64String(expectedBase64File);
        var clientMoq = Substitute.For<IAmazonS3>();
        clientMoq.GetObjectAsync(Arg.Is<GetObjectRequest>(v => v.Key == itemName)).
        Returns(
            new GetObjectResponse
            {
                ResponseStream = new MemoryStream(fileBytes),
                ContentLength = fileBytes.Length,
                HttpStatusCode = System.Net.HttpStatusCode.OK,
            }
        );
        clientMoq.When(x => x.GetObjectMetadataAsync(Arg.Is<GetObjectMetadataRequest>(v => v.Key == itemName))).Do(
            x =>  {throw new AmazonS3Exception("The specified key does not exist.", null)
            {
                StatusCode = System.Net.HttpStatusCode.NotFound,
                ErrorCode = "NoSuchKey",
                RequestId = "1234567890"
            };}
        );
        var service = new StorageService(_config, clientMoq, _logger);
        // Action is throwed       
        // Then
        await Assert.ThrowsAnyAsync<ArgumentException>(
            async () => await service.GetFileById(itemName)
        );

    }
    [Fact]
    public async void WithFile_UploadPublicFile_ReturnsFileId()
    {
        // Given
        string fileId = Guid.NewGuid().ToString();
        string fileB64 = "SGVsbG8gV29ybGQh";
        var awsClient = Substitute.For<IAmazonS3>();
        awsClient.PutObjectAsync(Arg.Is<PutObjectRequest>(v => v.Key == fileId)).
        Returns(new PutObjectResponse
        {
            HttpStatusCode = System.Net.HttpStatusCode.OK
        });
        awsClient.PutACLAsync(Arg.Is<PutACLRequest>(k => k.Key == fileId)).Returns(
            new PutACLResponse { HttpStatusCode = System.Net.HttpStatusCode.OK }
        );
        var service = new StorageService(_config, awsClient, _logger);
        // When
        var response = await service.UploadPublicFile(fileId, fileB64);
        // Then
        Assert.Equal(response, $"https://{service.PublicBucketName}.s3.amazonaws.com/{fileId}");
    }   
}