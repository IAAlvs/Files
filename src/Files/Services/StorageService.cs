using Files.Interfaces;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
namespace Files.Services;
public class StorageService : IStorageService
{
    //private readonly IRequestInvoker _client;
    private readonly IConfiguration _config;
    private readonly IAmazonS3 _awsClient;
    private readonly ILogger _logger;
    public string UrlDomain { get; }
    public string AccessKey { get; }
    public string SecretKey { get; }
    public string BucketName { get; }
    public string BucketRegion { get; }
    public StorageService(IConfiguration config, ILogger<IStorageService> logger)
    {
        _config = config;
        UrlDomain = _config["URL_DOMAIN"]!;
        AccessKey = _config["AWS_ACCESS_KEY"]!;
        SecretKey = _config["AWS_SECRET_KEY"]!;
        BucketName = _config["AWS_BUCKET_NAME"]!;
        BucketRegion = _config["AWS_BUCKET_REGION"]!;
        var regionAsRegion = RegionEndpoint.GetBySystemName(BucketRegion);
        _awsClient = new AmazonS3Client(AccessKey, SecretKey, regionAsRegion);
        _logger = logger;
    }
    //Just for testing purpose
    public StorageService(IConfiguration config, IAmazonS3 awsClient, ILogger logger)
    {
        _config = config;
        UrlDomain = _config["URL_DOMAIN"]!;
        AccessKey = _config["AWS_ACCESS_KEY"]!;
        SecretKey = _config["AWS_SECRET_KEY"]!;
        BucketName = _config["AWS_BUCKET_NAME"]!;
        BucketRegion = _config["AWS_BUCKET_REGION"]!;
        _awsClient = awsClient;        
        _logger = logger;

    }
    public async Task<bool> CheckIfExistsItem(string key)
    {
        var request = new GetObjectMetadataRequest
        {
            BucketName = BucketName,
            Key = key
        };

        try
        {
            var response = await _awsClient.GetObjectMetadataAsync(request);
            return true; // El archivo existe
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false; 
            else{
                LogError(ex);
                throw new ArgumentException($"Failed to retrieve metadata about posible existence file with id : {key}");
            }
        }
    }

    public async Task<string> GetFileById(string id)
    {
        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = id
        };
        var existFile = await CheckIfExistsItem(id);
        if(!existFile)
            throw new ArgumentException($"File does not exist for file id: {id}");
        var response = await _awsClient.GetObjectAsync(request);
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();
        var str = Convert.ToBase64String(fileBytes);
        return str;
    }

    public async Task<bool> UploadFile(string fileId, string base64String)
    {
        // Convert base64 to bytes
        byte[] bytes = Convert.FromBase64String(base64String);
        // Create upload request
        var putRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = fileId,
            InputStream = new MemoryStream(bytes)
        };
        try{
            var response = await _awsClient.PutObjectAsync(putRequest);
            
            return true;
        }
        catch(Exception e){
            LogError(e);
            throw new ArgumentException($"Failed to upload in cloud service");
        }
    }
    public async Task<string> UploadPublicFile(string fileId, string base64String)
    {
        // Convert base64 string to byte array
        byte[] bytes = Convert.FromBase64String(base64String);
        // create putRequest to upload file
        var putRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = fileId,
            InputStream = new MemoryStream(bytes)
        };
        try{
            var response = await _awsClient.PutObjectAsync(putRequest);
            var aclRequest = new PutACLRequest
            {
                BucketName = BucketName,
                Key = fileId,
                CannedACL = S3CannedACL.PublicRead
            };

            PutACLResponse aclResponse = await _awsClient.PutACLAsync(aclRequest);
            return $"https://{BucketName}.s3.amazonaws.com/{fileId}";
        }
        catch(Exception e){
            LogError(e);
            throw new ArgumentException($"Failed to upload in cloud service");
        }
    }
    private void LogError(Exception error)
    {
        string errorMessage = $"Error: {error.GetType().Name}\nMessage: {error.Message}\nStack Trace:\n{error.StackTrace}";
        if (error.InnerException != null)
        {
            errorMessage += $"\nInner Exception:\n{error.InnerException.GetType().Name}\nInner Message: {error.InnerException.Message}\nInner Stack Trace:\n{error.InnerException.StackTrace}";
        }
        _logger.LogError(errorMessage);

    }

}