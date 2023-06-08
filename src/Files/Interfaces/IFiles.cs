using Files.Models;
namespace Files.Interfaces;

public interface IFiles
{
    Task<GetFileSummaryDto?> GetFileById(Guid Id); 
    Task<UploadChunkResponseDto> UploadChunks(UploadChunkRequestDto chunkRequestDto);
    Task<UploadFileResponseDto?> UploadFile(Guid Id);
    Task<UploadFileResponseDto?> UploadPublicFile(Guid Id);

}