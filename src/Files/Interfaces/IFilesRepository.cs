using Files.Models;
namespace Files.Interfaces;  

public interface IFilesRepository
{
    /* Chunks Parts of Interface */
    Task<Chunk> AddChunkAsync(Chunk chunk);
    Task<int> CountAsync();
    Task<List<Chunk>> GetChunksByFileIdAsync(Guid fileId);
    Task<List<Chunk>> GetChunksOrderedByFileIdAsync(Guid fileId);
    Task<bool> UploadTemporalyChunk(UploadChunkRequestDto uploadRequestDto, bool publicChunk);
    string JoinChunks(List<Chunk> chunks);
    Task<string> JoinChunksByFileId(Guid fileId);
    void DeleteChunksByFileId(Guid fileId); 
    /* Files part of interface */
    Task<bool> CheckIfExistFile(Guid id);
    Task<Guid> AddFile(Models.File file);
    Task<Models.File?> GetFileById(Guid id);
}