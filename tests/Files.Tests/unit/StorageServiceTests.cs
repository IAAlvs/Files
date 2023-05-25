using Microsoft.Extensions.Configuration;
using Files.Interfaces;
using Files.Services;
using NSubstitute;
using Files.Clients;
using System.Text.Json;
public class StorageServiceTests
{
    private readonly IStorageService _service;
    private readonly IConfiguration _config;
    private readonly IRequestInvoker _invoker;

    public StorageServiceTests()
    {
        _invoker = Substitute.For<RequestInvoker>();
        _config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").AddEnvironmentVariables().Build();
        _service = new StorageService(_config, _invoker);
        
    }
    [Fact]
    public void WithStorageService_ReadsEnvironmentVariables()
    {
        // Given
        var expectedUrlDomain = "fakeurl_domain";
        var expectedAccessKey = "fake_aws_key";
        var expetectedSecretKey = "fake_aws_secret_key";
        var expectedBucketName = "fake_aws_bucket_name";
        var expectedBucketRegion = "fake_aws_bucket_region";
        // When
        var service = new StorageService(_config, _invoker);
        // Then
        Assert.Equal(expectedUrlDomain, service.UrlDomain);
        Assert.Equal(expectedAccessKey, service.AccessKey);
        Assert.Equal(expetectedSecretKey, service.SecretKey);
        Assert.Equal(expectedBucketName, service.BucketName);
        Assert.Equal(expectedBucketRegion, service.BucketRegion);
    }
}