using Files.Interfaces;

public class StorageService : IStorageService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;
    private readonly string _urlDomain;
    private readonly string _storageKey;

    public StorageService(IConfiguration config, HttpClient client)
    {
        _client = client;
        _config = config;
        _urlDomain = config.GetConnectionString("UrlDomain")!;
        _storageKey = config.GetConnectionString("StorageKey")!;
    }

    public Task<bool> CheckIfExistsItem(string Item)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetFileById(string Id)
    {
        return Task.FromResult("file mocking");
    }

    public Task<string> UploadFile(string File)
    {
        return Task.FromResult("31c430ed-c7e0-4a67-8d26-a929d3e7cd4d");
    }

    public Task<string> UploadFile(string fileId, string File)
    {
        throw new NotImplementedException();
    }

    public Task<string> UploadPublicFile(string fileId, string File)
    {
        throw new NotImplementedException();
    }
}