using Files.Interfaces;
using Files.Models;
using System.Text.Json;
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

    public async Task<UploadFileResponseDto?> UploadFile(Guid id)
    {
        //Check if file is not currently in db
        var fileInDb = await _repository.GetFileById(id);
        var fileInStorage = await _storageService.CheckIfExistsItem(id.ToString());
        //Todo: log this
        if(fileInDb != null){
            throw new ArgumentException(@"Current file Id is not available to upload,
             all chunks with fileId "+id+" try with another fileId");
        }
        if(fileInStorage){
            throw new ArgumentException(@"Current file Id is not available to upload,
             all chunks with fileId "+id+" try with another fileId");
        }
        //Todo: We shoul validate if private of not, not only use first chunk
        var chunksWithFileId = await _repository.GetChunksOrderedByFileIdAsync(id);
        var chunkWithData = chunksWithFileId.First();
        if(chunksWithFileId.Count == 0)
            return null;
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        if(fileString.Length != sum || fileString.Length != chunkWithData.FileSize)
            throw new FormatException("Sum of chunks saved is not the same that total size file parameter");
        //save file
        try{
            string? urlOfElement = null;
            if(chunkWithData.PublicFile)
                urlOfElement = await _storageService.UploadPublicFile(id.ToString(), fileString);
            else{
                await _storageService.UploadFile(id.ToString(), fileString);
            }
            var file = new Files.Models.File(id, DateTime.UtcNow, urlOfElement!, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            var fileSaved = _repository.AddFile(file);
            if(fileSaved is not null)
                _repository.DeleteChunksByFileId(id);
            return file.ToUploadFileResponseDto("File uploaded successfully");

        }
        catch(Exception err)
        {
            throw new ArgumentException($"Failed at upload file with id {id} error : {err}");
        }
    }

    public async Task<UploadChunkResponseDto> UploadPrivateChunck(UploadChunkRequestDto chunkRequestDto)
    {
        var chunkToUpload = Chunk.CreateFromDto(chunkRequestDto, false);
        var uploadSucess = await _repository.UploadTemporalyChunk(chunkRequestDto, false);
        return new UploadChunkResponseDto(chunkToUpload.Id, uploadSucess?"sucess":"failed at upload chunk");
    }

    public async Task<UploadChunkResponseDto> UploadPublicChunck(UploadChunkRequestDto chunkRequestDto)
    {
        var chunkToUpload = Chunk.CreateFromDto(chunkRequestDto, false);
        var uploadSucess = await _repository.UploadTemporalyChunk(chunkRequestDto, true);
        return new UploadChunkResponseDto(chunkToUpload.Id, uploadSucess?"sucess":"failed at upload chunk");
    }

}