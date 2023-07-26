using Files.Interfaces;
using Files.Models;
namespace Files.Services;
public class FilesService : IFiles
{
    private readonly IStorageService _storageService;
    private readonly IFilesRepository _repository;
    private readonly ILogger _logger;
    public FilesService(IStorageService storageService, IFilesRepository repository, ILogger<IFiles> logger)
    {
        _storageService = storageService;
        _repository = repository;
        _logger = logger;
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
        List<Chunk> chunksWithFileId = await GetChunksForFileId(id);
        var chunkWithData = chunksWithFileId.First();
        if (chunksWithFileId.Count == 0)
            throw new ArgumentNullException($"Not chunks available for id : {id}");
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        var expectedBytesSize = ((fileString.Length/(4))*3);
        var errorRange = ((expectedBytesSize*0.0001)<2)?2:expectedBytesSize*0.0001;
        if (Math.Abs(expectedBytesSize - sum) > errorRange || Math.Abs(expectedBytesSize - chunkWithData.FileSize) > errorRange )
            throw new FormatException("Sum of chunks saved is greather that fileSize of sum of chunks");
        //save file
        try
        {
            await _storageService.UploadFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, null!, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            var fileSaved = await _repository.AddFile(file);
            _repository.DeleteChunksByFileId(id);
            LogInfo(file);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            LogError(err);
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
        List<Chunk> chunksWithFileId = await GetChunksForFileId(id);
        var chunkWithData = chunksWithFileId.First();
        if (chunksWithFileId.Count == 0)
            return null;
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        var expectedBytesSize = ((int)(fileString.Length/4)*3);
        var errorRange = ((expectedBytesSize*0.0001)<2)?2:expectedBytesSize*0.0001;
        if (Math.Abs(expectedBytesSize - sum) > errorRange || Math.Abs(expectedBytesSize - chunkWithData.FileSize) > errorRange )
            throw new FormatException("Sum of chunks saved is greather that fileSize of sum of chunks");
        //save file
        try
        {
            var urlOfElement = await _storageService.UploadPublicFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, urlOfElement, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            var fileSaved = await _repository.AddFile(file);
            _repository.DeleteChunksByFileId(id);
            LogInfo(file);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            LogError(err);
            throw new ArgumentException($"Failed at upload file with id {id} error : {err}");
        }
    }
    private async Task<List<Chunk>> GetChunksForFileId(Guid id)
    {
        //Check if file is not currently in db
        var fileInDb = await _repository.GetFileById(id);
        var fileInStorage = await _storageService.CheckIfExistsItem(id.ToString());
        //Todo: log this
        string errorMessage;
        if (fileInDb != null)
        {
            errorMessage = $"Current fileId {id} is not available to use, use another id for chunks";
            LogError(new ArgumentException("Duplicated Key : " + errorMessage));
            throw new ArgumentException(errorMessage);
        }
        if (fileInStorage)
        {
            errorMessage = $"Current fileId :{id} is not available to use, use another id for chunks";
            LogError(new ArgumentException("Duplicated Key : " + errorMessage));
            throw new ArgumentException(errorMessage);
        }
        //Todo: We shoul validate if private of not, not only use first chunk
        var chunksWithFileId = await _repository.GetChunksOrderedByFileIdAsync(id);
        return chunksWithFileId;
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
    private void LogInfo(Files.Models.File fileUploaded)
    {
        _logger.LogInformation("Uploaded {@File} on {Date} -Server- ", fileUploaded, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
    }
}