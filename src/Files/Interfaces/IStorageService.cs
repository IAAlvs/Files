namespace Files.Interfaces;

public interface IStorageService
{
    Task<bool> CheckIfExistsItem(string ItemId);
    /* Return guid as string, or cloud storage conventions */
    Task<string> UploadFile(string fileId,string File); 
    Task<string> UploadPublicFile(string fileId,string File); 
    Task<string> GetFileById(string Id);
}