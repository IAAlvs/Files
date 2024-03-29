using Amazon.S3.Model;
using Files.Interfaces;
using Files.Models;
namespace Files.Services;
public class FilesService : IFiles
{
    private readonly IStorageService _storageService;
    private readonly IFilesRepository _repository;
    private readonly ILogger _logger;
    private readonly IVerifyChunk _ruleCalculator;

    public FilesService(IStorageService storageService, IFilesRepository repository, ILogger<IFiles> logger, IVerifyChunk verifyChunk)
    {
        _storageService = storageService;
        _repository = repository;
        _ruleCalculator = verifyChunk;
        _logger = logger;
    }
    public async Task<GetFileSummaryDto?> GetFileById(Guid id)
    {
        var file = await _repository.GetFileById(id);
        if (file is null)
            return null;
        // If file has a Url saved in db it means is public */
        int temporalyUrlMinutes = _ruleCalculator.UrlAvailabilityBasedOnSize(file.Size);
        // Files greater than 5 mb neeeds an url
        var fileUrl = file.Url ?? await _storageService.GetTemporalyUrlByFileId(file.Id.ToString(), temporalyUrlMinutes);
        return file.ToSummaryUrlDto(fileUrl);
    }

    private async Task<UploadFileResponseDto> UploadChunked(FileInfoBasedOnCHunks info){
        try{
            int numOfChunks = Convert.ToInt32(info.FileSize/info.Size);
            var remainder = info.FileSize%info.Size;
            if(remainder>0) 
                ++numOfChunks;
            var calculateIterations = _ruleCalculator.ChunksByRequest(info.FileSize, info.Size);
            //Console.WriteLine($"Iterations {calculateIterations.iteration} number of chunks size {calculateIterations.numberOfChunks}");
            //var etagList = 
            ChunkedUploadDtoAws dtoChunked = new()
            {
                Etags = new List<PartETag>(),
                UploadId = ""
            };
            for (int i = 0; i < calculateIterations.iteration; i++)
            {   
                List<Chunk> listC;
                dtoChunked.FileId = info.FileId.ToString();
                dtoChunked.TotalChunks = calculateIterations.iteration;
                dtoChunked.PartNumber = i+1;
                //Las iteration is the biggest
                if(i == calculateIterations.iteration-1)
                    listC = await _repository.GetChunksRange(info.FileId, i * calculateIterations.numberOfChunks, numOfChunks -1);
                else
                    listC = await _repository.GetChunksRange(info.FileId, i * calculateIterations.numberOfChunks, (i+1) * calculateIterations.numberOfChunks );
                var chunk = await _repository.GetChunksByIndex(info.FileId, i)?? 
                throw new ArgumentException($"Failed to read chunk index {i} of fileId {info.FileId}");
                var base64Chunk = _repository.JoinChunks(listC);
                dtoChunked.Base64Chunk = base64Chunk;
                //= new ChunkedUploadDtoAws(base64Chunk, info.FileId.ToString(), calculateIterations.iteration, i+1, etagList, uploadId);
                //Console.WriteLine($"UploadId {dtoChunked.FileId}");
                //Console.WriteLine($"UploadId {dtoChunked.PartNumber}");

                var uploaded = await _storageService.UploadChunked<ChunkedUploadDtoAws, ChunkedUploadResAws>(dtoChunked);
                dtoChunked.Etags = uploaded.ETags;
                dtoChunked.UploadId = uploaded.UploadId;
                if(dtoChunked.UploadId == "")
                    throw new ArgumentException($"Failed to upload chunk index {i} of file {info.FileId}");
                
            }
            var file = new Models.File(info.FileId, DateTime.UtcNow, null!, info.Type, info.FileName, info.FileSize);
            var fileSaved = await _repository.AddFile(file);
            LogInfo(file);
            _repository.DeleteChunksByFileId(info.FileId);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            //Console.WriteLine(err);
            LogError(err);
            throw new ArgumentException($"Failed at upload file with id {info.FileId} error : {err}");
        }

    }
    //Todo : TEST UPLOAD WHEN FILE IS GREATER THAN 20 MB

    public async Task<UploadFileResponseDto?> UploadFile(Guid id)
    {
        var chunkWithData = await _repository.GetChunksInfo(id) ??  throw new ArgumentNullException($"File for id {id} was not found");
        if (_ruleCalculator.BytesToMb(chunkWithData.FileSize)>20){
            return await UploadChunked(chunkWithData);
        }
        List<Chunk> chunksWithFileId = await GetChunksForFileId(id);
        if (chunksWithFileId.Count == 0)
            throw new ArgumentNullException($"Not chunks available for id : {id}");
        var fileString = _repository.JoinChunks(chunksWithFileId);
        var sum = 0;
        chunksWithFileId.ForEach(c => sum += c.Size);
        try
        {
            await _storageService.UploadFile(id.ToString(), fileString);
            var file = new Files.Models.File(id, DateTime.UtcNow, null!, chunkWithData.Type, chunkWithData.FileName, chunkWithData.FileSize);
            var fileSaved = await _repository.AddFile(file);
            _repository.DeleteChunksByFileId(id);
            LogInfo(file);
            return file.ToUploadFileResponseDto("File uploaded successfully");
        }
        catch (Exception err)
        {
            //Console.WriteLine(err);
            LogError(err);
            throw new ArgumentException($"Failed at upload file with id {id} error : {err}");
        }
    }
    public async Task<UploadChunkResponseDto> UploadChunks(UploadChunkRequestDto chunkRequestDto)
    {
        var chunkToUpload = Chunk.CreateFromDto(chunkRequestDto);
        var expectedBytesSize = _ruleCalculator.CalculateBytesBasedOnBase64(chunkRequestDto.Data);
        _ruleCalculator.CheckDividedChunkErrorSize(expectedBytesSize, chunkRequestDto.Size);
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
        var expectedBytesSize = _ruleCalculator.CalculateBytesBasedOnBase64(fileString);
        _ruleCalculator.CheckDividedChunkErrorSize(expectedBytesSize, chunkWithData.Size);
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