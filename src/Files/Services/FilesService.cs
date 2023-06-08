using Files.Interfaces;
using Files.Models;
namespace Files.Services;
public class FilesService : IFiles
{
    private readonly IStorageService _storageService;
    private readonly IFilesRepository _repository;
    public FilesService(IStorageService storageService, IFilesRepository repository)
    {
        _storageService = storageService;
        _repository = repository;
    }
    public async Task<GetFileSummaryDto?> GetFileById(Guid id)
    {
        var file = await _repository.GetFileById(id);
        if(file is null)
            return null;
        var fileData = await _storageService.GetFileById(id.ToString());
        return file.ToSummaryDto(fileData);
    }
    //Upload for private
    public async Task<UploadFileResponseDto?> UploadFile(Guid id)
    {
        List<Chunk> chunksWithFileId = await CheckIfExistsFile(id);
        var chunkWithData = chunksWithFileId.First();
        if (chunksWithFileId.Count == 0)
            throw new ArgumentNullException($"Not chunks available for id : {id}");
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        if (fileString.Length != sum || fileString.Length != chunkWithData.FileSize)
            throw new FormatException("Sum of chunks saved is not the same that total size file parameter");
        //save file
        try
        {
            await _storageService.UploadFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, null!, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            var fileSaved = _repository.AddFile(file);
            if (fileSaved is not null)
                _repository.DeleteChunksByFileId(id);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            throw new ArgumentException($"Failed at upload file with id {id} error : {err}");
        }
    }
    public async Task<UploadChunkResponseDto> UploadChunks(UploadChunkRequestDto chunkRequestDto)
    {
        var chunkToUpload = Chunk.CreateFromDto(chunkRequestDto);
        var upload = await _repository.UploadTemporalyChunk(chunkRequestDto);
        return new UploadChunkResponseDto(chunkToUpload.Id, upload  ?"success":"failed at upload chunk");
    }

    public async Task<UploadFileResponseDto?> UploadPublicFile(Guid id)
    {
        List<Chunk> chunksWithFileId = await CheckIfExistsFile(id);
        var chunkWithData = chunksWithFileId.First();
        if (chunksWithFileId.Count == 0)
            return null;
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        if (fileString.Length != sum || fileString.Length != chunkWithData.FileSize)
            throw new FormatException("Sum of chunks saved is not the same that total size file parameter");
        //save file
        try
        {
            var urlOfElement = await _storageService.UploadPublicFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, urlOfElement, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            var fileSaved = _repository.AddFile(file);
            if (fileSaved is not null)
                _repository.DeleteChunksByFileId(id);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            throw new ArgumentException($"Failed at upload file with id {id} error : {err}");
        }
    }
    private async Task<List<Chunk>> CheckIfExistsFile(Guid id)
    {
        //Check if file is not currently in db
        var fileInDb = await _repository.GetFileById(id);
        var fileInStorage = await _storageService.CheckIfExistsItem(id.ToString());
        //Todo: log this
        if (fileInDb != null)
        {
            throw new ArgumentException(@"Current file Id is not available to upload,
             all chunks with fileId " + id + " try with another fileId");
        }
        if (fileInStorage)
        {
            throw new ArgumentException(@"Current file Id is not available to upload,
            already in id :" + id + " try with another fileId");
        }
        //Todo: We shoul validate if private of not, not only use first chunk
        var chunksWithFileId = await _repository.GetChunksOrderedByFileIdAsync(id);
        return chunksWithFileId;
    }
}