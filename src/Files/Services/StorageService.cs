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
    public string UrlDomain { get; }
    public string AccessKey { get; }
    public string SecretKey { get; }
    public string BucketName { get; }
    public string BucketRegion { get; }
    public StorageService(IConfiguration config)
    {
        _config = config;
        UrlDomain = _config["URL_DOMAIN"]!;
        AccessKey = _config["AWS_ACCESS_KEY"]!;
        SecretKey = _config["AWS_SECRET_KEY"]!;
        BucketName = _config["AWS_BUCKET_NAME"]!;
        BucketRegion = _config["AWS_BUCKET_REGION"]!;
        var regionAsRegion = RegionEndpoint.GetBySystemName(BucketRegion);
        _awsClient = new AmazonS3Client(AccessKey, SecretKey, regionAsRegion);
    }
    //Just for testing purpose
    public StorageService(IConfiguration config, IAmazonS3 awsClient)
    {
        _config = config;
        UrlDomain = _config["URL_DOMAIN"]!;
        AccessKey = _config["AWS_ACCESS_KEY"]!;
        SecretKey = _config["AWS_SECRET_KEY"]!;
        BucketName = _config["AWS_BUCKET_NAME"]!;
        BucketRegion = _config["AWS_BUCKET_REGION"]!;
        _awsClient = awsClient;
        
    }


    public async Task<bool> CheckIfExistsItem(string key)
    {
        Console.WriteLine("=====================");
        Console.WriteLine(SecretKey);
        Console.WriteLine(AccessKey);
                Console.WriteLine("=====================");


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
                return false; // file does not exist
            else
                throw; // Has been any error
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
        // Convertir el base64 string en un array de bytes
        byte[] bytes = Convert.FromBase64String(base64String);
        // Crear una solicitud de subida de archivo
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
            throw new ArgumentException($"Failed to upload to cloud service {e.Message?? "message :" + e.Message }");

        }
        // Ejecutar la subida de archivo

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
            throw new ArgumentException($"Failed to upload to cloud service {e.Message?? "message :" + e.Message }");

        }
    }

}