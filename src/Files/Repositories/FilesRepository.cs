using Files.Models;
using Files.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Files.Repositories;

public class FilesRepository : IFilesRepository
{
    private readonly FilesDbContext _dbContext;
    public FilesRepository(FilesDbContext dbContext) => _dbContext = dbContext;
    public async Task<List<Chunk>> GetChunksByFileIdAsync(Guid fileId) => 
    await _dbContext.Chunks.Where(c => c.FileId == fileId).ToListAsync();
    public async Task<List<Chunk>> GetChunksOrderedByFileIdAsync(Guid fileId){
        var chunks = _dbContext.Chunks.Where(c => c.FileId == fileId);
        if(chunks.Count() == 0){
            throw new KeyNotFoundException($"Not chunks available for fileId : {fileId}");
        }
        return await chunks.OrderBy(c => c.Number).ToListAsync();
    }
    


    public async Task<Chunk> AddChunkAsync(Chunk chunk)
    {
        _dbContext.Chunks.Add(chunk);
        await _dbContext.SaveChangesAsync();
        return chunk;
    }


    public void DeleteChunksByFileId(Guid fileId)
    {
        var chunksToDelete = _dbContext.Chunks.Where<Chunk>(chunk => chunk.FileId.Equals(fileId));
        _dbContext.Chunks.RemoveRange(chunksToDelete);
        _dbContext.SaveChanges();   
    }
    public string JoinChunks(List<Chunk> chunks)
    {
        string fullyFile = "";
        chunks.ForEach(chunk => fullyFile += chunk.Data);
        //Console.WriteLine(fullyFile);
        return fullyFile;
    }
    public async Task<string> JoinChunksByFileId(Guid fileId)
    {
        string fullyFile = "";
        var totalChunks = await _dbContext.Chunks.
            Where(chunk => chunk.FileId.Equals(fileId)).
            OrderBy(chunk => chunk.Number).
            ToListAsync();
        totalChunks.ForEach(chunk => fullyFile += chunk.Data);
        return fullyFile;
    }

    public async Task<bool> UploadTemporalyChunk(UploadChunkRequestDto uploadRequestDto)
    {   
        var chunckElement= Chunk.CreateFromDto(uploadRequestDto);
        try
        {
            await _dbContext.Chunks.AddAsync(chunckElement);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (System.Exception e)
        {
/*             Console.WriteLine("=================");
            Console.WriteLine(e.InnerException);
            Console.WriteLine("================="); */
            throw new ArgumentException($"Error with chunk for fileId {uploadRequestDto.FileId}, error {e.Message}");
        }
    }

    public Task<bool> CheckIfExistFile(Guid id)
    {
        //todo:
        return Task.FromResult(false);
    }

    public async Task<Guid> AddFile(Models.File file)
    {
        await _dbContext.Files.AddAsync(file);
        await _dbContext.SaveChangesAsync();
        return file.Id;
    }

    public async Task<Models.File?> GetFileById(Guid id)
    {
        var file = await _dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
        return file;
    }

    public async Task<FileInfoBasedOnCHunks?> GetChunksInfo(Guid fileId)
    {
        var chunk = await _dbContext.Chunks.
            Where(c =>c.FileId== fileId && c.Number==0).
            Select(c => new{
                c.FileId,
                c.Number, 
                c.Size,
                c.FileSize,
                c.Type,
                c.Filename
            }).FirstOrDefaultAsync();
        if(chunk == null)
            return null;
        FileInfoBasedOnCHunks res = new(chunk!.FileId, chunk.Number, chunk.Size, chunk.FileSize, chunk.Type, chunk.Filename);
        return res;
    }

    public async Task<Chunk?> GetChunksByIndex(Guid fileId, int index)
    {
        var chunk = await _dbContext.Chunks.FirstOrDefaultAsync(c => c.FileId == fileId && c.Number == index);
        return chunk;
    }

    public async Task<List<Chunk>> GetChunksRange(Guid fileId,int initial, int final)
    {
        //Includes initial 
        //Not includes final
        var chunksRange = await _dbContext.Chunks.
        Where(c => c.FileId == fileId && c.Number >= initial && c.Number < final).
        OrderBy(c =>c.Number).ToListAsync();
        if(chunksRange.Count != final-initial)
            throw new ArgumentException($"Incomplete range of chunks between range {initial} - {final}");
        return chunksRange;

    }
}