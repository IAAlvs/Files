using Files.Interfaces;
using Files.Models;

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
        //Todo: log this
        if(fileInDb != null){
            throw new ArgumentException(@"Current file Id is not available to upload,
             all chunks with fileId "+id+" have been deleted try with other id");
        }
        var chunksWithFileId = await _repository.GetChunksByFileIdAsync(id);
        var chunkWithData = chunksWithFileId.First();
        if(chunksWithFileId.Count == 0)
            return null;
        var fileString = _repository.JoinChunks(chunksWithFileId);
        int sum = 0;
        chunksWithFileId.Select(c => sum += c.Size);
        if(fileString.Length != sum || fileString.Length != chunkWithData.FileSize)
            throw new FormatException("Sum of chunks saved is not the same that total size file parameter");
        //save file
        try{
            var urlOfElement = await _storageService.UploadFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, urlOfElement, chunkWithData.Type, chunkWithData.Filename, chunkWithData.FileSize);
            return file.ToUploadFileResponseDto("File uploaded Successfully");
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