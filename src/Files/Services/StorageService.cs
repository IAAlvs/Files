using Files.Interfaces;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Threading.Tasks;
namespace Files.Services;
public class StorageService : IStorageService
{
    private readonly IRequestInvoker _client;
    private readonly IConfiguration _config;
    private readonly AmazonS3Client _awsClient;
    public string UrlDomain { get; }
    public string AccessKey { get; }
    public string SecretKey { get; }
    public string BucketName { get; }
    public string BucketRegion { get; }




    public StorageService(IConfiguration config, IRequestInvoker client)
    {
        _client = client;
        _config = config;
        UrlDomain = _config["URL_DOMAIN"]!;
        AccessKey = _config["AWS_ACCESS_KEY"]!;
        SecretKey = _config["AWS_SECRET_KEY"]!;
        BucketName = _config["AWS_BUCKET_NAME"]!;
        BucketRegion = _config["AWS_BUCKET_REGION"]!;
        _awsClient = new AmazonS3Client(AccessKey, SecretKey, BucketRegion);
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
                return false; // file does not exist
            else
                throw; // Has been any error
        }
    }

    public async Task<string> GetFileById(string Id)
    {
        var s3Client = new AmazonS3Client();
        var request = new GetObjectRequest
        {
            BucketName = BucketName,
            Key = Id
        };
        var response = await s3Client.GetObjectAsync(request);
        using (var stream = new MemoryStream())
        {
            await response.ResponseStream.CopyToAsync(stream);
            //return stream.ToArray();
            return "";
        }
    }

    public async Task<bool> UploadFile(string fileId, string base64String)
    {
        // Convertir el base64 string en un array de bytes
        byte[] bytes = Convert.FromBase64String(base64String);

        // Configurar la conexión a Amazon S3 usando VPC
        var region = RegionEndpoint.GetBySystemName(BucketRegion); // Cambiar por la región correspondiente
        var s3Config = new AmazonS3Config
        {
            
            RegionEndpoint = region,
            UseHttp = true,
            // Cambiar por el endpoint de S3 correspondiente a la región
            ServiceURL = $"https://s3.{region.SystemName}.amazonaws.com"
        };
        var s3Client = new AmazonS3Client(AccessKey, SecretKey, s3Config);
        // Crear una solicitud de subida de archivo
        var putRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = fileId,
            InputStream = new MemoryStream(bytes)
        };
        try{
            var response = await s3Client.PutObjectAsync(putRequest);
            return true;

        }
        catch(Exception e){
            throw new ArgumentException($"Failed to upload to cloud service {e.Message?? "message :" + e.Message }");

        }
        // Ejecutar la subida de archivo

    }
    public async Task<string> UploadPublicFile(string fileId, string base64String)
    {
        // Convertir el base64 string en un array de bytes
        byte[] bytes = Convert.FromBase64String(base64String);

        // Configurar la conexión a Amazon S3 usando VPC
        var region = RegionEndpoint.GetBySystemName(BucketRegion); // Cambiar por la región correspondiente
        var s3Config = new AmazonS3Config
        {
            
            RegionEndpoint = region,
            UseHttp = true,
            // Cambiar por el endpoint de S3 correspondiente a la región
            ServiceURL = $"https://s3.{region.SystemName}.amazonaws.com"
        };
        var s3Client = new AmazonS3Client(AccessKey, SecretKey, s3Config);
        // Crear una solicitud de subida de archivo
        var putRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            Key = fileId,
            InputStream = new MemoryStream(bytes)
        };
        try{
            var response = await s3Client.PutObjectAsync(putRequest);
            return "ok";

        }
        catch(Exception e){
            throw new ArgumentException($"Failed to upload to cloud service {e.Message?? "message :" + e.Message }");

        }
        // Ejecutar la subida de archivo
    }

}