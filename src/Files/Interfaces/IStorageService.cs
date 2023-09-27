using Amazon.S3.Model;
using Files.Models;
namespace Files.Interfaces;

public interface IStorageService
{
    Task<bool> CheckIfExistsItem(string ItemId);
    /* Return guid as string, or cloud storage conventions */
    //Task<bool> UploadFile(string fileId,string File); }
    Task<string> GetFileById(string Id);
    Task<bool> UploadFile(string fileId,string File); 
    Task<string> UploadPublicFile(string fileId,string File); 
    Task<TResult> UploadChunked<T, TResult>(T dto) where T : ChunkedUploadDto where TResult : ChunkedUploadRes;

}